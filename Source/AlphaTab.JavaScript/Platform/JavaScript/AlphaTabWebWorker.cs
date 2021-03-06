/*
 * This file is part of alphaTab.
 * Copyright � 2018, Daniel Kuschny and Contributors, All rights reserved.
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3.0 of the License, or at your option any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.
 */
using System;
using AlphaTab.Haxe;
using AlphaTab.Haxe.Js;
using AlphaTab.Haxe.Js.Html;
using AlphaTab.Model;
using AlphaTab.Rendering;
using AlphaTab.Util;

namespace AlphaTab.Platform.JavaScript
{
    class AlphaTabWebWorker
    {
        private ScoreRenderer _renderer;
        private readonly DedicatedWorkerGlobalScope _main;

        public AlphaTabWebWorker(DedicatedWorkerGlobalScope main)
        {
            _main = main;
            _main.AddEventListener("message", (Action<Event>)HandleMessage, false);
        }

        public static void Init()
        {
            new AlphaTabWebWorker(Lib.Global);
        }

        private void HandleMessage(Event e)
        {
            var data = ((MessageEvent)e).Data;
            var cmd = data ? data.cmd : "";
            switch (cmd)
            {
                case "alphaTab.initialize":
                    Settings settings = Settings.FromJson(data.settings, null);
                    Logger.LogLevel = settings.LogLevel;
                    _renderer = new ScoreRenderer(settings);
                    _renderer.PartialRenderFinished += result => _main.PostMessage(new { cmd = "alphaTab.partialRenderFinished", result = result });
                    _renderer.RenderFinished += result => _main.PostMessage(new { cmd = "alphaTab.renderFinished", result = result });
                    _renderer.PostRenderFinished += () => _main.PostMessage(new { cmd = "alphaTab.postRenderFinished", boundsLookup = _renderer.BoundsLookup.ToJson() });
                    _renderer.PreRender += () => _main.PostMessage(new { cmd = "alphaTab.preRender" });
                    _renderer.Error += Error;
                    break;
                case "alphaTab.invalidate":
                    _renderer.Invalidate();
                    break;
                case "alphaTab.resize":
                    _renderer.Resize(data.width);
                    break;
                case "alphaTab.render":
                    var score = JsonConverter.JsObjectToScore(data.score, _renderer.Settings);
                    RenderMultiple(score, data.trackIndexes);
                    break;
                case "alphaTab.updateSettings":
                    UpdateSettings(data.settings);
                    break;
            }
        }

        private void UpdateSettings(object settings)
        {
            _renderer.UpdateSettings(Settings.FromJson(settings, null));
        }

        private void RenderMultiple(Score score, int[] trackIndexes)
        {
            try
            {
                _renderer.Render(score, trackIndexes);
            }
            catch (Exception e)
            {
                Error("render", e);
            }
        }

        private void Error(string type, Exception e)
        {
            Logger.Error(type, "An unexpected error occurred in worker", e);

            dynamic error = Json.Parse(Json.Stringify(e));

            dynamic e2 = e;

            if (e2.message)
            {
                error.message = e2.message;
            }
            if (e2.stack)
            {
                error.stack = e2.stack;
            }
            if (e2.constructor && e2.constructor.name)
            {
                error.type = e2.constructor.name;
            }
            _main.PostMessage(new { cmd = "alphaTab.error", error = new { type = type, detail = error } });
        }
    }
}
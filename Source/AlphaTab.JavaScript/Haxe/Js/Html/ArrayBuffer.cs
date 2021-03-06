﻿/*
 * This file is part of alphaTab.
 * Copyright © 2018, Daniel Kuschny and Contributors, All rights reserved.
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
using Phase.Attributes;

namespace Haxe.Js.Html
{
    [External]
    [Name("js.html.ArrayBuffer")]
    public class ArrayBuffer
    {
        [Name("isView")]
        public static extern HaxeBool IsView(object value);

        [Name("byteLength")]
        public HaxeInt ByteLength { get; }

        public extern ArrayBuffer(HaxeInt length);

        [Name("slice")]
        public extern ArrayBuffer Slice(HaxeInt begin);

        [Name("slice")]
        public extern ArrayBuffer Slice(HaxeInt begin, HaxeInt end);
    }
}
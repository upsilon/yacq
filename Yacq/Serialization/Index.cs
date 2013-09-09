﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
 * All rights reserved.
 * 
 * This file is part of YACQ.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class Index
        : Node
    {
        [DataMember(Order = 0)]
        public Node Object
        {
            get;
            set;
        }

        [DataMember(Order = 1)]
        public PropertyRef Indexer
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        public Node[] Arguments
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return Expression.MakeIndex(
                this.Object.Deserialize(),
                this.Indexer.Null(p => p.Deserialize()),
                this.Arguments.SelectAll(n => n.Deserialize())
            );
        }

        public override String ToString()
        {
            return (this.Object.Null(n => n.ToString())
                ?? this.Object.Type.Describe().ToString()
            )
                + this.Indexer.Null(p => "." + p, "")
                + "[" + this.Arguments.Stringify(", ") + "]";
        }
    }

    partial class Node
    {
        internal static Index Index(IndexExpression expression)
        {
            return new Index()
            {
                Object = Serialize(expression.Object),
                Indexer = expression.Indexer.Null(p => PropertyRef.Serialize(p)),
                Arguments = expression.Arguments.Select(Serialize).ToArray(),
            }.Apply(n => n.Type = TypeRef.Serialize(expression.Type));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:

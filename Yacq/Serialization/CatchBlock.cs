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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using E = System.Linq.Expressions;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class CatchBlock
    {
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef Test
        {
            get;
            set;
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public Parameter Variable
        {
            get;
            set;
        }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public Node Filter
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        public Node Body
        {
            get;
            set;
        }

        internal static CatchBlock Serialize(E.CatchBlock handler)
        {
            return new CatchBlock()
            {
                Test = handler.Variable == null
                    ? TypeRef.Serialize(handler.Test)
                    : null,
                Variable = handler.Variable.Null(v => Node.Parameter(v)),
                Filter = handler.Filter.Null(e => Node.Serialize(e)),
                Body = Node.Serialize(handler.Body),
            };
        }

        public override String ToString()
        {
            return "catch (" + (this.Variable != null
                ? this.Variable.ToString()
                : this.Test.ToString()
            ) + ") {" + (this.Body != null
                ? " " + this.Body + " }"
                : "}"
            );
        }

        internal E.CatchBlock Deserialize()
        {
            return this.Variable != null
                ? E.Expression.Catch(
                      this.Variable.Deserialize<E.ParameterExpression>(),
                      this.Body.Deserialize(),
                      this.Filter.Null(n => n.Deserialize())
                  )
                : E.Expression.Catch(
                      this.Test.Deserialize(),
                      this.Body.Deserialize(),
                      this.Filter.Null(n => n.Deserialize())
                  );
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:

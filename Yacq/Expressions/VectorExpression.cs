﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    public class VectorExpression
        : YacqExpression
    {
        public ReadOnlyCollection<Expression> Elements
        {
            get;
            private set;
        }

        public Expression this[Int32 index]
        {
            get
            {
                return this.Elements[index];
            }
        }

        internal VectorExpression(
            SymbolTable symbols,
            IList<Expression> elements
        )
            : base(symbols)
        {
            this.Elements = new ReadOnlyCollection<Expression>(elements);
        }

        public override String ToString()
        {
            return "[" + String.Join(" ", this.Elements.Select(e => e.ToString())) + "]";
        }

        protected override Expression ReduceImpl(SymbolTable symbols)
        {
            return this.Elements.ReduceAll(symbols)
                .Let(es => es
                    .Select(e => e.Type)
                    .Distinct()
                    .Let(ts => ts
                        .SelectMany(t => t.GetConvertibleTypes())
                        .Distinct()
                        .Except(EnumerableEx.Return(typeof(Object)))
                        .OrderByDescending(t => EnumerableEx
                            .Generate(t, _ => _.BaseType != null, _ => _.BaseType, _ => _)
                            .Count()
                        )
                        .Concat(EnumerableEx.Return(typeof(Object)))
                        .First(t => ts.All(t.IsAssignableFrom))
                    )
                    .Let(t => NewArrayInit(
                        t,
                        t.IsValueType
                            ? es
                            : es.Select(e => e.Type.IsValueType
                                  ? Convert(e, t)
                                  : e
                              )
                    ))
                );
        }
    }

    partial class YacqExpression
    {
        public static VectorExpression Vector(SymbolTable symbols, params Expression[] elements)
        {
            return new VectorExpression(symbols, elements);
        }

        public static VectorExpression Vector(SymbolTable symbols, IEnumerable<Expression> elements)
        {
            return Vector(symbols, elements.ToArray());
        }

        public static VectorExpression Vector(params Expression[] elements)
        {
            return Vector(null, elements);
        }

        public static VectorExpression Vector(IEnumerable<Expression> elements)
        {
            return Vector(null, elements.ToArray());
        }
    }
}

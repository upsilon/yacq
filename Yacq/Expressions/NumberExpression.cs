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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    public class NumberExpression
        : YacqExpression
    {
        public Char QuoteChar
        {
            get;
            private set;
        }

        public String SourceText
        {
            get;
            private set;
        }

        public Object Value
        {
            get;
            private set;
        }

        internal NumberExpression(
            SymbolTable symbols,
            String text
        )
            : base(symbols)
        {
            this.SourceText = text;
            this.Value = this.Parse();
        }

        public override String ToString()
        {
            return this.SourceText;
        }

        protected override Expression ReduceImpl(SymbolTable symbols)
        {
            return Constant(this.Value);
        }

        private Object Parse()
        {
            var text = this.SourceText.Replace("_", "").ToLower();
            if (text.Contains("."))
            {
                return text.Last() == 'f'
                    ? Single.Parse(text.Remove(text.Length - 1), NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture)
                    : Double.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            else
            {
                if (text[0] != '-')
                {
                    if (text[0] == '+')
                    {
                        text = text.Substring(1);
                    }
                    var b = text.Length > 2
                        ? GetBase(text.Substring(0, 2))
                        : 10;
                    var value = b != 10
                        ? System.Convert.ToUInt64(text.Substring(2), b)
                        : UInt64.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture);
                    return value <= Int32.MaxValue
                        ? (Int32) value
                        : value <= UInt32.MaxValue
                              ? (UInt32) value
                              : (Object) value;
                }
                else
                {
                    var b = text.Length > 3
                        ? GetBase(text.Substring(1, 3))
                        : 10;
                    var value = b != 10
                        ? System.Convert.ToInt64("-" + text.Substring(3), b)
                        : Int64.Parse(text, CultureInfo.InvariantCulture);
                    return value >= Int32.MinValue && value <= Int32.MaxValue
                        ? (Int32) value
                        : value;
                }
            }
        }

        private static Int32 GetBase(String b)
        {
            return b == "0b"
                ? 2
                : b == "0o"
                      ? 8
                      : b == "0x"
                            ? 16
                            : 10;
        }
    }

    partial class YacqExpression
    {
        public static NumberExpression Number(SymbolTable symbols, String text)
        {
            return new NumberExpression(symbols, text);
        }

        public static NumberExpression Number(String text)
        {
            return Number(null, text);
        }
    }
}

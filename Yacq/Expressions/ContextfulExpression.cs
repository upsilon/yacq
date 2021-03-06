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
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression that should be evaluated on special context.
    /// </summary>
    public class ContextfulExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets an <see cref="Expression"/> which is evaluated on special context.
        /// </summary>
        /// <value>An <see cref="Expression"/> which is evaluated on special context.</value>
        public Expression Expression
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of evaluating context.
        /// </summary>
        /// <value>The type of evaluating context.</value>
        public ContextType ContextType
        {
            get;
            private set;
        }

        internal ContextfulExpression(
            SymbolTable symbols,
            Expression expression,
            ContextType contextType
        )
            : base(symbols)
        {
            this.Expression = expression;
            this.ContextType = contextType;
            this.SetPosition(expression);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return "(" + this.ContextType + " " + this.Expression + ")";
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            switch (this.ContextType)
            {
                case ContextType.Default:
                case ContextType.Dynamic:
                    return this.Expression;
                default:
                    throw new ArgumentOutOfRangeException("this.ContextType");
            }
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="ContextfulExpression"/> that represents the expression which is evaluated on special context.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="expression">The expression to evaluate on special context.</param>
        /// <param name="contextType">The type of evaluating context.</param>
        /// <returns>An <see cref="ContextfulExpression"/>.</returns>
        public static ContextfulExpression Contextful(SymbolTable symbols, Expression expression, ContextType contextType)
        {
            return new ContextfulExpression(symbols, expression, contextType);
        }

        /// <summary>
        /// Creates a <see cref="ContextfulExpression"/> that represents the expression which is evaluated on special context.
        /// </summary>
        /// <param name="expression">The expression to evaluate on special context.</param>
        /// <param name="contextType">The type of evaluating context.</param>
        /// <returns>An <see cref="ContextfulExpression"/>.</returns>
        public static ContextfulExpression Contextful(Expression expression, ContextType contextType)
        {
            return Contextful(null, expression, contextType);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:

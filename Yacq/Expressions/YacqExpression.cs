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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that represent YACQ expression tree nodes are derived.
    /// It also contains static factory methods to create the various node types. This is an abstract class.
    /// </summary>
    public abstract partial class YacqExpression
        : Expression
    {
        private Boolean _canReduce;

        private Expression _reducedExpression;

        /// <summary>
        /// Gets the node type of this expression.
        /// </summary>
        /// <returns>One of the <see cref="ExpressionType"/> values.</returns>
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        /// <returns><c>true</c> if the node can be reduced, otherwise <c>false</c>.</returns>
        public override Boolean CanReduce
        {
            get
            {
                return this._canReduce;
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this expression represents.
        /// </summary>
        /// <returns>The <see cref="System.Type"/> that represents the static type of the expression.</returns>
        public override Type Type
        {
            get
            {
                if (this._reducedExpression == null && this.CanReduce)
                {
                    this.Reduce();
                }
                return this._reducedExpression != null && this._reducedExpression != this
                    ? this._reducedExpression.Type
                    : null;
            }
        }

        /// <summary>
        /// Gets the symbol table linked with this expression.
        /// </summary>
        /// <value>
        /// The symbol table linked with this expression.
        /// </value>
        public SymbolTable Symbols
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="YacqExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table linked with this expression.</param>
        protected YacqExpression(SymbolTable symbols)
        {
            this._canReduce = true;
            this.Symbols = symbols ?? new SymbolTable();
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If <see cref="CanReduce"/> returns <c>true</c>, this should return a valid expression.
        /// This method can return another node which itself must be reduced.
        /// </summary>
        /// <returns>
        /// The reduced expression.
        /// </returns>
        public override Expression Reduce()
        {
            return this.Reduce(null);
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables. Reducing is continued while the reduced expression is not <see cref="YacqExpression"/>.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <returns>The reduced expression.</returns>
        public Expression Reduce(SymbolTable symbols)
        {
            symbols = new SymbolTable(this.Symbols.Parent, symbols != null
                ? this.Symbols
                      .Except(symbols.Flatten)
                      .Concat(symbols.Flatten)
                      .ToDictionary(p => p.Key, p => p.Value)
                : null
            );
            if (symbols.All(p => this.Symbols.ContainsKey(p.Key) && this.Symbols[p.Key] == p.Value))
            {
                if (this._reducedExpression == null && this.CanReduce)
                {
                    this._reducedExpression = this.ReduceImpl(symbols) ?? this;
                    if (this._reducedExpression != this && this._reducedExpression is YacqExpression)
                    {
                        this._reducedExpression = this._reducedExpression.Reduce(symbols);
                    }
                }
                this._canReduce = false;
                return this._reducedExpression;
            }
            else
            {
                var expression = this.ReduceImpl(symbols) ?? this;
                if (expression != this && expression is YacqExpression)
                {
                    expression = expression.Reduce(symbols);
                }
                return expression;
            }
        }

        /// <summary>
        /// When implemented in a derived class, reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <returns>The reduced expression.</returns>
        protected abstract Expression ReduceImpl(SymbolTable symbols);
    }
}

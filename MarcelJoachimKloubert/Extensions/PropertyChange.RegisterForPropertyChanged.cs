/**********************************************************************************************************************
 * Extensions.NET (https://github.com/mkloubert/Extensions.NET)                                                       *
 *                                                                                                                    *
 * Copyright (c) 2015, Marcel Joachim Kloubert <marcel.kloubert@gmx.net>                                              *
 * All rights reserved.                                                                                               *
 *                                                                                                                    *
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the   *
 * following conditions are met:                                                                                      *
 *                                                                                                                    *
 * 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the          *
 *    following disclaimer.                                                                                           *
 *                                                                                                                    *
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the       *
 *    following disclaimer in the documentation and/or other materials provided with the distribution.                *
 *                                                                                                                    *
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote    *
 *    products derived from this software without specific prior written permission.                                  *
 *                                                                                                                    *
 *                                                                                                                    *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, *
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE  *
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, *
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR    *
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,  *
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE   *
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                                           *
 *                                                                                                                    *
 **********************************************************************************************************************/

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace MarcelJoachimKloubert.Extensions
{
    // RegisterForPropertyChanged()
    static partial class MJKNotificationExtensionMethods
    {
        #region Methods (3)

        /// <summary>
        /// Registers an action for an <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <typeparam name="TObj">Type of the object.</typeparam>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="action">The action to register.</param>
        /// <param name="expr">The expression that provides the property name.</param>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> contains no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj" />, <paramref name="action" /> and/or <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Body of <paramref name="expr" /> contains no <see cref="PropertyInfo" />.
        /// </exception>
        public static IPropertyChangeContext<TObj, TProperty> RegisterForPropertyChanged<TObj, TProperty>(this TObj obj,
                                                                                                          Action<IPropertyChangeContext<TObj, TProperty>> action,
                                                                                                          Expression<Func<TObj, TProperty>> expr)
            where TObj : global::System.ComponentModel.INotifyPropertyChanged
        {
            if (expr == null)
            {
                throw new ArgumentNullException("expr");
            }

            var memberExpr = expr.Body as MemberExpression;
            if (memberExpr == null)
            {
                throw new ArgumentException("expr");
            }

            var property = (PropertyInfo)memberExpr.Member;

            return RegisterForPropertyChanged<TObj, TProperty>(obj: obj,
                                                               action: action,
                                                               property: property);
        }

        /// <summary>
        /// Registers an action for an <see cref="INotifyPropertyChanged.PropertyChanged" /> event.
        /// </summary>
        /// <typeparam name="TObj">Type of the object.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="action">The action to register.</param>
        /// <param name="propertyName">The property name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj" />, <paramref name="action" /> and/or <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// No property found with the name stored in <paramref name="propertyName" />.
        /// </exception>
        public static IPropertyChangeContext<TObj> RegisterForPropertyChanged<TObj>(this TObj obj,
                                                                                    Action<IPropertyChangeContext<TObj>> action,
                                                                                    string propertyName)
            where TObj : global::System.ComponentModel.INotifyPropertyChanged
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            propertyName = propertyName.Trim();
            if (propertyName == string.Empty)
            {
                throw new ArgumentException("propertyName");
            }

            var property = typeof(TObj).GetProperty(propertyName,
                                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                throw new MissingMemberException();
            }

            return RegisterForPropertyChanged<TObj, object>(obj: obj,
                                                            action: action,
                                                            property: property);
        }

        private static PropertyChangeContext<TObj, TProperty> RegisterForPropertyChanged<TObj, TProperty>(this TObj obj,
                                                                                                          Action<IPropertyChangeContext<TObj, TProperty>> action,
                                                                                                          PropertyInfo property)
            where TObj : global::System.ComponentModel.INotifyPropertyChanged
        {
            var result = new PropertyChangeContext<TObj, TProperty>()
                {
                    Object = obj,
                    Property = property,
                };

            result.Action = action;

            return result;
        }

        #endregion Methods (3)
    }
}
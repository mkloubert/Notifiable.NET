﻿/**********************************************************************************************************************
 * Notifiable.NET (https://github.com/mkloubert/Notifiable.NET)                                                       *
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MarcelJoachimKloubert.ComponentModel
{
    /// <summary>
    /// A basic notifiable object.
    /// </summary>
    public abstract partial class NotifiableBase : MarshalByRefObject, INotifyPropertyChanged
    {
        #region Fields (2)

        private readonly IDictionary<string, object> _PROPERTIES;

        #endregion Fields (2)

        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableBase" /> class.
        /// </summary>
        /// <param name="syncRoot">The custom object for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        protected NotifiableBase(object syncRoot = null)
        {
            SyncRoot = syncRoot ?? new object();

            _PROPERTIES = CreatePropertyStorage() ?? new Dictionary<string, object>();
        }

        #endregion Constructors (1)

        #region Events (1)

        /// <summary>
        /// Is raised on an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged" />
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events (1)

        #region Properties (1)

        /// <summary>
        /// Gets the object for thread safe operations.
        /// </summary>
        public object SyncRoot { get; private set; }

        #endregion Properties (1)

        #region Methods (31)

        /// <summary>
        /// Adds or sets a dictionary value.
        /// </summary>
        /// <typeparam name="TKey">Type of the keys.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <paramref name="value" /> was added to <paramref name="dict" /> (<see langword="true" />)
        /// or set (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="dict" /> is <see langword="null" />.
        /// </returns>
        public static bool? AddOrSet<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict == null)
            {
                return null;
            }

            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return false;
            }

            dict.Add(key, value);
            return true;
        }

        /// <summary>
        /// Returns a sequence as array.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="seq">The sequence.</param>
        /// <returns>
        /// <paramref name="seq" /> as array.
        /// If <paramref name="seq" /> is already an array, it is simply casted.
        /// If <paramref name="seq" /> is <see langword="null" />, a <see langword="null" /> reference will be returned.
        /// </returns>
        public static T[] AsArray<T>(IEnumerable<T> seq)
        {
            if (seq is T[])
            {
                return (T[])seq;
            }

            if (seq == null)
            {
                return null;
            }

            if (seq is List<T>)
            {
                // ToArray() of list object
                return ((List<T>)seq).ToArray();
            }

            // LINQ style
            return seq.ToArray();
        }

        /// <summary>
        /// Returns an object as string.
        /// </summary>
        /// <param name="obj">The input value.</param>
        /// <param name="dbNullAsNull">
        /// Returns <see cref="DBNull" /> as <see langword="null" /> reference or not.
        /// </param>
        /// <returns>The output value.</returns>
        public static string AsString(object obj, bool dbNullAsNull = true)
        {
            if (obj is string)
            {
                return (string)obj;
            }

            if (dbNullAsNull)
            {
                if (DBNull.Value.Equals(obj))
                {
                    obj = null;
                }
            }

            if (obj == null)
            {
                return null;
            }

            if (obj is IEnumerable<char>)
            {
                return new string(AsArray(obj as IEnumerable<char>));
            }

            if (obj is TextReader)
            {
                return ((TextReader)obj).ReadToEnd();
            }

            return obj.ToString();
        }

        private static bool CheckNotifictionAttributeOptions(ReceiveNotificationFromAttribute attrib, bool areDifferent)
        {
            if (areDifferent)
            {
                if (attrib.Options.HasFlag(ReceiveFromOptions.AreDifferent) ||
                    (attrib.Options == ReceiveFromOptions.Default))
                {
                    return true;
                }
            }
            else
            {
                if (attrib.Options.HasFlag(ReceiveFromOptions.AreEqual))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts a property value to a target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="obj">The input value.</param>
        /// <returns>The output value.</returns>
        protected virtual TTarget ConvertPropertyValue<TTarget>(string propertyName, object obj)
        {
            return ConvertTo<TTarget>(obj);
        }

        /// <summary>
        /// Converts an object to a target type.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="obj">The input value.</param>
        /// <returns>The output value.</returns>
        protected virtual TTarget ConvertTo<TTarget>(object obj)
        {
            if (DBNull.Value.Equals(obj))
            {
                obj = null;
            }

            return (TTarget)obj;
        }

        /// <summary>
        /// Creates the inner storage for the property values.
        /// </summary>
        /// <returns>The new instance.</returns>
        protected virtual IDictionary<string, object> CreatePropertyStorage()
        {
            // create defualt instance
            return null;
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(Expression<Func<TProperty>> expr)
        {
            bool found;
            return Get(out found, expr);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="found">Stores if value exists / was found or not.</param>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(out bool found, Expression<Func<TProperty>> expr)
        {
            return Get<TProperty>(propertyName: GetPropertyName(expr),
                                  found: out found);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(string propertyName)
        {
            bool found;
            return Get<TProperty>(propertyName: propertyName,
                                  found: out found);
        }

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="found">Stores if value exists / was found or not.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected TProperty Get<TProperty>(out bool found, string propertyName)
        {
            var pn = NormalizePropertyName(propertyName);

            object temp;
            if (_PROPERTIES.TryGetValue(pn, out temp))
            {
                found = true;
                return ConvertPropertyValue<TProperty>(pn, temp);
            }

            found = false;
            return GetDefaultPropertyValue<TProperty>(pn);
        }

        /// <summary>
        /// Returns the default value for a type.
        /// </summary>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The default value.</returns>
        protected virtual TValue GetDefaultValue<TValue>()
        {
            return default(TValue);
        }

        /// <summary>
        /// Returns the default value for a property.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The default value.</returns>
        protected virtual TProperty GetDefaultPropertyValue<TProperty>(string propertyName)
        {
            return GetDefaultValue<TProperty>();
        }

        /// <summary>
        /// Returns the <see cref="IEqualityComparer{T}" /> for a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <returns>The instance.</returns>
        protected virtual IEqualityComparer<TProperty> GetPropertyValueEqualityComparer<TProperty>(string propertyName)
        {
            // return default
            return null;
        }

        /// <summary>
        /// Returns the name of a property from an expression.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression.</param>
        /// <returns>The name of the property.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> expr)
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

            return ((PropertyInfo)memberExpr.Member).Name;
        }

        private void HandleReceiveNotificationFromAttributes<TProperty>(string propertyName, bool areDifferent)
        {
            var propertiesToNotify =
                GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(x =>
                            {
                                return new
                                {
                                    Attributes = x.GetCustomAttributes(typeof(ReceiveNotificationFromAttribute), false)
                                                  .Cast<ReceiveNotificationFromAttribute>()
                                                  .ToArray(),
                                    Property = x,
                                };
                            })
                    .Select(x =>
                            {
                                var attrib = x.Attributes.SingleOrDefault(y => (y.SenderName ?? string.Empty).Trim() == propertyName);
                                if (attrib == null)
                                {
                                    // no do match
                                    return null;
                                }

                                if (!CheckNotifictionAttributeOptions(attrib, areDifferent))
                                {
                                    return null;
                                }

                                if (x.Property.Name == propertyName)
                                {
                                    // not allowed
                                    return null;
                                }

                                return new
                                {
                                    Attribute = attrib,
                                    Property = x.Property,
                                };
                            })
                    .Where(x => x != null);

            foreach (var property in propertiesToNotify.OrderBy(x => x.Attribute.SortOrder)
                                                       .ThenBy(x => x.Property.Name, StringComparer.Ordinal)
                                                       .Select(x => x.Property.Name)
                                                       .Distinct(StringComparer.Ordinal))
            {
                try
                {
                    RaisePropertyChanged(property);
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
            }
        }

        private void HandleReceiveValueFromAttributes<TProperty>(string propertyName, TProperty oldValue, TProperty newValue, bool areDifferent)
        {
            var membersToNotify = new List<ReceiveValueFromArgs>();

            var members = Enumerable.Empty<MemberInfo>()
                                    .Concat(GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                    .Concat(GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                    .Concat(GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));

            // first collect members to notify
            foreach (var m in members)
            {
                foreach (var attrib in m.GetCustomAttributes(typeof(ReceiveValueFromAttribute), false)
                                        .Cast<ReceiveValueFromAttribute>())
                {
                    if ((attrib.SenderName ?? string.Empty).Trim() != propertyName)
                    {
                        // does not match
                        continue;
                    }

                    if (!CheckNotifictionAttributeOptions(attrib, areDifferent))
                    {
                        continue;
                    }

                    var args = new ReceiveValueFromArgs()
                    {
                        Attribute = attrib,
                        NotifiableObject = this,
                        NewValue = newValue,
                        OldValue = oldValue,
                        TargetMember = m,
                    };

                    membersToNotify.Add(args);
                }
            }

            // now invoke them in a specific order
            foreach (var args in membersToNotify.OrderBy(x => x.Attribute.SortOrder)
                                                .ThenBy(x => x.TargetMember.Name, StringComparer.Ordinal))
            {
                try
                {
                    object resultToHandle = null;

                    if (args.TargetMember is MethodInfo)
                    {
                        var method = (MethodInfo)args.TargetMember;

                        object[] methodParams = null;

                        var @params = method.GetParameters();
                        if (@params.Length > 0)
                        {
                            if (@params.Length > 2)
                            {
                                // submit sender name, old and new value
                                methodParams = new object[] { args.SenderName, oldValue, newValue };
                            }
                            else if (@params.Length > 1)
                            {
                                // submit old and new value
                                methodParams = new object[] { oldValue, newValue };
                            }
                            else
                            {
                                methodParams = new object[1];

                                methodParams[0] = args;
                                if (!@params[0].ParameterType.Equals(typeof(IReceiveValueFromArgs)))
                                {
                                    // submit new value instead
                                    methodParams[0] = newValue;
                                }
                            }
                        }

                        resultToHandle = method.Invoke(obj: this,
                                                       parameters: methodParams);
                    }
                    else if (args.TargetMember is PropertyInfo)
                    {
                        var property = (PropertyInfo)args.TargetMember;

                        if (property.Name == propertyName)
                        {
                            // not allowed
                            continue;
                        }

                        object propertyValue = args.NewValue;
                        if (property.PropertyType.Equals(typeof(IReceiveValueFromArgs)))
                        {
                            // use argument object instead
                            propertyValue = args;
                        }

                        object[] index = null;

                        var indexParams = property.GetIndexParameters();
                        if (indexParams.Length > 0)
                        {
                            if (indexParams.Length > 1)
                            {
                                // [Type][string]
                                index = new object[] { typeof(TProperty), args.SenderName };
                            }
                            else
                            {
                                index = new object[1];

                                if (indexParams[0].ParameterType.Equals(typeof(Type)) ||
                                    typeof(Type).IsSubclassOf(indexParams[0].ParameterType) ||
                                    typeof(Type).GetInterfaces().Any(x => x.Equals(indexParams[0].ParameterType)))
                                {
                                    // property type

                                    index[0] = typeof(TProperty);
                                }
                                else
                                {
                                    // sender name
                                    index[0] = args.SenderName;
                                }
                            }
                        }

                        property.SetValue(obj: this,
                                          value: propertyValue, index: index);

                        resultToHandle = propertyValue;
                    }
                    else if (args.TargetMember is FieldInfo)
                    {
                        var field = (FieldInfo)args.TargetMember;

                        var fieldValue = args.NewValue;
                        if (field.FieldType.Equals(typeof(IReceiveValueFromArgs)))
                        {
                            // use argument object instead
                            fieldValue = args;
                        }

                        field.SetValue(this, fieldValue);

                        resultToHandle = fieldValue;
                    }

                    var resultHandler = args.ResultHandler;
                    if (resultHandler != null)
                    {
                        resultHandler(args, resultToHandle);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError(ex);
                }
            }
        }

        /// <summary>
        /// Invokes an action for that object thread safe.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeThreadSafe(Action<NotifiableBase> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            InvokeThreadSafe(action: (obj, state) => state.Action(obj),
                             actionState: new
                                 {
                                     Action = action,
                                 });
        }

        /// <summary>
        /// Invokes an action for that object thread safe.
        /// </summary>
        /// <typeparam name="TState">The type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeThreadSafe<TState>(Action<NotifiableBase, TState> action, TState actionState)
        {
            InvokeThreadSafe<TState>(action: action,
                                     actionStateFactory: (obj) => actionState);
        }

        /// <summary>
        /// Invokes an action for that object thread safe.
        /// </summary>
        /// <typeparam name="TState">The type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateFactory">The factory that produces the second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected void InvokeThreadSafe<TState>(Action<NotifiableBase, TState> action, Func<NotifiableBase, TState> actionStateFactory)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            InvokeThreadSafe(
                func: (obj, state) =>
                    {
                        state.Action(obj,
                                     state.StateFactory(obj));

                        return (object)null;
                    },
                funcState: new
                    {
                        Action = action,
                        StateFactory = actionStateFactory,
                    });
        }

        /// <summary>
        /// Invokes a function for that object thread safe.
        /// </summary>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeThreadSafe<TResult>(Func<NotifiableBase, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return InvokeThreadSafe(func: (obj, state) => state.Function(obj),
                                    funcState: new
                                        {
                                            Function = func,
                                        });
        }

        /// <summary>
        /// Invokes a function for that object thread safe.
        /// </summary>
        /// <typeparam name="TState">The tpe of the second argument of <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcState">The second argument of <paramref name="func" />.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeThreadSafe<TState, TResult>(Func<NotifiableBase, TState, TResult> func, TState funcState)
        {
            return InvokeThreadSafe<TState, TResult>(func: func,
                                                     funcStateFactory: (obj) => funcState);
        }

        /// <summary>
        /// Invokes a function for that object thread safe.
        /// </summary>
        /// <typeparam name="TState">The tpe of the second argument of <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcStateFactory">The factory that produces the second argument of <paramref name="func" />.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected TResult InvokeThreadSafe<TState, TResult>(Func<NotifiableBase, TState, TResult> func, Func<NotifiableBase, TState> funcStateFactory)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (funcStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            var result = default(TResult);

            lock (SyncRoot)
            {
                try
                {
                    result = func(this,
                                  funcStateFactory(this));
                }
                catch (Exception ex)
                {
                    RaiseError(ex, true);
                }
            }

            return result;
        }

        private string NormalizePropertyName(IEnumerable<char> propertyName)
        {
            var result = AsString(propertyName);

            if (result == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            result = result.Trim();

            if (result == string.Empty)
            {
                throw new ArgumentException("propertyName");
            }

#if DEBUG
            if (global::System.ComponentModel.TypeDescriptor.GetProperties(this)[result] == null)
            {
                throw new global::System.MissingMemberException(className: this.GetType().FullName,
                                                                memberName: result);
            }
#endif

            return result;
        }

        /// <summary>
        /// Raises the <see cref="NotifiableBase.Error" /> event.
        /// </summary>
        /// <param name="ex">The underlying exception.</param>
        /// <param name="rethrow">
        /// Rethrow exception or not.
        /// <see langword="null" /> indicates that exception only should be rethrown if no event handler was raised.
        /// </param>
        /// <returns>
        /// Handler was raised (<see langword="true" />) or not (<see langword="false" />).
        /// <see langword="null" /> indicates that <paramref name="ex" /> is <see langword="null" />.
        /// </returns>
        /// <exception cref="Exception">The exception of <paramref name="ex" />.</exception>
        protected bool? RaiseError(Exception ex, bool? rethrow = null)
        {
            if (ex == null)
            {
                return null;
            }

            var e = new ErrorEventArgs(ex);
            var result = RaiseEventHandler(Error, e);

            if (rethrow ?? !result)
            {
                throw e.GetException();
            }

            return result;
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <param name="handler">The handler to raise.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        protected bool RaiseEventHandler(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises an event handler.
        /// </summary>
        /// <typeparam name="TArgs">Type of the event arguments.</typeparam>
        /// <param name="handler">The handler to raise.</param>
        /// <param name="e">The arguments for the event.</param>
        /// <returns>Handler was raised (<see langword="true" />); otherwise <paramref name="handler" /> is <see langword="null" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseEventHandler<TArgs>(EventHandler<TArgs> handler, TArgs e)
            where TArgs : global::System.EventArgs
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (handler != null)
            {
                handler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises the <see cref="NotifiableBase.PropertyChanged" /> event.
        /// </summary>
        /// <param name="expr">The expression that contains the property name.</param>
        /// <returns>Handler was raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool RaisePropertyChanged<T>(Expression<Func<T>> expr)
        {
            return RaisePropertyChanged(propertyName: GetPropertyName(expr));
        }

        /// <summary>
        /// Raises the <see cref="NotifiableBase.PropertyChanged" /> event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Handler was raised or not.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool RaisePropertyChanged(string propertyName)
        {
            var pn = NormalizePropertyName(propertyName);

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(pn));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="expr">The expression that contains the name of the property.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        /// <paramref name="newValue" /> is different to current value (<see langword="true" />); otherwise <see langword="false" />.
        /// <see langword="null" /> indicates that old and new value are different, but that
        /// <see cref="NotifiableBase.PropertyChanged" /> event was NOT raised.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="expr" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Body of <paramref name="expr" /> is no <see cref="MemberExpression" />.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Member expression does not contain a <see cref="_PropertyInfo" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool? Set<TProperty>(TProperty newValue, Expression<Func<TProperty>> expr)
        {
            return Set<TProperty>(propertyName: GetPropertyName(expr),
                                  newValue: newValue);
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        /// <paramref name="newValue" /> is different to current value (<see langword="true" />); otherwise <see langword="false" />.
        /// <see langword="null" /> indicates that old and new value are different, but that
        /// <see cref="NotifiableBase.PropertyChanged" /> event was NOT raised.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="propertyName" /> is invalid.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="propertyName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// Property was not found.
        /// </exception>
        protected bool? Set<TProperty>(TProperty newValue, string propertyName)
        {
            var pn = NormalizePropertyName(propertyName);

            var oldValue = Get<TProperty>(pn);

            var comparer = GetPropertyValueEqualityComparer<TProperty>(pn) ?? EqualityComparer<TProperty>.Default;
            var areDifferent = !comparer.Equals(oldValue, newValue);

            bool? result = false;

            if (areDifferent)
            {
                AddOrSet(_PROPERTIES, pn, newValue);

                result = RaisePropertyChanged(pn) ? (bool?)true : null;
            }

            HandleReceiveValueFromAttributes<TProperty>(pn, oldValue, newValue, areDifferent);
            HandleReceiveNotificationFromAttributes<TProperty>(pn, areDifferent);

            return result;
        }

        #endregion Methods (31)
    }
}
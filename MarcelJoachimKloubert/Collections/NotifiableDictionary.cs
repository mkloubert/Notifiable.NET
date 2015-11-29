/**********************************************************************************************************************
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

using MarcelJoachimKloubert.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MarcelJoachimKloubert.Collections
{
    /// <summary>
    /// A notifiable dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    public class NotifiableDictionary<TKey, TValue> : NotifiableCollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableDictionary{TKey, TValue}" /> class.
        /// </summary>
        /// <param name="items">
        /// The initial items / the base collection (if defined).
        /// If that value is already an <see cref="IList{T}" /> object, it is used as value for <see cref="NotifiableDictionary{TKey, TValue}.BaseCollection" /> property.
        /// Otherwise a new collection is created with the items of that value.
        /// </param>
        /// <param name="syncRoot">The custom object for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        public NotifiableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items = null, object syncRoot = null)
            : base(items: items, syncRoot: syncRoot)
        {
        }

        #endregion Constructors (1)

        #region Properties (8)

        /// <summary>
        /// <see cref="NotifiableCollection{T}.BaseCollection"/>
        /// </summary>
        public new IDictionary<TKey, TValue> BaseCollection
        {
            get { return (IDictionary<TKey, TValue>)base.BaseCollection; }
        }

        /// <summary>
        /// <see cref="IDictionary.IsFixedSize" />
        /// </summary>
        [ReceiveNotificationFrom("IsReadOnly")]
        public bool IsFixedSize
        {
            get
            {
                return IfIDictionary((dict) => dict.IsFixedSize,
                                     (dict) => dict.IsReadOnly);
            }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Keys" />
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return BaseCollection.Keys; }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return IfIDictionary((dict) => dict.Keys,
                                     (dict) => AsCollection(dict.Keys));
            }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.this[TKey]" />
        /// </summary>
        public TValue this[TKey key]
        {
            get { return BaseCollection[key]; }

            set
            {
                var oldValue = TryGetOldValue(key);

                BaseCollection[key] = value;

                var valueComparer = GetPropertyValueEqualityComparer<TValue>("Item") ?? EqualityComparer<TValue>.Default;
                if (!valueComparer.Equals(oldValue, value))
                {
                    RaisePropertyChanged("Item");
                    RaisePropertyChanged(() => Values);
                }

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Replace,
                                                             newItem: value, oldItem: oldValue);
                RaiseCollectionChanged(e);
            }
        }

        object IDictionary.this[object key]
        {
            get { return this[ConvertTo<TKey>(key)]; }

            set { this[ConvertTo<TKey>(key)] = ConvertTo<TValue>(value); }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Values" />
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return BaseCollection.Values; }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return IfIDictionary((dict) => dict.Values,
                                     (dict) => AsCollection(dict.Values));
            }
        }

        #endregion Properties (8)

        #region Methods (23)

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            BaseCollection.Add(key, value);
            RaiseCollectionEvents();

            var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Add,
                                                         changedItem: new KeyValuePair<TKey, TValue>(key, value));
            RaiseCollectionChanged(e);
        }

        void IDictionary.Add(object key, object value)
        {
            Add(ConvertTo<TKey>(key),
                ConvertTo<TValue>(value));
        }

        bool IDictionary.Contains(object key)
        {
            return ContainsKey(ConvertTo<TKey>(key));
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.ContainsKey(TKey)" />
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return BaseCollection.ContainsKey(key);
        }

        /// <summary>
        /// Invokes an action for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditDictionary(Action<NotifiableDictionary<TKey, TValue>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            EditDictionary(action: (dict, state) => state.Action(dict),
                           actionState: new
                               {
                                   Action = action,
                               });
        }

        /// <summary>
        /// Invokes an action for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditDictionary<TState>(Action<NotifiableDictionary<TKey, TValue>, TState> action, TState actionState)
        {
            EditDictionary<TState>(action: action,
                                   actionStateFactory: (dict) => actionState);
        }

        /// <summary>
        /// Invokes an action for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateFactory">The function that returns the second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        public void EditDictionary<TState>(Action<NotifiableDictionary<TKey, TValue>, TState> action,
                                           Func<NotifiableDictionary<TKey, TValue>, TState> actionStateFactory)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            EditDictionary(
                func: (dict, state) =>
                    {
                        state.Action(dict,
                                     state.StateFactory(dict));

                        return (object)null;
                    },
                funcState: new
                    {
                        Action = action,
                        StateFactory = actionStateFactory,
                    });
        }

        /// <summary>
        /// Invokes a function for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        public TResult EditDictionary<TResult>(Func<NotifiableDictionary<TKey, TValue>, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return EditDictionary(func: (dict, state) => state.Function(dict),
                                  funcState: new
                                      {
                                          Function = func,
                                      });
        }

        /// <summary>
        /// Invokes a function for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcState">The second argument for <paramref name="func" />.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        public TResult EditDictionary<TState, TResult>(Func<NotifiableDictionary<TKey, TValue>, TState, TResult> func, TState funcState)
        {
            return EditDictionary<TState, TResult>(func: func,
                                                   funcStateFactory: (dict) => funcState);
        }

        /// <summary>
        /// Invokes a function for this dictionary while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="func" />.</typeparam>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <param name="funcStateFactory">The function that returns the second argument for <paramref name="func" />.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        public TResult EditDictionary<TState, TResult>(Func<NotifiableDictionary<TKey, TValue>, TState, TResult> func,
                                                       Func<NotifiableDictionary<TKey, TValue>, TState> funcStateFactory)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (funcStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            return EditCollection(
                func: (coll, state) =>
                    {
                        var dict = (NotifiableDictionary<TKey, TValue>)coll;

                        return state.Function(dict,
                                              state.StateFactory(dict));
                    },
                funcState: new
                    {
                        Function = func,
                        StateFactory = funcStateFactory,
                    });
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return IfIDictionary((dict) => dict.GetEnumerator(),
                                 (dict) => new Dictionary<TKey, TValue>(dict).GetEnumerator());
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfIDictionary(Action<IDictionary> actionYes,
                                     Action<IDictionary<TKey, TValue>> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("actionYes");
            }

            IfIDictionary(actionYes: (dict, state) => state.ActionYes(dict),
                          actionState: new
                              {
                                  ActionNo = actionNo,
                                  ActionYes = actionYes,
                              },
                          actionNo: (coll, state) =>
                              {
                                  state.ActionNo(coll);
                              });
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="actionState">The value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfIDictionary<TState>(Action<IDictionary, TState> actionYes,
                                             TState actionState,
                                             Action<IDictionary<TKey, TValue>, TState> actionNo = null)
        {
            IfIDictionary<TState>(actionYes: actionYes,
                                  actionNo: actionNo,
                                  actionStateFactory: (dict) => actionState);
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="actionStateFactory">The function that returns the value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected void IfIDictionary<TState>(Action<IDictionary, TState> actionYes,
                                             Func<NotifiableDictionary<TKey, TValue>, TState> actionStateFactory,
                                             Action<IDictionary<TKey, TValue>, TState> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("actionYes");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            IfIDictionary(
                funcYes: (dict, state) =>
                    {
                        state.ActionYes(dict, state.State);

                        return (object)null;
                    },
                funcState: new
                    {
                        ActionNo = actionNo,
                        ActionYes = actionYes,
                        State = actionStateFactory(this),
                    },
                funcNo: (dict, state) =>
                    {
                        if (state.ActionNo != null)
                        {
                            state.ActionNo(dict, state.State);
                        }

                        return (object)null;
                    });
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIDictionary<TResult>(Func<IDictionary, TResult> funcYes,
                                                 Func<IDictionary<TKey, TValue>, TResult> funcNo = null)
        {
            if (funcYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            return IfIDictionary(funcYes: (dict, state) => state.FunctionYes(dict),
                                 funcState: new
                                     {
                                         FunctionNo = funcNo,
                                         FunctionYes = funcYes,
                                     },
                                 funcNo: (dict, state) => state.FunctionNo != null ? state.FunctionNo(dict)
                                                                                   : default(TResult));
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="funcState">The value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIDictionary<TState, TResult>(Func<IDictionary, TState, TResult> funcYes,
                                                         TState funcState,
                                                         Func<IDictionary<TKey, TValue>, TState, TResult> funcNo = null)
        {
            return IfIDictionary<TState, TResult>(funcYes: funcYes,
                                                  funcNo: funcNo,
                                                  funcStateFactory: (dict) => funcState);
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IDictionary" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IDictionary" /> object.</param>
        /// <param name="funcStateFactory">The function that returns the value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIDictionary<TState, TResult>(Func<IDictionary, TState, TResult> funcYes,
                                                         Func<NotifiableDictionary<TKey, TValue>, TState> funcStateFactory,
                                                         Func<IDictionary<TKey, TValue>, TState, TResult> funcNo = null)
        {
            if (funcYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            if (funcStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            if (funcNo == null)
            {
                funcNo = (dict, state) => default(TResult);
            }

            var funcState = funcStateFactory(this);

            var dictionary = BaseCollection as IDictionary;
            if (dictionary != null)
            {
                return funcYes(dictionary, funcState);
            }

            return funcNo(BaseCollection, funcState);
        }

        /// <summary>
        /// Initializes the value for <see cref="NotifiableCollection{T}.BaseCollection" /> property.
        /// </summary>
        /// <param name="items">The initial items (if defined).</param>
        /// <returns>The value for <see cref="NotifiableCollection{T}.BaseCollection" /></returns>
        protected override ICollection<KeyValuePair<TKey, TValue>> InitBaseCollection(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            if (items is IDictionary<TKey, TValue>)
            {
                return (IDictionary<TKey, TValue>)items;
            }

            IDictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            if (items != null)
            {
                using (var e = items.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        result.Add(e.Current);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// <see cref="NotifiableCollection{T}.RaiseCollectionEvents()" />
        /// </summary>
        protected override void RaiseCollectionEvents()
        {
            if (IsEditing)
            {
                return;
            }

            base.RaiseCollectionEvents();

            RaisePropertyChanged(() => Keys);
            RaisePropertyChanged(() => Values);
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Remove(TKey)" />
        /// </summary>
        public bool Remove(TKey key)
        {
            var oldValue = TryGetOldValue(key);

            var result = BaseCollection.Remove(key);

            if (result)
            {
                RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove,
                                                             changedItem: oldValue);
                RaiseCollectionChanged(e);
            }

            return result;
        }

        void IDictionary.Remove(object key)
        {
            Remove(ConvertTo<TKey>(key));
        }

        [DebuggerStepThrough]
        private TValue TryGetOldValue(TKey key)
        {
            TValue result;
            TryGetValue(key, out result);

            return result;
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return BaseCollection.TryGetValue(key, out value);
        }

        #endregion Methods (23)
    }
}
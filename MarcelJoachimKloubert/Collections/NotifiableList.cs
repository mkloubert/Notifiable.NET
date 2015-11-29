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
    /// A notifiable list.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class NotifiableList<T> : NotifiableCollection<T>, IList<T>, IList, IReadOnlyList<T>
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableList{T}" /> class.
        /// </summary>
        /// <param name="items">
        /// The initial items / the base collection (if defined).
        /// If that value is already an <see cref="IList{T}" /> object, it is used as value for <see cref="NotifiableList{T}.BaseCollection" /> property.
        /// Otherwise a new collection is created with the items of that value.
        /// </param>
        /// <param name="syncRoot">The custom object for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        public NotifiableList(IEnumerable<T> items = null, object syncRoot = null)
            : base(items: items, syncRoot: syncRoot)
        {
        }

        #endregion Constructors (1)

        #region Properties (3)

        /// <summary>
        /// <see cref="NotifiableCollection{T}.BaseCollection"/>
        /// </summary>
        public new IList<T> BaseCollection
        {
            get { return (IList<T>)base.BaseCollection; }
        }

        /// <summary>
        /// <see cref="IList{T}.this[int]" />
        /// </summary>
        public T this[int index]
        {
            get { return BaseCollection[index]; }

            set
            {
                var oldItem = TryGetOldItem(index);

                BaseCollection[index] = value;

                var comparer = GetPropertyValueEqualityComparer<T>("Item") ?? EqualityComparer<T>.Default;
                if (!comparer.Equals(oldItem, value))
                {
                    RaisePropertyChanged("Item");
                }

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Replace,
                                                             newItem: value, oldItem: oldItem, index: index);
                RaiseCollectionChanged(e);
            }
        }

        object IList.this[int index]
        {
            get { return this[index]; }

            set { this[index] = ConvertTo<T>(value); }
        }

        /// <summary>
        /// <see cref="IList.IsFixedSize" />
        /// </summary>
        [ReceiveNotificationFrom("IsReadOnly")]
        public bool IsFixedSize
        {
            get
            {
                return IfIList((list) => list.IsFixedSize,
                               (list) => list.IsReadOnly);
            }
        }

        #endregion Properties (3)

        #region Methods (22)

        int IList.Add(object value)
        {
            Add(ConvertTo<T>(value));
            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            return Contains(ConvertTo<T>(value));
        }

        /// <summary>
        /// Invokes an action for this list while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditList(Action<NotifiableList<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            EditList(action: (list, state) => state.Action(list),
                     actionState: new
                         {
                             Action = action,
                         });
        }

        /// <summary>
        /// Invokes an action for this list while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditList<TState>(Action<NotifiableList<T>, TState> action, TState actionState)
        {
            EditList<TState>(action: action,
                             actionStateFactory: (list) => actionState);
        }

        /// <summary>
        /// Invokes an action for this list while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateFactory">The function that returns the second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        public void EditList<TState>(Action<NotifiableList<T>, TState> action,
                                     Func<NotifiableList<T>, TState> actionStateFactory)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            EditList(
                func: (list, state) =>
                    {
                        state.Action(list,
                                     state.StateFactory(list));

                        return (object)null;
                    },
                funcState: new
                    {
                        Action = action,
                        StateFactory = actionStateFactory,
                    });
        }

        /// <summary>
        /// Invokes a function for this list while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        public TResult EditList<TResult>(Func<NotifiableList<T>, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return EditList(func: (list, state) => state.Function(list),
                            funcState: new
                                {
                                    Function = func,
                                });
        }

        /// <summary>
        /// Invokes a function for this list while it is in edit mode.
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
        public TResult EditList<TState, TResult>(Func<NotifiableList<T>, TState, TResult> func, TState funcState)
        {
            return EditList<TState, TResult>(func: func,
                                             funcStateFactory: (list) => funcState);
        }

        /// <summary>
        /// Invokes a function for this list while it is in edit mode.
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
        public TResult EditList<TState, TResult>(Func<NotifiableList<T>, TState, TResult> func,
                                                 Func<NotifiableList<T>, TState> funcStateFactory)
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
                        var list = (NotifiableList<T>)coll;

                        return state.Function(list,
                                              state.StateFactory(list));
                    },
                funcState: new
                    {
                        Function = func,
                        StateFactory = funcStateFactory,
                    });
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfIList(Action<IList> actionYes,
                               Action<IList<T>> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("actionYes");
            }

            IfIList(actionYes: (list, state) => state.ActionYes(list),
                         actionState: new
                             {
                                 ActionNo = actionNo,
                                 ActionYes = actionYes,
                             },
                         actionNo: (list, state) =>
                             {
                                 state.ActionNo(list);
                             });
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="actionState">The value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfIList<TState>(Action<IList, TState> actionYes,
                                       TState actionState,
                                       Action<IList<T>, TState> actionNo = null)
        {
            IfIList<TState>(actionYes: actionYes,
                            actionNo: actionNo,
                            actionStateFactory: (list) => actionState);
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="actionStateFactory">The function that returns the value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected void IfIList<TState>(Action<IList, TState> actionYes,
                                       Func<NotifiableList<T>, TState> actionStateFactory,
                                       Action<IList<T>, TState> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("actionYes");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            IfIList(
                funcYes: (list, state) =>
                    {
                        state.ActionYes(list, state.State);

                        return (object)null;
                    },
                funcState: new
                    {
                        ActionNo = actionNo,
                        ActionYes = actionYes,
                        State = actionStateFactory(this),
                    },
                funcNo: (list, state) =>
                    {
                        if (state.ActionNo != null)
                        {
                            state.ActionNo(list, state.State);
                        }

                        return (object)null;
                    });
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIList<TResult>(Func<IList, TResult> funcYes,
                                           Func<IList<T>, TResult> funcNo = null)
        {
            if (funcYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            return IfIList(funcYes: (list, state) => state.FunctionYes(list),
                           funcState: new
                               {
                                   FunctionNo = funcNo,
                                   FunctionYes = funcYes,
                               },
                               funcNo: (list, state) => state.FunctionNo != null ? state.FunctionNo(list)
                                                                                 : default(TResult));
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="funcState">The value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIList<TState, TResult>(Func<IList, TState, TResult> funcYes,
                                                   TState funcState,
                                                   Func<IList<T>, TState, TResult> funcNo = null)
        {
            return IfIList<TState, TResult>(funcYes: funcYes,
                                            funcNo: funcNo,
                                            funcStateFactory: (coll) => funcState);
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="IList" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="IList" /> object.</param>
        /// <param name="funcStateFactory">The function that returns the value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfIList<TState, TResult>(Func<IList, TState, TResult> funcYes,
                                                   Func<NotifiableList<T>, TState> funcStateFactory,
                                                   Func<IList<T>, TState, TResult> funcNo = null)
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
                funcNo = (list, state) => default(TResult);
            }

            var funcState = funcStateFactory(this);

            var iList = BaseCollection as IList;
            if (iList != null)
            {
                return funcYes(iList, funcState);
            }

            return funcNo(BaseCollection, funcState);
        }

        /// <summary>
        /// <see cref="IList{T}.IndexOf(T)" />
        /// </summary>
        public int IndexOf(T item)
        {
            return BaseCollection.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf(ConvertTo<T>(value));
        }

        /// <summary>
        /// <see cref="NotifiableCollection{T}.InitBaseCollection(IEnumerable{T})"/>
        /// </summary>
        protected override ICollection<T> InitBaseCollection(IEnumerable<T> items)
        {
            if (items is IList<T>)
            {
                return (IList<T>)items;
            }

            var result = new List<T>();

            if (items != null)
            {
                result.AddRange(items);
            }

            return result;
        }

        /// <summary>
        /// <see cref="IList{T}.Insert(int, T)" />
        /// </summary>
        public void Insert(int index, T item)
        {
            var oldItem = TryGetOldItem(index);

            BaseCollection.Insert(index, item);
            RaiseCollectionEvents();

            var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Move,
                                                         changedItem: oldItem,
                                                         index: index + 1, oldIndex: index);
            RaiseCollectionChanged(e);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index,
                   ConvertTo<T>(value));
        }

        void IList.Remove(object value)
        {
            Remove(ConvertTo<T>(value));
        }

        /// <summary>
        /// <see cref="IList{T}.RemoveAt(int)" />
        /// </summary>
        public void RemoveAt(int index)
        {
            var oldCount = Count;
            var oldItem = TryGetOldItem(index);

            BaseCollection.RemoveAt(index);

            if (oldCount != Count)
            {
                RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove,
                                                             changedItem: oldItem, index: index);
                RaiseCollectionChanged(e);
            }
        }

        [DebuggerStepThrough]
        private T TryGetOldItem(int index)
        {
            T result;
            try
            {
                result = this[index];
            }
            catch
            {
                result = default(T);
            }

            return result;
        }

        #endregion Methods (22)
    }
}
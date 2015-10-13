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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MarcelJoachimKloubert.Collections
{
    /// <summary>
    /// A notifiable collection.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class NotifiableCollection<T> : NotifiableBase, ICollection<T>, ICollection, INotifyCollectionChanged, IReadOnlyCollection<T>
    {
        #region Fields (1)

        private readonly ICollection<T> _BASE_COLLECTION;

        #endregion Fields (1)

        #region Constructors (2)

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableCollection{T}" /> class.
        /// </summary>
        /// <param name="items">
        /// The initial items / the base collection (if defined).
        /// If that value is already an <see cref="ICollection{T}" /> object, it is used as value for <see cref="NotifiableCollection{T}.BaseCollection" /> property.
        /// Otherwise a new collection is created with the items of that value.
        /// </param>
        /// <param name="syncRoot">The custom object for the <see cref="NotifiableBase.SyncRoot" /> property.</param>
        public NotifiableCollection(IEnumerable<T> items = null, object syncRoot = null)
            : base(syncRoot: syncRoot)
        {
            this._BASE_COLLECTION = this.InitBaseCollection(items) ?? new List<T>();

            if (this._BASE_COLLECTION is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)this._BASE_COLLECTION).CollectionChanged += this.NotifiableCollection_CollectionChanged;
            }

            if (this._BASE_COLLECTION is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)this._BASE_COLLECTION).PropertyChanged += this.NotifiableCollection_PropertyChanged;
            }
        }

        /// <summary>
        /// Sends that object to the garbage collector.
        /// </summary>
        ~NotifiableCollection()
        {
            try
            {
                try
                {
                    if (this._BASE_COLLECTION is INotifyCollectionChanged)
                    {
                        ((INotifyCollectionChanged)this._BASE_COLLECTION).CollectionChanged -= this.NotifiableCollection_CollectionChanged;
                    }
                }
                finally
                {
                    if (this._BASE_COLLECTION is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)this._BASE_COLLECTION).PropertyChanged -= this.NotifiableCollection_PropertyChanged;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        #endregion Constructors (2)

        #region Events (1)

        /// <summary>
        /// <see cref="INotifyCollectionChanged.CollectionChanged" />
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion Events (1)

        #region Properties (5)

        /// <summary>
        /// Gets the base collection.
        /// </summary>
        public ICollection<T> BaseCollection
        {
            get { return this._BASE_COLLECTION; }
        }

        /// <summary>
        /// <see cref="ICollection{T}.Count" />
        /// </summary>
        public int Count
        {
            get { return this._BASE_COLLECTION.Count; }
        }

        /// <summary>
        /// Gets if the collection is in edit mode or not.
        /// </summary>
        public bool IsEditing
        {
            get { return this.Get(() => this.IsEditing); }

            private set { this.Set(value, () => this.IsEditing); }
        }

        /// <summary>
        /// <see cref="ICollection{T}.IsReadOnly" />
        /// </summary>
        public bool IsReadOnly
        {
            get { return this._BASE_COLLECTION.IsReadOnly; }
        }

        /// <summary>
        /// <see cref="ICollection.IsSynchronized" />
        /// </summary>
        public bool IsSynchronized
        {
            get { return this.IfICollection((coll) => coll.IsSynchronized); }
        }

        #endregion Properties (5)

        #region Methods (29)

        /// <summary>
        /// <see cref="ICollection{T}.Add(T)" />
        /// </summary>
        public void Add(T item)
        {
            this._BASE_COLLECTION.Add(item);
            this.RaiseCollectionEvents();

            var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Add,
                                                         changedItem: item);
            this.RaiseCollectionChanged(e);
        }

        /// <summary>
        /// Returns an <see cref="ICollection{TItem}" /> object as <see cref="ICollection" />.
        /// </summary>
        /// <typeparam name="TItem">Type of the items.</typeparam>
        /// <param name="coll">The input value.</param>
        /// <returns>The output value.</returns>
        public static ICollection AsCollection<TItem>(ICollection<TItem> coll)
        {
            if (coll is ICollection)
            {
                return (ICollection)coll;
            }

            if (coll == null)
            {
                return null;
            }

            return new List<TItem>(coll);
        }

        /// <summary>
        /// <see cref="ICollection{T}.Clear()" />
        /// </summary>
        public void Clear()
        {
            var oldCount = this._BASE_COLLECTION.Count;

            this._BASE_COLLECTION.Clear();

            if (oldCount != this._BASE_COLLECTION.Count)
            {
                this.RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset);
                this.RaiseCollectionChanged(e);
            }
        }

        /// <summary>
        /// <see cref="ICollection{T}.Contains(T)" />
        /// </summary>
        public bool Contains(T item)
        {
            return this._BASE_COLLECTION.Contains(item);
        }

        /// <summary>
        /// <see cref="ICollection{T}.CopyTo(T[], int)" />
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this._BASE_COLLECTION.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            var srcArray = AsArray(this._BASE_COLLECTION);

            Array.Copy(sourceArray: srcArray, sourceIndex: 0,
                       destinationArray: array, destinationIndex: index,
                       length: srcArray.Length);
        }

        /// <summary>
        /// Invokes an action for this collection while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditCollection(Action<NotifiableCollection<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.EditCollection(action: (coll, state) => state.Action(coll),
                                actionState: new
                                    {
                                        Action = action,
                                    });
        }

        /// <summary>
        /// Invokes an action for this collection while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionState">The second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> is <see langword="null" />.
        /// </exception>
        public void EditCollection<TState>(Action<NotifiableCollection<T>, TState> action, TState actionState)
        {
            this.EditCollection<TState>(action: action,
                                        actionStateFactory: (coll) => actionState);
        }

        /// <summary>
        /// Invokes an action for this collection while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TState">Type of the second argument of <paramref name="action" />.</typeparam>
        /// <param name="action">The action to invoke.</param>
        /// <param name="actionStateFactory">The function that returns the second argument for <paramref name="action" />.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        public void EditCollection<TState>(Action<NotifiableCollection<T>, TState> action,
                                           Func<NotifiableCollection<T>, TState> actionStateFactory)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("actionStateFactory");
            }

            this.EditCollection(
                func: (coll, state) =>
                    {
                        state.Action(coll,
                                     state.StateFactory(coll));

                        return (object)null;
                    },
                funcState: new
                    {
                        Action = action,
                        StateFactory = actionStateFactory,
                    });
        }

        /// <summary>
        /// Invokes a function for this collection while it is in edit mode.
        /// In that mode change events have no effect.
        /// </summary>
        /// <typeparam name="TResult">Type of the result of <paramref name="func" />.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>The result of <paramref name="func" />.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="func" /> is <see langword="null" />.
        /// </exception>
        public TResult EditCollection<TResult>(Func<NotifiableCollection<T>, TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return this.EditCollection(func: (coll, state) => state.Function(coll),
                                       funcState: new
                                           {
                                               Function = func,
                                           });
        }

        /// <summary>
        /// Invokes a function for this collection while it is in edit mode.
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
        public TResult EditCollection<TState, TResult>(Func<NotifiableCollection<T>, TState, TResult> func, TState funcState)
        {
            return this.EditCollection<TState, TResult>(func: func,
                                                        funcStateFactory: (coll) => funcState);
        }

        /// <summary>
        /// Invokes a function for this collection while it is in edit mode.
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
        public TResult EditCollection<TState, TResult>(Func<NotifiableCollection<T>, TState, TResult> func,
                                                       Func<NotifiableCollection<T>, TState> funcStateFactory)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            if (funcStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            var oldState = this.IsEditing;
            try
            {
                this.IsEditing = true;

                return func(this,
                            funcStateFactory(this));
            }
            catch (Exception ex)
            {
                this.RaiseError(ex);

                throw ex;
            }
            finally
            {
                this.IsEditing = oldState;

                if (!oldState)
                {
                    this.RaiseCollectionEvents();

                    var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    this.RaiseCollectionChanged(e);
                }
            }
        }

        /// <summary>
        /// <see cref="IEnumerable{T}.GetEnumerator()" />
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return this._BASE_COLLECTION.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfICollection(Action<ICollection> actionYes,
                                     Action<ICollection<T>> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            this.IfICollection(actionYes: (coll, state) => state.ActionYes(coll),
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
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="actionState">The value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> is <see langword="null" />.
        /// </exception>
        protected void IfICollection<TState>(Action<ICollection, TState> actionYes,
                                             TState actionState,
                                             Action<ICollection<T>, TState> actionNo = null)
        {
            this.IfICollection<TState>(actionYes: actionYes,
                                       actionNo: actionNo,
                                       actionStateFactory: (coll) => actionState);
        }

        /// <summary>
        /// Invokes an action for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative action.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the actions.</typeparam>
        /// <param name="actionYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="actionStateFactory">The function that returns the value for the second arguments of the actions.</param>
        /// <param name="actionNo">The optional alternative actions to invoke.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="actionYes" /> and/or <paramref name="actionStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected void IfICollection<TState>(Action<ICollection, TState> actionYes,
                                             Func<NotifiableCollection<T>, TState> actionStateFactory,
                                             Action<ICollection<T>, TState> actionNo = null)
        {
            if (actionYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            if (actionStateFactory == null)
            {
                throw new ArgumentNullException("funcStateFactory");
            }

            this.IfICollection(
                funcYes: (coll, state) =>
                    {
                        state.ActionYes(coll, state.State);

                        return (object)null;
                    },
                funcState: new
                    {
                        ActionNo = actionNo,
                        ActionYes = actionYes,
                        State = actionStateFactory(this),
                    },
                funcNo: (coll, state) =>
                    {
                        if (state.ActionNo != null)
                        {
                            state.ActionNo(coll, state.State);
                        }

                        return (object)null;
                    });
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfICollection<TResult>(Func<ICollection, TResult> funcYes,
                                                 Func<ICollection<T>, TResult> funcNo = null)
        {
            if (funcYes == null)
            {
                throw new ArgumentNullException("funcYes");
            }

            return this.IfICollection(funcYes: (coll, state) => state.FunctionYes(coll),
                                      funcState: new
                                          {
                                              FunctionNo = funcNo,
                                              FunctionYes = funcYes,
                                          },
                                      funcNo: (coll, state) => state.FunctionNo != null ? state.FunctionNo(coll)
                                                                                        : default(TResult));
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="funcState">The value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfICollection<TState, TResult>(Func<ICollection, TState, TResult> funcYes,
                                                         TState funcState,
                                                         Func<ICollection<T>, TState, TResult> funcNo = null)
        {
            return this.IfICollection<TState, TResult>(funcYes: funcYes,
                                                       funcNo: funcNo,
                                                       funcStateFactory: (coll) => funcState);
        }

        /// <summary>
        /// Invokes a function for <see cref="NotifiableCollection{T}.BaseCollection" /> if it is an <see cref="ICollection" /> object.
        /// Otherwise an optional alternative function.
        /// </summary>
        /// <typeparam name="TState">Type of the second arguments of the functions.</typeparam>
        /// <typeparam name="TResult">The result of the functions.</typeparam>
        /// <param name="funcYes">The function to invoke if base collection is an <see cref="ICollection" /> object.</param>
        /// <param name="funcStateFactory">The function that returns the value for the second arguments of the functions.</param>
        /// <param name="funcNo">
        /// The optional alternative function to invoke.
        /// If not defined, a default logic is invoked that returns a default value.
        /// </param>
        /// <returns>The result of the matching function.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="funcYes" /> and/or <paramref name="funcStateFactory" /> is <see langword="null" />.
        /// </exception>
        protected TResult IfICollection<TState, TResult>(Func<ICollection, TState, TResult> funcYes,
                                                         Func<NotifiableCollection<T>, TState> funcStateFactory,
                                                         Func<ICollection<T>, TState, TResult> funcNo = null)
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
                funcNo = (coll, state) => default(TResult);
            }

            var funcState = funcStateFactory(this);

            var collection = this._BASE_COLLECTION as ICollection;
            if (collection != null)
            {
                return funcYes(collection, funcState);
            }

            return funcNo(this._BASE_COLLECTION, funcState);
        }

        /// <summary>
        /// Initializes the value for <see cref="NotifiableCollection{T}.BaseCollection" /> property.
        /// </summary>
        /// <param name="items">The initial items (if defined).</param>
        /// <returns>The value for <see cref="NotifiableCollection{T}.BaseCollection" /></returns>
        protected virtual ICollection<T> InitBaseCollection(IEnumerable<T> items)
        {
            if (items is ICollection<T>)
            {
                return (ICollection<T>)items;
            }

            var result = new List<T>();

            if (items != null)
            {
                result.AddRange(items);
            }

            return result;
        }

        private void NotifiableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RaiseCollectionChanged(e);
        }

        private void NotifiableCollection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = this.GetType()
                               .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                               .FirstOrDefault(x => x.Name == e.PropertyName);

            if (property == null)
            {
                return;
            }

            this.RaisePropertyChanged(property.Name);
        }

        /// <summary>
        /// Is invoked when edit mode started.
        /// </summary>
        protected virtual void OnBeginEdit()
        {
        }

        /// <summary>
        /// Is invoked when edit mode has changed.
        /// </summary>
        /// <param name="args">The data with the new value.</param>
        [ReceiveValueFrom("IsEditing")]
        protected void OnEditModeChanged(IReceiveValueFromArgs args)
        {
            if (args.GetNewValue<bool>())
            {
                this.OnBeginEdit();
            }
            else
            {
                this.OnEndEdit();
            }
        }

        /// <summary>
        /// Is invoked when edit mode has ended.
        /// </summary>
        protected virtual void OnEndEdit()
        {
        }

        /// <summary>
        /// Raises the <see cref="NotifiableCollection{T}.CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The argument for the event.</param>
        /// <returns>
        /// Handler was raised or not.
        /// <see langword="null" /> indicates that collection is currently in edit mode.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool? RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (this.IsEditing)
            {
                return null;
            }

            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raises all common collection events.
        /// </summary>
        protected virtual void RaiseCollectionEvents()
        {
            if (this.IsEditing)
            {
                return;
            }

            this.RaisePropertyChanged(() => this.Count);
        }

        /// <summary>
        /// <see cref="ICollection{T}.Remove(T)" />
        /// </summary>
        public bool Remove(T item)
        {
            var result = this._BASE_COLLECTION.Remove(item);

            if (result)
            {
                this.RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove,
                                                             changedItem: item);
                this.RaiseCollectionChanged(e);
            }

            return result;
        }

        #endregion Methods (29)
    }
}
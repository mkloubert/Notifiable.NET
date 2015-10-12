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
            get { return this.BaseCollection[index]; }

            set
            {
                var oldItem = this.TryGetOldItem(index);

                this.BaseCollection[index] = value;

                var comparer = this.GetPropertyValueEqualityComparer<T>("Item") ?? EqualityComparer<T>.Default;
                if (!comparer.Equals(oldItem, value))
                {
                    this.RaisePropertyChanged("Item");
                }

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Replace,
                                                             newItem: value, oldItem: oldItem, index: index);
                this.RaiseCollectionChanged(e);
            }
        }

        object IList.this[int index]
        {
            get { return this[index]; }

            set { this[index] = (T)value; }
        }

        /// <summary>
        /// <see cref="IList.IsFixedSize" />
        /// </summary>
        [ReceiveNotificationFrom("IsReadOnly")]
        public bool IsFixedSize
        {
            get
            {
                if (this.BaseCollection is IList)
                {
                    return ((IList)this.BaseCollection).IsFixedSize;
                }

                return this.IsReadOnly;
            }
        }

        #endregion Properties (3)

        #region Methods (10)

        int IList.Add(object value)
        {
            this.Add((T)value);
            return this.Count;
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        /// <summary>
        /// <see cref="IList{T}.IndexOf(T)" />
        /// </summary>
        public int IndexOf(T item)
        {
            return this.BaseCollection.IndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
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
            var oldItem = this.TryGetOldItem(index);

            this.BaseCollection.Insert(index, item);
            this.RaiseCollectionEvents();

            var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Move,
                                                         changedItem: oldItem,
                                                         index: index + 1, oldIndex: index);
            this.RaiseCollectionChanged(e);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        /// <summary>
        /// <see cref="IList{T}.RemoveAt(int)" />
        /// </summary>
        public void RemoveAt(int index)
        {
            var oldCount = this.Count;
            var oldItem = this.TryGetOldItem(index);

            this.BaseCollection.RemoveAt(index);

            if (oldCount != this.Count)
            {
                this.RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove,
                                                         changedItem: oldItem, index: index);
                this.RaiseCollectionChanged(e);
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

        #endregion Methods (10)
    }
}
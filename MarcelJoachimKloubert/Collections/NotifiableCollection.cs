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

        #region Properties (4)

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
            get
            {
                if (this._BASE_COLLECTION is ICollection)
                {
                    return ((ICollection)this._BASE_COLLECTION).IsSynchronized;
                }

                return false;
            }
        }

        #endregion Properties (4)

        #region Methods (14)

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
            var oldItems = this._BASE_COLLECTION.ToArray();

            this._BASE_COLLECTION.Clear();

            if (oldItems.Length != this._BASE_COLLECTION.Count)
            {
                this.RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset,
                                                             changedItems: oldItems);
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
        /// Raises the <see cref="NotifiableCollection{T}.CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The argument for the event.</param>
        /// <returns>Handler was raised or not.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="e" /> is <see langword="null" />.
        /// </exception>
        protected bool RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
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

        #endregion Methods (14)
    }
}
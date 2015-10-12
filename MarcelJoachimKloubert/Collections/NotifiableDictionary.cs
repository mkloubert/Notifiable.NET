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
    /// A notifiable dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of the keys.</typeparam>
    /// <typeparam name="TValue">Type of the values.</typeparam>
    public class NotifiableDictionary<TKey, TValue> : NotifiableCollection<KeyValuePair<TKey, TValue>>,
                                                      IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
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

        #region Properties (10)

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
        public bool IsFixedSize
        {
            get
            {
                if (this.BaseCollection is IDictionary)
                {
                    return ((IDictionary)this.BaseCollection).IsFixedSize;
                }

                return this.IsReadOnly;
            }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Keys" />
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return this.BaseCollection.Keys; }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get { return this.Keys; }
        }

        ICollection IDictionary.Keys
        {
            get { return AsCollection(this.Keys); }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.this[TKey]" />
        /// </summary>
        public TValue this[TKey key]
        {
            get { return this.BaseCollection[key]; }

            set
            {
                var oldValue = this.TryGetOldValue(key);

                this.BaseCollection[key] = value;

                // find zero based index of key
                var keyComparer = this.GetPropertyValueEqualityComparer<TKey>("Item") ?? EqualityComparer<TKey>.Default;
                var index = -1;
                var i = -1;
                foreach (var k in this.Keys)
                {
                    ++i;

                    if (keyComparer.Equals(k, key))
                    {
                        // found

                        index = i;
                        break;
                    }
                }

                var valueComparer = this.GetPropertyValueEqualityComparer<TValue>("Item") ?? EqualityComparer<TValue>.Default;
                if (!valueComparer.Equals(oldValue, value))
                {
                    this.RaisePropertyChanged("Item");
                    this.RaisePropertyChanged(() => this.Values);
                }

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Replace,
                                                             newItem: value, oldItem: oldValue, index: index);
                this.RaiseCollectionChanged(e);
            }
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Values" />
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return this.BaseCollection.Values; }
        }

        object IDictionary.this[object key]
        {
            get { return this[(TKey)key]; }

            set { this[(TKey)key] = (TValue)value; }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get { return this.Values; }
        }

        ICollection IDictionary.Values
        {
            get { return AsCollection(this.Values); }
        }

        #endregion Properties (10)

        #region Methods (11)

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            this.BaseCollection.Add(key, value);
            this.RaiseCollectionEvents();

            var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Add,
                                                         changedItem: new KeyValuePair<TKey, TValue>(key, value));
            this.RaiseCollectionChanged(e);
        }

        void IDictionary.Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.ContainsKey(TKey)" />
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return this.BaseCollection.ContainsKey(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            if (this.BaseCollection is IDictionary)
            {
                return ((IDictionary)this.BaseCollection).GetEnumerator();
            }

            return new Dictionary<TKey, TValue>(this.BaseCollection).GetEnumerator();
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
                foreach (var i in items)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// <see cref="NotifiableCollection{T}.RaiseCollectionEvents()" />
        /// </summary>
        protected override void RaiseCollectionEvents()
        {
            base.RaiseCollectionEvents();

            this.RaisePropertyChanged(() => this.Keys);
            this.RaisePropertyChanged(() => this.Values);
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.Remove(TKey)" />
        /// </summary>
        public bool Remove(TKey key)
        {
            var oldValue = this.TryGetOldValue(key);

            var result = this.BaseCollection.Remove(key);

            if (result)
            {
                this.RaiseCollectionEvents();

                var e = new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Remove,
                                                             changedItem: oldValue);
                this.RaiseCollectionChanged(e);
            }

            return result;
        }

        void IDictionary.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        [DebuggerStepThrough]
        private TValue TryGetOldValue(TKey key)
        {
            TValue result;
            try
            {
                result = this[key];
            }
            catch
            {
                result = default(TValue);
            }

            return result;
        }

        /// <summary>
        /// <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.BaseCollection.TryGetValue(key, out value);
        }

        #endregion Methods (11)
    }
}
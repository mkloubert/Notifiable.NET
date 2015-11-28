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
using System.Reflection;

namespace MarcelJoachimKloubert.Extensions
{
    internal class PropertyChangedContext<TObj, TProperty> : IPropertyChangedContext<TObj, TProperty>
        where TObj : global::System.ComponentModel.INotifyPropertyChanged
    {
        #region Fields (1)

        private PropertyChangedEventHandler _handler;

        #endregion Fields (1)

        #region Constructors (1)

        ~PropertyChangedContext()
        {
            Dispose(false);
        }

        #endregion Constructors (1)

        #region Properties (6)

        internal Action<IPropertyChangedContext<TObj, TProperty>> Action
        {
            set
            {
                var oldHandler = _handler;
                if (oldHandler != null)
                {
                    Object.PropertyChanged -= oldHandler;
                }

                var newHandler = value != null ? new PropertyChangedEventHandler((sender, e) =>
                    {
                        value(this);
                    }) : null;

                _handler = null;
                if (newHandler != null)
                {
                    Object.PropertyChanged += _handler = newHandler;
                }
            }
        }

        public TObj Object { get; internal set; }

        internal PropertyInfo Property { get; set; }

        string IPropertyChangedContext<TObj>.Property
        {
            get { return Property.Name; }
        }

        public TProperty Value
        {
            get { return (TProperty)Property.GetValue(Object, null); }
        }

        object IPropertyChangedContext<TObj>.Value
        {
            get { return Value; }
        }

        #endregion Properties (6)

        #region Methods (3)

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            try
            {
                Unregister();
            }
            catch
            {
                if (disposing)
                {
                    throw;
                }
            }
        }

        public void Unregister()
        {
            Action = null;
        }

        #endregion Methods (3)
    }
}
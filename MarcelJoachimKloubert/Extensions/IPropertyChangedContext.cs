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

namespace MarcelJoachimKloubert.Extensions
{
    #region INTERFACE: IPropertyChangeContext<out TObj>

    /// <summary>
    /// Describes a context for an <see cref="INotifyPropertyChanged" /> object.
    /// </summary>
    /// <typeparam name="TObj">Type of the object.</typeparam>
    public interface IPropertyChangedContext<out TObj> : IDisposable
        where TObj : global::System.ComponentModel.INotifyPropertyChanged
    {
        #region Properties (3)

        /// <summary>
        /// Gets the underlying object.
        /// </summary>
        TObj Object { get; }

        /// <summary>
        /// Gets the name of the property that has been changed.
        /// </summary>
        string Property { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        object Value { get; }

        #endregion Properties (3)

        #region Methods (1)

        /// <summary>
        /// Unregisters the underlying action.
        /// </summary>
        void Unregister();

        #endregion Methods (1)
    }

    #endregion INTERFACE: IPropertyChangeContext<out TObj>

    #region INTERFACE: IPropertyChangeContext<out TObj, TProperty>

    /// <summary>
    /// Describes a context for an <see cref="INotifyPropertyChanged" /> object.
    /// </summary>
    /// <typeparam name="TObj">Type of the object.</typeparam>
    /// <typeparam name="TProperty">Type of the property.</typeparam>
    public interface IPropertyChangedContext<out TObj, out TProperty> : IPropertyChangedContext<TObj>
        where TObj : global::System.ComponentModel.INotifyPropertyChanged
    {
        #region Properties (1)

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        new TProperty Value { get; }

        #endregion Properties (1)
    }

    #endregion INTERFACE: IPropertyChangeContext<out TObj, TProperty>
}
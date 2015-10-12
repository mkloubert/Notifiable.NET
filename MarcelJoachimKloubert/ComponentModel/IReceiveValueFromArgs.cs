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

using System;

namespace MarcelJoachimKloubert.ComponentModel
{
    /// <summary>
    /// Arguments for a method that receives values via <see cref="ReceiveValueFromAttribute" />.
    /// </summary>
    public interface IReceiveValueFromArgs
    {
        #region Data members (4)

        /// <summary>
        /// The new value.
        /// </summary>
        object NewValue { get; }

        /// <summary>
        /// The old value.
        /// </summary>
        object OldValue { get; }

        /// <summary>
        /// The name of the sending element / property.
        /// </summary>
        string SenderName { get; }

        /// <summary>
        /// The target type.
        /// </summary>
        Type TargetType { get; }

        #endregion Data members (4)

        #region Methods (2)

        /// <summary>
        /// Gets the value of <see cref="IReceiveValueFromArgs.NewValue" /> property strong typed.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns>The casted value of <see cref="IReceiveValueFromArgs.NewValue" /> property.</returns>
        TTarget GetNewValue<TTarget>();

        /// <summary>
        /// Gets the value of <see cref="IReceiveValueFromArgs.OldValue" /> property strong typed.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <returns>The casted value of <see cref="IReceiveValueFromArgs.OldValue" /> property.</returns>
        TTarget GetOldValue<TTarget>();

        #endregion Methods (2)
    }
}
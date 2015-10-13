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
using System.Runtime.InteropServices;

namespace MarcelJoachimKloubert.ComponentModel
{
    partial class NotifiableBase
    {
        private class ReceiveValueFromArgs : IReceiveValueFromArgs
        {
            #region Properties (8)

            internal ReceiveValueFromAttribute Attribute
            {
                get;
                set;
            }

            public object NewValue
            {
                get;
                internal set;
            }

            internal NotifiableBase NotifiableObject
            {
                get;
                set;
            }

            public object OldValue
            {
                get;
                internal set;
            }

            public Action<IReceiveValueFromArgs, object> ResultHandler
            {
                get;
                set;
            }

            public string SenderName
            {
                get;
                internal set;
            }

            internal _MemberInfo TargetMember
            {
                get;
                set;
            }

            public Type TargetType
            {
                get;
                internal set;
            }

            #endregion Properties (8)

            #region Methods (2)

            public TTarget GetNewValue<TTarget>()
            {
                return this.NotifiableObject
                           .ConvertPropertyValue<TTarget>(propertyName: this.SenderName,
                                                          obj: this.NewValue);
            }

            public TTarget GetOldValue<TTarget>()
            {
                return this.NotifiableObject
                           .ConvertPropertyValue<TTarget>(propertyName: this.SenderName,
                                                          obj: this.OldValue);
            }

            #endregion Methods (2)
        }
    }
}
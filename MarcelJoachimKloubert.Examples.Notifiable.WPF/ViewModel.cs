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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MarcelJoachimKloubert.Examples.Notifiable.WPF
{
    /// <summary>
    /// A simple ViewModel.
    /// </summary>
    public class ViewModel : NotifiableBase
    {
        #region Fields (1)

        [ReceiveValueFrom("StringValue")]
        private string _stringValue;

        #endregion Fields (1)

        #region Properties (5)

        [ReceiveNotificationFrom("StringValue")]
        public string LowerCase
        {
            get;
            private set;
        }

        public string StringValue
        {
            get { return this.Get<string>(); }

            set { this.Set(value); }
        }

        [ReceiveNotificationFrom("StringValue")]
        public string StringValueField
        {
            get { return this._stringValue; }
        }

        [ReceiveValueFrom("StringValue")]
        public string TrimmedAndUpperCase
        {
            get { return this.Get<string>(); }

            private set
            {
                var newValue = value;
                if (newValue != null)
                {
                    newValue = newValue.ToUpper().Trim();
                }

                this.Set(newValue);
            }
        }

        [ReceiveNotificationFrom("StringValue")]
        public string UpperCase
        {
            get
            {
                var val = this.StringValue;
                if (val != null)
                {
                    val = val.ToUpper();
                }

                return val;
            }
        }

        #endregion Properties (5)

        #region Methods (2)

        protected override IDictionary<string, object> CreatePropertyStorage()
        {
            return new ConcurrentDictionary<string, object>();
        }

        [ReceiveValueFrom("StringValue")]
        protected void UpdateLowerCaseStringValue(IReceiveValueFromArgs args)
        {
            var val = (string)args.NewValue;
            if (val != null)
            {
                val = val.ToLower();
            }

            this.LowerCase = val;
        }

        #endregion Methods (2)
    }
}
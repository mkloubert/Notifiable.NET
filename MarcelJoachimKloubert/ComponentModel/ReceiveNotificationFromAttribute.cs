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
    /// Defines from where a property should receive notifications.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
                    AllowMultiple = true,
                    Inherited = false)]
    public class ReceiveNotificationFromAttribute : Attribute
    {
        #region Constructors (1)

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveNotificationFromAttribute" /> class.
        /// </summary>
        /// <param name="senderName">
        /// The value for the <see cref="ReceiveNotificationFromAttribute.SenderName" /> property.
        /// </param>
        /// <param name="options">
        /// The value for the <see cref="ReceiveNotificationFromAttribute.Options" /> property.
        /// </param>
        /// <param name="sortOrder">
        /// The value for the <see cref="ReceiveNotificationFromAttribute.SortOrder" /> property.
        /// </param>
        public ReceiveNotificationFromAttribute(string senderName, ReceiveFromOptions options = ReceiveFromOptions.Default, int sortOrder = 0)
        {
            this.Options = options;
            this.SenderName = senderName;
            this.SortOrder = sortOrder;
        }

        #endregion Constructors

        #region Properties (3)

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public ReceiveFromOptions Options
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of sender / sending member of the notification.
        /// </summary>
        public string SenderName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public int SortOrder
        {
            get;
            set;
        }

        #endregion Properties
    }
}
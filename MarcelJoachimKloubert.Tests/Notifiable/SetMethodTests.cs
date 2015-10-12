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
using NUnit.Framework;
using System;

namespace MarcelJoachimKloubert.Tests.Notifiable
{
    /// <summary>
    /// Set methods tests.
    /// </summary>
    public class SetMethodTests : TestFixtureBase
    {
        #region CLASS: TestObj1

        private class TestObj1 : NotifiableBase
        {
            public object Test1
            {
                get { return this.Get(() => this.Test1); }

                set { this.Set(value, () => this.Test1); }
            }
        }

        #endregion CLASS: TestObj1

        #region CLASS: TestObj2

        private class TestObj2 : NotifiableBase
        {
            public bool Method1Raised;
            public bool Method2Raised;

            public object Test1
            {
                get { return this.Get(() => this.Test1); }

                set { this.Set(value, () => this.Test1); }
            }

            [ReceiveValueFrom("Test1")]
            public void Method1()
            {
                this.Method1Raised = true;
            }

            public void Method2()
            {
                this.Method2Raised = true;
            }
        }

        #endregion CLASS: TestObj2

        #region Methods (2)

        [Test]
        public void Test1()
        {
            var test1PropertyRaised = false;

            var obj = new TestObj1();
            obj.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "Test1")
                    {
                        test1PropertyRaised = true;
                    }
                };

            Assert.AreEqual(false, test1PropertyRaised);

            obj.Test1 = true;

            Assert.AreEqual(true, test1PropertyRaised);

            test1PropertyRaised = false;

            obj.Test1 = true;
            Assert.AreEqual(false, test1PropertyRaised);

            test1PropertyRaised = false;

            obj.Test1 = false;
            Assert.AreEqual(true, test1PropertyRaised);
        }

        [Test]
        public void Test2()
        {
            var obj2 = new TestObj2();

            Assert.AreEqual(false, obj2.Method1Raised);
            Assert.AreEqual(false, obj2.Method2Raised);

            obj2.Test1 = DateTimeOffset.Now;

            Assert.AreEqual(true, obj2.Method1Raised);
            Assert.AreEqual(false, obj2.Method2Raised);
        }

        #endregion Methods (2)
    }
}
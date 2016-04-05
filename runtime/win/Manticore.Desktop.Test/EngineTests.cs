using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Manticore.Test
{
    public class EngineFixture : IDisposable
    {
        public EngineFixture()
        {
            JsBackedObject.CreateManticoreEngine(SampleScript, "index.js");
        }

        public void Dispose()
        {
            JsBackedObject.Engine.Shutdown();
        }

        string SampleScript
        {
            get
            {
#if WINDOWS_APP
                return new StreamReader(typeof(EngineTests).GetTypeInfo().Assembly.GetManifestResourceStream("Manticore.Test.index.pack.js")).ReadToEnd();
#elif WINDOWS_PHONE_APP
                return new StreamReader(typeof(EngineTests).GetTypeInfo().Assembly.GetManifestResourceStream("Manticore.Test.index.pack.js")).ReadToEnd();
#else
#if DOTNET_4
                String src = "Manticore.Net4.Test.index.pack.js";
#else
                String src = "Manticore.Desktop.Test.index.pack.js";
#endif
                String javascript;
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(src))
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        javascript = reader.ReadToEnd();
                    }
                }
                return javascript;
#endif
            }
        }
    }

    public class EngineTests : IClassFixture<EngineFixture>
    {
        public EngineTests(EngineFixture fixture, ITestOutputHelper output)
        {
            output.WriteLine("Creating test instance");
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_MakeEngineTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_MakeEngineTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_MakeEngineTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_MakeEngineTest()
#endif
        {
            var engine = new ManticoreEngine();
            Assert.NotNull(engine);
            engine.Shutdown();
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_LoadJsTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_LoadJsTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_LoadJsTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_LoadJsTest()
#endif
        {
            var engine = new ManticoreEngine();
            engine.LoadScript("{}", "index.js");
            engine.Shutdown();
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_PropertyTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_PropertyTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_PropertyTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_PropertyTest()
#endif
        {
            SDKTestDefault simple = new SDKTestDefault();
            Assert.Equal(1, simple.Test);
            Assert.True(simple.ItsTrue);
            Assert.False(simple.ItsFalse);
            Assert.Null(simple.BlankDecimal);
            Assert.Equal(0, simple.BlankInt);
            Assert.Equal(1, simple.IntOne);
            Assert.Equal(Decimal.Parse("100.01"), simple.DecimalHundredOhOne);

            SDKTest tester = new SDKTest("STRINGISHERE");
            Assert.Equal(1, tester.ItsOne);
            Assert.Equal("STRINGISHERE", tester.StringProperty);
            Assert.Null(tester.AccessorString);
            Assert.NotNull(tester.ComplexType);
            Assert.Equal(Decimal.Parse("100.01"), tester.ComplexType
                .DecimalHundredOhOne);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_FunctionCallTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_FunctionCallTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_FunctionCallTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_FunctionCallTest()
#endif
        {
            var test = new SDKTest("");
            var d = test.ReturnAnObject();
            Assert.NotNull(d);
            Assert.IsAssignableFrom(typeof(SDKTestDefault), d);
            Assert.True(d.IsItTrue());
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_StaticFunctionCallTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_StaticFunctionCallTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_StaticFunctionCallTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_StaticFunctionCallTest()
#endif
        {
            var d = SDKTest.StaticMethod();
            Assert.NotNull(d);
            Assert.IsAssignableFrom(typeof(SDKTest), d);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_PreDecrementTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_PreDecrementTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_PreDecrementTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_PreDecrementTest()
#endif
        {
            var test = new SDKTest("");
            var d = test.PreDecrement(0, 1, 10);
            Assert.Equal(10, d[0]);
            Assert.Equal(1, d[1]);
            Assert.Equal(0, d[2]);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_PostDecrementTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_PostDecrementTest()
#elif DOTNET_4
        public void Net4_PostDecrementTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_PostDecrementTest()
#endif
        {
            var test = new SDKTest("");
            var d = test.PostDecrement(0,1,10);
            Assert.Equal(0, d[0]);
            Assert.Equal(10, d[1]);
            Assert.Equal(0, d[2]);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_CallbackTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_CallbackTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_CallbackTest()
#else
        public void Desktop_CallbackTest()
#endif
        {
            String testing = "Testing123";
            var tester = new SDKTest(testing);

            var latch = new ManualResetEvent(false);
            tester.Echo(testing, (e, a) =>
            {
                Assert.Null(e);
                Assert.Equal(testing, a);
                latch.Set();
            });
            Assert.True(latch.WaitOne(1000));
            latch.Reset();

            tester.EchoWithSetTimeout(testing, (e, a) =>
            {
                Assert.Null(e);
                Assert.Equal(testing, a);
                latch.Set();
            });
            Assert.True(latch.WaitOne(1000));
            latch.Reset();
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_EventTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_EventTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_EventTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_EventTest()
#endif
        {
            var tester = new SDKTest("123");
            var latch = new ManualResetEvent(false);
            var calledOnce = false;
            var fakeDelegate = new SDKTest.FakeEventDelegate((sender, item) =>
            {
                Assert.False(calledOnce);
                calledOnce = true;
                Assert.NotNull(item);
                Assert.Equal(1, item.Test);
                latch.Set();
            });
            tester.FakeEvent += fakeDelegate;
            tester.TriggerFakeAfterTimeout();
            Assert.True(latch.WaitOne(1000));
            latch.Reset();

            tester.FakeEvent -= fakeDelegate;
            tester.TriggerFakeAfterTimeout();
#if WINDOWS_PHONE_APP || WINDOWS_APP
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
#else
            Thread.Sleep(1000);
#endif
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_CollectionTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_CollectionTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_CollectionTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_CollectionTest()
#endif
        {
            var d = new SDKTestDefault();
            var arr = d.StringArray;

            Assert.Equal(3, arr.Count);

            arr.Add("helloworld");
            d.StringArray = arr;
            Assert.Equal(4, d.StringArray.Count);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_ExceptionTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_ExceptionTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_ExceptionTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_ExceptionTest()
#endif
        {
            var d = new SDKTest("123");
            try
            {
                d.ThrowOne();
                Assert.True(false);
            }
            catch (ManticoreException)
            {
                // That's what we wanted.
            }
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_DictionaryTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_DictionaryTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_DictionaryTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_DictionaryTest()
#endif
        {
            var d = new SDKTest("123");
            var dict = d.ReturnAMixedType();

            Verify(dict);

            Verify(d.TakeAMixedType(dict));
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_InheritanceTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_InheritanceTest()
#elif DOTNET_4
        public void Net4_InheritanceTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_InheritanceTest()
#endif
        {
            var tester = new SDKTest("123");
            var derived = tester.ReturnADerivedObject();
            Assert.True(derived is SDKTestDefault);
            Assert.True(derived is SDKTestDefaultSubclass);
            var both = tester.ReturnBaseAndDerived();
            Assert.True(both[0] is SDKTestDefault);
            Assert.False(both[0] is SDKTestDefaultSubclass);
            Assert.True(both[1] is SDKTestDefault);
            Assert.True(both[1] is SDKTestDefaultSubclass);
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public void Phone_FetchTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public void Win81_FetchTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public void Net4_FetchTest()
#else
        [Trait("Category", "Desktop")]
        public void Desktop_FetchTest()
#endif
        {
            var latch = new ManualResetEvent(false);
            var tester = new SDKTest("123");
            Exception encountered = null;
            tester.GoFetch(new SDKTest.FetchedDelegate((x, r) =>
            {
                try
                {
                    Assert.Null(x);
                    Assert.NotNull(r);
                    Assert.NotNull(r["args"]);
                    Assert.IsAssignableFrom(typeof(IDictionary<String, Object>), r["args"]);
                    Assert.Equal(((IDictionary<String,Object>)r["args"])["foo"], "bar");
                }
                catch (Exception assertExc)
                {
                    encountered = assertExc;
                }
                finally
                {
                    latch.Set();
                }
            }));
            Assert.True(latch.WaitOne(30000));
            if (encountered != null)
            {
                throw encountered;
            }
        }

        [Fact]
#if WINDOWS_PHONE_APP
        [Trait("Category", "WinPhone")]
        public async Task Phone_FetchPTest()
#elif WINDOWS_APP
        [Trait("Category", "WinStore")]
        public async Task Win81_FetchPTest()
#elif DOTNET_4
        [Trait("Category", "Net4")]
        public async void Net4_FetchPTest()
#else
        [Trait("Category", "Desktop")]
        public async Task Desktop_FetchPTest()
#endif
        {
            var tester = new SDKTest("123");
            var response = await tester.GoFetchP();
            Assert.NotNull(response);
            Assert.NotNull(response["args"]);
            Assert.IsAssignableFrom(typeof(IDictionary<String, Object>), response["args"]);
            Assert.Equal(((IDictionary<String, Object>)response["args"])["baz"], "bop");
        }

        private void Verify(IDictionary<String, Object> dict)
        {
            Assert.Equal(4, (int)dict["anInt"]);
            Assert.Equal(1.1, (double)dict["aFloat"]);
            Assert.Equal("testing", dict["aString"].ToString());
            Assert.Equal(true, (bool)dict["aBool"]);
            Assert.Null(dict["aNull"]);
            Assert.IsAssignableFrom(typeof(IDictionary<String, Object>), dict["anObject"]);
            Assert.IsAssignableFrom(typeof(IDictionary<String, Object>), dict["aMixed"]);
        }
    }
}

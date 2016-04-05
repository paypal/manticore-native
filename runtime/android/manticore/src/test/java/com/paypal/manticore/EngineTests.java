/*-------------------------------------------------------------------------------------------------------------------*\
 |  Copyright (C) 2015 PayPal                                                                                          |
 |                                                                                                                     |
 |  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance     |
 |  with the License.                                                                                                  |
 |                                                                                                                     |
 |  You may obtain a copy of the License at                                                                            |
 |                                                                                                                     |
 |       http://www.apache.org/licenses/LICENSE-2.0                                                                    |
 |                                                                                                                     |
 |  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed   |
 |  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for  |
 |  the specific language governing permissions and limitations under the License.                                     |
 \*-------------------------------------------------------------------------------------------------------------------*/
package com.paypal.manticore;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import android.content.Context;
import com.eclipsesource.v8.V8Value;
import junit.framework.Assert;
import okhttp3.ConnectionSpec;
import okhttp3.OkHttpClient;

public class EngineTests
{
  public void makeEngineTest(Context context)
  {
    ManticoreEngine me = new ManticoreEngine().start(context);
    Assert.assertNotNull(me);
    me.shutDown();
  }


  public void loadJsTest(Context context, String script)
  {
    final ManticoreEngine me = new ManticoreEngine();

    for (int i = 0; i < 2; i++)
    {
      me.start(context);
      me.loadScript(script, "index.pack.js");
      me.getExecutor().run(new Runnable()
      {
        @Override
        public void run()
        {
          Assert.assertEquals(me.v8.getType("SDKTest"), V8Value.V8_FUNCTION);
        }
      });
      me.shutDown();
    }
  }

  public void pluginTest(Context context, String script) {
    final StringBuilder builder = new StringBuilder();
    IManticoreObserver observer = new IManticoreObserver()
    {
      @Override
      public void willLoadPolyfill(ManticoreEngine engine)
      {
        builder.append("willLoadPoly,");
      }


      @Override
      public void didLoadPolyfill(ManticoreEngine engine)
      {
        builder.append("didLoadPoly,");
      }


      @Override
      public void willLoadScript(ManticoreEngine engine, String script, String name)
      {
        builder.append("willLoadScript,").append(name).append(',');
      }


      @Override
      public void didLoadScript(ManticoreEngine engine, String script, String name)
      {
        builder.append("didLoadScript,").append(name).append(',');
      }
    };

    ManticoreEngine me = new ManticoreEngine().start(context);
    me.addObserver(observer);
    me.loadScript(script, "index.pack.js");
    me.shutDown();
    Assert.assertEquals("willLoadPoly,didLoadPoly,willLoadScript,index.pack.js,didLoadScript,index.pack.js,", builder.toString());
  }

  public void verifyDefaults(SDKTestDefault simple)
  {
    Assert.assertNotNull(simple);
    Assert.assertEquals(1, simple.getTest().intValue());
    Assert.assertTrue(simple.getItsTrue());
    Assert.assertFalse(simple.getItsFalse());
    Assert.assertNull(simple.getBlankDecimal());
    Assert.assertEquals(0, simple.getBlankInt().intValue());
    Assert.assertEquals(1, simple.getIntOne().longValue());
    Assert.assertEquals(new BigDecimal("100.01").toString(), simple.getDecimalHundredOhOne().toString());
  }


  public void propertyTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTestDefault simple = new SDKTestDefault();
    verifyDefaults(simple);

    SDKTest tester = new SDKTest("STRINGISHERE");
    Assert.assertEquals(1, tester.getItsOne().intValue());
    Assert.assertEquals("STRINGISHERE", tester.getStringProperty());
    Assert.assertNull(tester.getAccessorString());
    Assert.assertNotNull(tester.getComplexType());
    Assert.assertEquals(new BigDecimal("100.01").toString(), tester.getComplexType().getDecimalHundredOhOne().toString());
  }


  public void functionCallTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest test = new SDKTest("");
    SDKTestDefault d = test.returnAnObject();
    verifyDefaults(d);

    JsBackedObject.getEngine().shutDown();
  }


  public void callbackTest(Context context, String script) throws Throwable
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    final String testing = "Testing123";
    SDKTest tester = new SDKTest(testing);
    final CountDownLatch latch = new CountDownLatch(1);
    final List<Throwable> failure = new ArrayList<>();
    tester.echo(testing, new SDKTest.EchoCallback()
    {
      @Override
      public void echo(ManticoreException error, String arg)
      {
        try
        {
          Assert.assertNull(error);
          Assert.assertEquals(testing, arg);
        }
        catch (Throwable t)
        {
          failure.add(t);
        }
        finally
        {
          latch.countDown();
        }
      }
    });
    latch.await(1, TimeUnit.SECONDS);
    if (failure.size() > 0)
    {
      throw failure.get(0);
    }

    failure.clear();
    final CountDownLatch latch2 = new CountDownLatch(1);
    tester.echoWithSetTimeout(testing, new SDKTest.EchoCallback()
    {
      @Override
      public void echo(ManticoreException error, String arg)
      {
        try
        {
          Assert.assertNull(error);
          Assert.assertEquals(testing, arg);
        }
        catch (Throwable t)
        {
          failure.add(t);
        }
        finally
        {
          latch2.countDown();
        }
      }
    });
    latch.await(1, TimeUnit.SECONDS);
    if (failure.size() > 0)
    {
      throw failure.get(0);
    }

    JsBackedObject.getEngine().shutDown();
  }


  public void eventTest(Context context, String script) throws Throwable
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest tester = new SDKTest("123");
    final CountDownLatch latch = new CountDownLatch(1);
    final List<Throwable> failure = new ArrayList<>();
    SDKTest.FakeEventObserver fake = new SDKTest.FakeEventObserver()
    {
      public boolean calledOnce;


      @Override
      public void fakeEvent(SDKTestDefault item)
      {
        try
        {
          Assert.assertFalse(calledOnce);
          calledOnce = true;
          Assert.assertNotNull(item);
          Assert.assertEquals(1, item.getTest().intValue());
        }
        catch (Throwable t)
        {
          failure.add(t);
        }
        finally
        {
          latch.countDown();
        }
      }
    };

    tester.addFakeEventObserver(fake);
    tester.triggerFakeAfterTimeout();
    latch.await(1, TimeUnit.SECONDS);
    if (failure.size() > 0)
    {
      throw failure.get(0);
    }

    tester.removeFakeEventObserver(fake);
    tester.triggerFakeAfterTimeout();
    Thread.sleep(1000);

    if (failure.size() > 0)
    {
      throw failure.get(0);
    }

    JsBackedObject.getEngine().shutDown();
  }


  public void collectionTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTestDefault d = new SDKTestDefault();

    List<String> strArray = d.getStringArray();
    Assert.assertEquals(3, strArray.size());

    strArray.add("foobar");
    d.setStringArray(strArray);
    Assert.assertEquals(4, d.getStringArray().size());
  }


  public void staticMethodTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest t = SDKTest.staticMethod();
    verifyDefaults(t.returnAnObject());
  }


  public void objectTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    JsBackedObject.getEngine().getExecutor().run(new Runnable()
    {
      @Override
      public void run()
      {
        verify(new SDKTest("123").returnAMixedType());
      }
    });
  }

  public void dateTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    JsBackedObject.getEngine().getExecutor().run(new Runnable()
    {
      @Override
      public void run()
      {
        DefaultTypeConverter defaultTypeConverter = new DefaultTypeConverter(JsBackedObject.getEngine());
        Date d = Calendar.getInstance().getTime();
        Assert.assertNotNull(defaultTypeConverter.asJsDate(d));
      }
    });
  }


  private void verify(Map<String, ? super Object> dict)
  {
    Assert.assertEquals(4, (int) dict.get("anInt"));
    Assert.assertEquals(1.1, dict.get("aFloat"));
    Assert.assertEquals("testing", dict.get("aString").toString());
    Assert.assertEquals(true, (boolean) dict.get("aBool"));
    Assert.assertNull(dict.get("aNull"));
    Assert.assertEquals("This is an SDK Default object: true", dict.get("anObject"));
    Assert.assertEquals("[object Object]", dict.get("aMixed"));
  }


  public void inheritanceTest(Context context, String script)
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest t = new SDKTest("testing");
    SDKTestDefault def = t.returnADerivedObject();
    Assert.assertTrue(def instanceof SDKTestDefaultSubclass);
    List<SDKTestDefault> both = t.returnBaseAndDerived();
    Assert.assertEquals(2, both.size());
    Assert.assertFalse(both.get(0) instanceof SDKTestDefaultSubclass);
    Assert.assertTrue(both.get(1) instanceof SDKTestDefaultSubclass);
  }


  public void fetchTest(Context context, String script) throws Throwable
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");
    JsBackedObject.getEngine().nativeServices.injectHttpClient(
        new OkHttpClient.Builder()
            .connectionSpecs(Collections.singletonList(ConnectionSpec.CLEARTEXT))
            .build());

    final List<Throwable> asyncExceptions = new ArrayList<>();
    SDKTest t = new SDKTest("testing");
    t.setNoSsl(true);
    final CountDownLatch latch = new CountDownLatch(1);
    t.goFetch(new SDKTest.FetchedCallback()
    {
      @Override
      @SuppressWarnings("unchecked")
      public void fetched(ManticoreException error, Map<String, ? super Object> response)
      {
        try
        {
          Assert.assertNull(error);
          Assert.assertNotNull(response);
          Object rawArgs = response.get("args");
          Assert.assertTrue(rawArgs instanceof Map);
          Map<String, Object> args = (Map<String, Object>) rawArgs;
          Assert.assertNotNull(args);
          Assert.assertEquals(args.get("foo"), "bar");
          Assert.assertTrue(true);
        }
        catch (Throwable x)
        {
          asyncExceptions.add(x);
        }
        finally
        {
          latch.countDown();
        }
      }
    });
    latch.await(30, TimeUnit.SECONDS);
    if (asyncExceptions.size() > 0)
    {
      throw asyncExceptions.get(0);
    }
    JsBackedObject.getEngine().shutDown();
  }


  public void fetchPTest(Context context, String script) throws Throwable
  {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");
    JsBackedObject.getEngine().nativeServices.injectHttpClient(
        new OkHttpClient.Builder()
            .connectionSpecs(Collections.singletonList(ConnectionSpec.CLEARTEXT))
            .build());

    SDKTest t = new SDKTest("testing");
    t.setNoSsl(true);
    final CountDownLatch latch = new CountDownLatch(1);
    final List<Throwable> asyncExceptions = new ArrayList<>();
    t.goFetchP(new SDKTest.GoFetchPCallback()
    {
      @Override
      @SuppressWarnings("unchecked")
      public void done(ManticoreException error, Map<String, ? super Object> response)
      {
        try
        {
          Assert.assertNull(error);
          Assert.assertNotNull(response);
          Object rawArgs = response.get("args");
          Assert.assertTrue(rawArgs instanceof Map);
          Map<String, Object> args = (Map<String, Object>) rawArgs;
          Assert.assertNotNull(args);
          Assert.assertEquals(args.get("baz"), "bop");
          Assert.assertTrue(true);
        }
        catch (Throwable x)
        {
          asyncExceptions.add(x);
        }
        finally
        {
          latch.countDown();
        }
      }
    });
    latch.await(30, TimeUnit.SECONDS);
    if (asyncExceptions.size() > 0)
    {
      throw asyncExceptions.get(0);
    }

    JsBackedObject.getEngine().shutDown();
  }
}

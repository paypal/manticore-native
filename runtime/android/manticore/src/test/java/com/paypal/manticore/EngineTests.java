package com.paypal.manticore;

import java.math.BigDecimal;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import android.content.Context;
import com.eclipsesource.v8.V8Value;
import junit.framework.Assert;

/**
 * Created by mmetral on 6/11/15.
 */
public class EngineTests {
  public void makeEngineTest(Context context) {
    ManticoreEngine me = new ManticoreEngine(context);
    Assert.assertNotNull(me);
    me.shutDown();
  }

  public void loadJsTest(Context context, String script) {
    final ManticoreEngine me = new ManticoreEngine(context);

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

  public void verifyDefaults(SDKTestDefault simple) {
    Assert.assertNotNull(simple);
    Assert.assertEquals(1, simple.getTest().intValue());
    Assert.assertTrue(simple.getItsTrue());
    Assert.assertFalse(simple.getItsFalse());
    Assert.assertNull(simple.getBlankDecimal());
    Assert.assertEquals(0, simple.getBlankInt().intValue());
    Assert.assertEquals(1, simple.getIntOne().longValue());
    Assert.assertEquals(new BigDecimal("100.01").toString(), simple.getDecimalHundredOhOne().toString());
  }

  public void propertyTest(Context context, String script) {
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

  public void functionCallTest(Context context, String script) {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest test = new SDKTest("");
    SDKTestDefault d = test.returnAnObject();
    verifyDefaults(d);

    JsBackedObject.getEngine().shutDown();
  }

  public void callbackTest(Context context, String script) throws Exception {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    final String testing = "Testing123";
    SDKTest tester = new SDKTest(testing);
    final CountDownLatch latch = new CountDownLatch(1);
    tester.echo(testing, new SDKTest.EchoCallback() {
      @Override
      public void echo(ManticoreException error, String arg) {
        Assert.assertNull(error);
        Assert.assertEquals(testing, arg);
        latch.countDown();
      }
    });
    latch.await(1, TimeUnit.SECONDS);

    final CountDownLatch latch2 = new CountDownLatch(1);
    tester.echoWithSetTimeout(testing, new SDKTest.EchoCallback() {
      @Override
      public void echo(ManticoreException error, String arg) {
        Assert.assertNull(error);
        Assert.assertEquals(testing, arg);
        latch2.countDown();
      }
    });
    latch.await(1, TimeUnit.SECONDS);

    JsBackedObject.getEngine().shutDown();
  }

  public void eventTest(Context context, String script) throws Exception {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest tester = new SDKTest("123");
    final CountDownLatch latch = new CountDownLatch(1);
    SDKTest.FakeEventObserver fake = new SDKTest.FakeEventObserver() {
      public boolean calledOnce;
      @Override
      public void fakeEvent(SDKTestDefault item) {
        Assert.assertFalse(calledOnce);
        calledOnce = true;
        Assert.assertNotNull(item);
        Assert.assertEquals(1, item.getTest().intValue());
        latch.countDown();
      }
    };

    tester.addFakeEventObserver(fake);
    tester.triggerFakeAfterTimeout();
    latch.await(1, TimeUnit.SECONDS);

    tester.removeFakeEventObserver(fake);
    tester.triggerFakeAfterTimeout();
    Thread.sleep(1000);

    JsBackedObject.getEngine().shutDown();
  }

  public void collectionTest(Context context, String script) {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTestDefault d = new SDKTestDefault();

    List<String> strArray = d.getStringArray();
    Assert.assertEquals(3, strArray.size());

    strArray.add("foobar");
    d.setStringArray(strArray);
    Assert.assertEquals(4, d.getStringArray().size());
  }

  public void staticMethodTest(Context context, String script) {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest t = SDKTest.staticMethod();
    verifyDefaults(t.returnAnObject());
  }

  public void objectTest(Context context, String script) {
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

  public void invoiceTest(Runnable runnable) {
    JsBackedObject.createManticoreEngine(BaseTest._mockContext, BaseTest._testJs, "index.pack.js");

    JsBackedObject.getEngine().getExecutor().run(runnable);
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

  private void verify(Map<String,? super Object> dict) {
    Assert.assertEquals(4, (int) dict.get("anInt"));
    Assert.assertEquals(1.1, (double) dict.get("aFloat"));
    Assert.assertEquals("testing", dict.get("aString").toString());
    Assert.assertEquals(true, (boolean) dict.get("aBool"));
    Assert.assertNull(dict.get("aNull"));
    Assert.assertEquals("This is an SDK Default object: true", dict.get("anObject"));
    Assert.assertEquals("[object Object]", dict.get("aMixed"));
  }

  public void inheritanceTest(Context context, String script) {
    JsBackedObject.createManticoreEngine(context, script, "index.pack.js");

    SDKTest t = new SDKTest("testing");
    SDKTestDefault def = t.returnADerivedObject();
    Assert.assertTrue(def instanceof SDKTestDefault);
    Assert.assertTrue(def instanceof SDKTestDefaultSubclass);
    List<SDKTestDefault> both = t.returnBaseAndDerived();
    Assert.assertEquals(2, both.size());
    Assert.assertTrue(both.get(0) instanceof SDKTestDefault);
    Assert.assertFalse(both.get(0) instanceof SDKTestDefaultSubclass);
    Assert.assertTrue(both.get(1) instanceof SDKTestDefault);
    Assert.assertTrue(both.get(1) instanceof SDKTestDefaultSubclass);
  }
}

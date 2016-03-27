package com.paypal.manticore;


import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

import android.content.Context;
import android.os.Build;
import android.util.Log;
import com.eclipsesource.v8.V8;
import com.eclipsesource.v8.V8Array;
import com.eclipsesource.v8.V8Function;
import com.eclipsesource.v8.V8Object;

/**
 * A host for a J2V8 VM that communicates with native classes and proxy objects. Your Javascript will have
 * access to a "manticore" global object that exposes native methods. ManticoreEngine will add the following
 * by default (you can replace them if you wish, after construction and typically before loadScript).
 *
 *  log(level {string}, component {string}, message {string}) - Log a JS-generated message to LogCat
 *  http(options {object}, callback {function(err,result)}) - Make an HTTP request and return the result in a callback
 *  export(items {object}) - key/value pairs of Javascript elements (usually classes) to make available to native code
 *  setTimeout(func {function()}, delayInMsec) - run a deferred task
 *  platform = "android"
 */
public class ManticoreEngine
{
  V8Array EmptyArray;

  com.eclipsesource.v8.V8 v8;
  JsExecutor executor;
  V8Object manticoreJsObject;
  V8Object exports;
  NativeServices nativeServices;
  IManticoreTypeConverter converter;

  public ManticoreEngine(final Context androidContext) {
    executor = new JsExecutor();
    converter = new DefaultTypeConverter(this);

    executor.run(new Runnable()
    {
      @Override
      public void run()
      {
        // To enable JUnit tests, we need to handle both "real context" and fake context cases...
        if (androidContext.getApplicationInfo() != null)
        {
          v8 = V8.createV8Runtime("global", androidContext.getApplicationInfo().dataDir);
        }
        else
        {
          v8 = V8.createV8Runtime("global");
        }
        EmptyArray = new V8Array(v8);
        manticoreJsObject = new V8Object(v8);
        v8.add("manticore", manticoreJsObject);

        V8Object platform = new V8Object(v8);
        manticoreJsObject.add("platform", platform);
        platform.add("name", "android");
        platform.add("version", Build.VERSION.RELEASE);

        exports = new V8Object(v8);
        v8.add("exports", exports);
        nativeServices = new NativeServices(ManticoreEngine.this);

        InputStream resourceStream = androidContext.getResources().openRawResource(R.raw.polyfill_pack);

        InputStreamReader inputreader = new InputStreamReader(resourceStream);
        BufferedReader jsReader = new BufferedReader(inputreader);

        StringBuilder fullJs = new StringBuilder();
        String line;
        try
        {
          while ((line = jsReader.readLine()) != null)
          {
            fullJs.append(line).append('\n');
          }
        } catch (IOException x) {
          Log.e("manticore", "Failed to load polyfill", x);
        }
        v8.executeVoidScript(fullJs.toString(), "polyfill_pack.js", 0);
      }
    });
  }

  public void loadScript(final String script, final String name) {
    executor.run(new Runnable()
    {
      @Override
      public void run()
      {
        v8.executeVoidScript(script, name, 0);
      }
    });
  }

  public void shutDown() {
    v8.terminateExecution();
    v8 = null;
    exports = null;
    manticoreJsObject = null;
  }

  public V8Array getEmptyArray() { return EmptyArray; }

  public V8Object getManticoreJsObject() {
    return manticoreJsObject;
  }

  public IManticoreTypeConverter getConverter() { return converter; }

  public void setConverter(IManticoreTypeConverter converter) {
    this.converter = converter;
  }

  public JsExecutor getExecutor() {
    return executor;
  }

  public V8Array createJsArray() {
    return new V8Array(v8);
  }

  public V8Object createJsObject() {
    return new V8Object(v8);
  }

  public V8Object asJsError(Exception x) {
    V8Object err = v8.executeObjectFunction("Error", createJsArray().push(x.toString()));
    // TODO copy the stack somehow.
    return err;
  }


  /**
   * You MUST call this from inside an executor. Since you're likely to want to DO something
   * with the return value, we don't do that internally to avoid unnecessary "asynchrony overhead"
   * @param className
   * @param constructorArguments
   * @return
   */
  public V8Object createJsObject(String className, V8Array constructorArguments) {
    V8Object constructor = getJSClass(className);
    if (constructor instanceof V8Function)
    {
      if (constructorArguments == null)
      {
        constructorArguments = EmptyArray;
      }
      return manticoreJsObject.executeObjectFunction("construct", createJsArray().push(constructor).push(constructorArguments));
    }
    return null;
  }


  public V8Object getJSClass(String className) {
    return exports.getObject(className);
  }

  public V8Object getJsObject(String objectName) {
    return v8.getObject(objectName);
  }
}

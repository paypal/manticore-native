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


import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;

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
  List<IManticoreObserver> plugins;
  String polyfillToLoad;
  boolean started = false;

  public ManticoreEngine() {
    executor = new JsExecutor();
    converter = new DefaultTypeConverter(this);
  }


  /**
   * You must call start before loading scripts into the engine.
   * @param androidContext The context in which to load the polyfills and other dependencies
   * @return this, for easier chaining.
   */
  public ManticoreEngine start(final Context androidContext) {
    started = true;
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
        v8.executeVoidScript("manticore.toString = function () { return \"MANTICORE\"; }");

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
        polyfillToLoad = fullJs.toString();
      }
    });
    return this;
  }


  /**
   * Load script into the engine with a given name
   * @param script The Javascript to load
   * @param name A name for stack traces
   * @return this, for easier chaining
   */
  public ManticoreEngine loadScript(final String script, final String name) {
    final String poly = polyfillToLoad;
    this.polyfillToLoad = null;
    executor.run(new Runnable()
    {
      @Override
      public void run()
      {
        List<IManticoreObserver> observers = null;
        if (poly != null) {
          observers = ManticoreEngine.this.plugins;
          if (observers != null) {
            for (IManticoreObserver o : observers)
            {
              o.willLoadPolyfill(ManticoreEngine.this);
            }
          }
          v8.executeVoidScript(poly.toString(), "polyfill_pack.js", 0);
          observers = ManticoreEngine.this.plugins;
          if (observers != null) {
            for (IManticoreObserver o : observers)
            {
              o.didLoadPolyfill(ManticoreEngine.this);
            }
          }
        }
        observers = ManticoreEngine.this.plugins;
        if (observers != null) {
          for (IManticoreObserver o : observers)
          {
            o.willLoadScript(ManticoreEngine.this, script, name);
          }
        }
        v8.executeVoidScript(script, name, 0);
        observers = ManticoreEngine.this.plugins;
        if (observers != null) {
          for (IManticoreObserver o : observers)
          {
            o.didLoadScript(ManticoreEngine.this, script, name);
          }
        }
      }
    });
    return this;
  }


  /**
   * Force a shutdown of all the v8 services. The object will NOT
   * be usable after this.
   */
  public void shutDown() {
    started = false;
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


  /**
   * Under the covers, JS async functions (marked with @async for native exposure) return promises and are code-gened
   * to take an extra parameter (callback). The resolvePromise function merely adds then() and catch() handlers
   * that call through to callback
   * @param promise The promise returned from the async method (or any promise)
   * @param callback The JS function (typically a wrapped Java interface) to be invoked when the promise
   *                 is accepted or rejected
   */
  public void resolvePromise(V8Object promise, V8Object callback) {
    manticoreJsObject.executeVoidFunction("asCallback", createJsArray().push(promise).push(callback));
  }

  public V8Object getJSClass(String className) {
    return exports.getObject(className);
  }

  public V8Object getJsObject(String objectName) {
    return v8.getObject(objectName);
  }


  /**
   * Add an object that will be notified of Manticore engine events. Note that
   * these events are handled syncrhonously - loading/execution will not continue
   * until the methods return.
   * @param observer The observer to be notified
   */
  public void addObserver(IManticoreObserver observer) {
    if (this.plugins == null) {
      this.plugins = new ArrayList<>();
    }
    this.plugins.add(observer);
  }

  /**
   * Remove an observer for manticore engine events
   * @param observer The observer previously passed to addObserver
   * @return true if found and removed, false otherwise
   */
  public boolean removeObserver(IManticoreObserver observer) {
    if (this.plugins != null) {
      return this.plugins.remove(observer);
    }
    return false;
  }
}

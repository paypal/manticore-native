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

import android.content.Context;
import com.eclipsesource.v8.V8Object;

/**
 * The JsBackedObject provides a common base class for your generated classes. If you are building an SDK and don't necessarily
 * want to expose the "Javascriptiness" of your system, you can define your own base class, make the methods only visible to your
 * package and just reimplement these methods with relevant custom types. Code generation and the compiler should collude to make the
 * rest of it just work...
 */
public class JsBackedObject
{
  V8Object impl;
  static ManticoreEngine engine;

  /**
   * All the classes generated against a particular base class must share a single Manticore engine.
   * This is because we expose raw constructors which need to instantiate JS objects without having
   * to pass the engine around all over the place.
   * @return the engine in use
   */
  public static ManticoreEngine getEngine() {
    return engine;
  }

  protected JsBackedObject(V8Object jsObject) {
    impl = jsObject;
  }

  protected JsBackedObject() { }

  /**
   * Create an engine given a blob of Javascript. If you need to add js-to-native services
   * via ManticoreJsObject, you will likely want to pass null for the script here because
   * your scripts probably rely on those services being available on load.
   * @param context
   * @param script
   */
  public static void createManticoreEngine(Context context, String script, String name) {
    ManticoreEngine me = new ManticoreEngine(context);
    me.setConverter(new DefaultTypeConverter(me));
    // Make sure to set the engine before loading the script because some JS things may call
    // BACK into Java upon loading and you need an engine to do stuff with those calls.
    if (script != null)
    {
      me.loadScript(script, name);
    }
    engine = me;
  }

}

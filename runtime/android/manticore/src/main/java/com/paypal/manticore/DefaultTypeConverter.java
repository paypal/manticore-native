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
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.eclipsesource.v8.V8Array;
import com.eclipsesource.v8.V8Object;
import com.eclipsesource.v8.V8Value;

public class DefaultTypeConverter implements IManticoreTypeConverter
{
  protected ManticoreEngine engine;

  public DefaultTypeConverter(ManticoreEngine engine) {
    this.engine = engine;
  }

  public <T> T asNative(Object value, Class<T> type)
  {
    if (value == null)
    {
      return null;
    }

    if (JsBackedObject.class.isAssignableFrom(type))
    {
      JsBackedObject returnValue = null;
      try
      {
        returnValue = (JsBackedObject) type.getDeclaredMethod("nativeInstanceForObject", V8Object.class).invoke(null, value);
      }
      catch (Exception e)
      {
        e.printStackTrace();
      }
      returnValue.impl = (V8Object) value;
      return type.cast(returnValue);
    }
    if (type == BigDecimal.class) {
      V8Object jsDecimal = (V8Object) value;
      // toString will just get [object object] here, so call it in JS explicitly
      String stringValue = jsDecimal.executeStringFunction("toString", null);
      return type.cast(new BigDecimal(stringValue));
    } else if (Exception.class.isAssignableFrom(type)) {

    }
    return type.cast(value);
  }

  public V8Object asJs(Object nativeInstance) {
    if (nativeInstance instanceof JsBackedObject) {
      return ((JsBackedObject)nativeInstance).impl;
    }
    return null;
  }

  /**
   * Convert a native BigDecimal to a JS string which JS will convert to its own
   * arbitrary precision format.
   */
  public String asJsDecimal(BigDecimal d) {
    return d.toString();
  }

  public BigDecimal asNativeDecimal(V8Object value) {
    return new BigDecimal(value.toString());
  }

  public Date asNativeDate(V8Object date)
  {
    return new Date(((Double) date.executeDoubleFunction("getTime", engine.EmptyArray)).longValue());
  }

  public V8Object asJsDate(Date d) {

    return engine.getManticoreJsObject().executeObjectFunction("newDate",
                                                               engine.createJsArray().push(d.getTime()));
  }

  public <T> V8Array toJsArray(List<T> nativeList, JsElementConverter<T> converter) {
    if (nativeList == null) {
      return null;
    }
    V8Array jsArray = new V8Array(engine.v8);
    for (T element : nativeList) {
      converter.push(jsArray, element);
    }
    return jsArray;
  }

  public <T> List<T> toNativeArray(V8Array jsArray, NativeElementConverter<T> converter)
  {
    if(jsArray == null) {
      return null;
    }
    List<T> nativeList = new ArrayList<T>();
    for (int i = 0; i < jsArray.length(); i++) {
      nativeList.add(converter.convert(jsArray.get(i)));
    }
    return nativeList;
  }

  public V8Object asJsObject(Map<String, ? super Object> map) {
    if (map == null) {
      return null;
    }
    V8Object dict = new V8Object(engine.v8);
    for (Map.Entry<String, ? super Object> kv : map.entrySet()) {
      Object v = kv.getValue();
      if (v == null) {
        dict.addNull(kv.getKey());
      } else if (v instanceof Double) {
        dict.add(kv.getKey(), (double) v);
      } else if (v instanceof Integer) {
        dict.add(kv.getKey(), (int) v);
      } else if (v instanceof Boolean) {
        dict.add(kv.getKey(), (boolean) v);
      } else if (v instanceof String) {
        dict.add(kv.getKey(), (String) v);
      } else {
        dict.add(kv.getKey(), asJs(v));
      }
    }
    return dict;
  }

  public Map<String,? super Object> asNativeObject(V8Object object) {
    HashMap<String,Object> mixed = new HashMap<>();
    String[] keys = object.getKeys();

    for (String k : keys) {
      // We make the call that _ prefixed properties do not cross the barrier
      if (k.length() > 0 && k.charAt(0) == '_') {
        continue;
      }
      int type = object.getType(k);
      switch (type)
      {
        case V8Value.DOUBLE:
          mixed.put(k, object.getDouble(k));
          break;
        case V8Value.INTEGER:
          mixed.put(k, object.getInteger(k));
          break;
        case V8Value.BOOLEAN:
          mixed.put(k, object.getBoolean(k));
          break;
        case V8Value.NULL:
          mixed.put(k, null);
          break;
        case V8Value.STRING:
          mixed.put(k, object.getString(k));
          break;
        default:
          mixed.put(k, object.getObject(k).toString());
          break;
      }
    }

    return mixed;
  }

}

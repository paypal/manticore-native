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
import java.util.List;
import java.util.Map;

import com.eclipsesource.v8.V8Array;
import com.eclipsesource.v8.V8Object;

public interface IManticoreTypeConverter
{
  <T> T asNative(Object value, Class<T> type);


  V8Object asJs(Object nativeInstance);

  /**
   * Convert a native BigDecimal to a JS string which JS will convert to its own
   * arbitrary precision format.
   */
  String asJsDecimal(BigDecimal d);

  BigDecimal asNativeDecimal(V8Object value);

  Date asNativeDate(V8Object date);

  V8Object asJsDate(Date d);

  Map<String,? super Object> asNativeObject(V8Object object);

  V8Object asJsObject(Map<String, ? super Object> map);

  <T> V8Array toJsArray(List<T> nativeList, JsElementConverter<T> converter);

  <T> List<T> toNativeArray(V8Array jsArray, NativeElementConverter<T> converter);

  /**
   * Used to convert an element of a native collection of elements to javascript equivalent.
   *
   * @param <T>
   */
  interface JsElementConverter<T>
  {
    void push(V8Array dest, T nativeValue);
  }


  /**
   * Used to convert an element of collection of JS elements to the native equivalent
   *
   * @param <T> The target native type
   */
  interface NativeElementConverter<T>
  {
    T convert(Object jsValue);
  }


}

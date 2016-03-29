using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Function;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using Manticore;

/**
 * SDKTestDefault.cs
 * 
 * DO NOT EDIT THIS FILE! IT IS AUTOMATICALLY GENERATED AND SHOULD NOT BE CHECKED IN.
 * 
 *
* 
 */
namespace Manticore
{
  public class SDKTestDefault : JsBackedObject {


  internal SDKTestDefault(ObjectInstance value) : base(value) {
  }


  public SDKTestDefault() {
    this.impl = Engine.CreateJsObject("SDKTestDefault", ManticoreEngine.EmptyArgs);
  }


  internal static SDKTestDefault NativeInstanceForObject(ObjectInstance value) {
    if (value == null) {
      return null;
    }

    var nativeClass = value.Get("_native");
    if (!nativeClass.IsString()) {
      return new SDKTestDefault(value);
    }
    var strNativeClass = nativeClass.AsString();

    if ("SDKTestDefaultSubclass".Equals(strNativeClass)) {
      return new SDKTestDefaultSubclass(value);
    }

    return new SDKTestDefault(value);
  }
  /**
   * Test closure
   */
  public  bool IsItTrue() {
    JsValue[] args = new JsValue[] {
      
    };
    
    var func = this.impl.Get("isItTrue").As<FunctionInstance>();
      return Engine.JsWithReturn(() => {
      var returnValue = func.Call(this.impl, args);
      return Engine.Converter.AsNativeBool(returnValue);
    });
  }



  public int Test {
    get {
      return Engine.JsWithReturn(() => {
        var test = this.impl.Get("test");
        return Engine.Converter.AsNativeInt(test);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsInt(value);
      Engine.Js(() => {
        this.impl.Put("test", jsValue, true);
      });
    }
    
  }

  public bool ItsTrue {
    get {
      return Engine.JsWithReturn(() => {
        var itsTrue = this.impl.Get("itsTrue");
        return Engine.Converter.AsNativeBool(itsTrue);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsBool(value);
      Engine.Js(() => {
        this.impl.Put("itsTrue", jsValue, true);
      });
    }
    
  }

  public bool ItsFalse {
    get {
      return Engine.JsWithReturn(() => {
        var itsFalse = this.impl.Get("itsFalse");
        return Engine.Converter.AsNativeBool(itsFalse);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsBool(value);
      Engine.Js(() => {
        this.impl.Put("itsFalse", jsValue, true);
      });
    }
    
  }

  public int BlankInt {
    get {
      return Engine.JsWithReturn(() => {
        var blankInt = this.impl.Get("blankInt");
        return Engine.Converter.AsNativeInt(blankInt);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsInt(value);
      Engine.Js(() => {
        this.impl.Put("blankInt", jsValue, true);
      });
    }
    
  }

  public int IntOne {
    get {
      return Engine.JsWithReturn(() => {
        var intOne = this.impl.Get("intOne");
        return Engine.Converter.AsNativeInt(intOne);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsInt(value);
      Engine.Js(() => {
        this.impl.Put("intOne", jsValue, true);
      });
    }
    
  }

  public Decimal? BlankDecimal {
    get {
      return Engine.JsWithReturn(() => {
        var blankDecimal = this.impl.Get("blankDecimal");
        return Engine.Converter.AsNativeDecimal(blankDecimal);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsDecimal(value);
      Engine.Js(() => {
        this.impl.Put("blankDecimal", jsValue, true);
      });
    }
    
  }

  public Decimal? DecimalHundredOhOne {
    get {
      return Engine.JsWithReturn(() => {
        var decimalHundredOhOne = this.impl.Get("decimalHundredOhOne");
        return Engine.Converter.AsNativeDecimal(decimalHundredOhOne);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsDecimal(value);
      Engine.Js(() => {
        this.impl.Put("decimalHundredOhOne", jsValue, true);
      });
    }
    
  }

  public String NullString {
    get {
      return Engine.JsWithReturn(() => {
        var nullString = this.impl.Get("nullString");
        return Engine.Converter.AsNativeString(nullString);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsString(value);
      Engine.Js(() => {
        this.impl.Put("nullString", jsValue, true);
      });
    }
    
  }

  public DateTime Now {
    get {
      return Engine.JsWithReturn(() => {
        var now = this.impl.Get("now");
        return Engine.Converter.AsNativeDate(now);
      });
    }
    
    set {
      var jsValue = Engine.Converter.AsJsDate(value);
      Engine.Js(() => {
        this.impl.Put("now", jsValue, true);
      });
    }
    
  }

  public List<String> StringArray {
    get {
      return Engine.JsWithReturn(() => {
        var stringArray = this.impl.Get("stringArray");
        return Engine.Converter.ToNativeArray(stringArray, new Func<JsValue,String>((element) => Engine.Converter.AsNativeString(element)));
      });
    }
    
    set {
      var jsValue = Engine.Converter.ToJsArray(value, (element) => Engine.Converter.AsJsString(element));
      Engine.Js(() => {
        this.impl.Put("stringArray", jsValue, true);
      });
    }
    
  }



  }
}

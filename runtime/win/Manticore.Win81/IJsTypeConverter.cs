using Jint.Native;
using Jint.Native.Array;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    public interface IJsTypeConverter
    {
        JsValue AsJs(Object nativeValue);

        JsValue AsJsBool(bool b);
        JsValue AsJsDate(DateTime date);
        JsValue AsJsDecimal(decimal? d);
        JsValue AsJsInt(int i);
        JsValue AsJsString(string s);
        JsValue AsJsObject(IDictionary<String, Object> d);

        bool AsNativeBool(JsValue value);
        DateTime AsNativeDate(JsValue value);
        decimal? AsNativeDecimal(JsValue value);
        int AsNativeInt(JsValue value);
        string AsNativeString(JsValue s);
        Exception AsException(JsValue v);
        IDictionary<String, Object> AsNativeObject(JsValue o);

        ArrayInstance ToJsArray<T>(System.Collections.Generic.List<T> v, Func<T, JsValue> converter);
        System.Collections.Generic.List<T> ToNativeArray<T>(JsValue value, Func<JsValue, T> converter);
    }
}

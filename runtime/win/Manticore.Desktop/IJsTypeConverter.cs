using System;
using System.Collections.Generic;

namespace Manticore
{
    public interface IJsTypeConverter
    {
        dynamic AsJs(Object nativeValue);

        dynamic AsJsBool(bool b);
        dynamic AsJsDate(DateTime date);
        dynamic AsJsDecimal(decimal? d);
        dynamic AsJsInt(int i);
        dynamic AsJsString(string s);
        dynamic AsJsObject(IDictionary<String, Object> d);

        bool AsNativeBool(dynamic value);
        DateTime AsNativeDateTime(dynamic value);
        decimal? AsNativeDecimal(dynamic value);
        int AsNativeInt(dynamic value);
        string AsNativeString(dynamic s);
        Exception AsException(dynamic v);
        IDictionary<String, Object> AsNativeObject(dynamic o);

        dynamic ToJsArray<T>(System.Collections.Generic.List<T> v, Func<T, dynamic> converter);
        System.Collections.Generic.List<T> ToNativeArray<T>(object value, Func<object, T> converter);
    }
}

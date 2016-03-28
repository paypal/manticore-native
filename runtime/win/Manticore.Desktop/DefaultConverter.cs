using Microsoft.ClearScript;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    /**
     * A generic class in charge of converting to and from Javascript types.
     **/
    public class DefaultConverter<JsBaseClass> : IJsTypeConverter where JsBaseClass : class, new()
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        ManticoreEngine engine;
        Func<JsBaseClass,dynamic> jsExtractor;
        Func<dynamic,Exception> jsException;

        /// <summary>
        /// One of the important goals of Manticore is to isolate the fact that you have chosen to use it
        /// for services you advertise to others (e.g. the SDK scenario). SO, you can't have your objects
        /// inherit from our base class. This converter constructor is your punishment. You need to provide
        /// us two simple lambdas - one that retrives the JS value from your objects and one that makes
        /// your exception class. This means that in most simple cases you won't have to derive from
        /// DefaultConverter, just create one.
        /// </summary>
        public DefaultConverter(ManticoreEngine engine, Func<JsBaseClass, dynamic> jsExtractor, Func<dynamic,Exception> jsException)
        {
            this.engine = engine;
            this.jsExtractor = jsExtractor;
            this.jsException = jsException;
        }

        public dynamic AsJs(Object value)
        {
            if (value == null)
            {
                return null;
            }
            var jsv = value as JsBaseClass;
            if (jsv != null)
            {
                return jsExtractor((JsBaseClass)value);
            }
            return value;
        }

        public Exception AsException(dynamic value)
        {
            return jsException(value);
        }

        public DateTime AsNativeDateTime(dynamic value)
        {
            return Epoch.AddMilliseconds(value.getTime());
        }

        public dynamic AsJsDate(DateTime date)
        {
            return engine.v8.Evaluate(String.Format("new Date({0})", Math.Round(date.Subtract(Epoch).TotalMilliseconds)));
        }

        public Decimal? AsNativeDecimal(dynamic value)
        {
            if (value == null || value is Undefined)
            {
                return null;
            }

            try
            {
                return Decimal.Parse(value.toString());
            }
            catch (RuntimeBinderException)
            {
                return Decimal.Parse(value.ToString());
            }
        }

        public dynamic AsJsDecimal(Decimal? d)
        {
            return d != null ? d.ToString() : null;
        }

        public int AsNativeInt(dynamic value)
        {
            if (value is Undefined)
            {
                return 0;
            }
            if (value is int)
            {
                return (int)value;
            }
            return int.Parse(value.toString());
        }

        public dynamic AsJsInt(int i)
        {
            return i;
        }

        // We need a custom method for string because null != JSValue.null
        public dynamic AsJsString(string s)
        {
            return s;
        }

        public String AsNativeString(dynamic s)
        {
            if (s is Undefined)
            {
                return null;
            }
            return (String)s;
        }

        public dynamic AsJsBool(bool b) { return b; }
        public bool AsNativeBool(dynamic value)
        {
            return value != null && (!(value is Undefined)) && (bool)value;
        }

        public dynamic AsJsObject(IDictionary<string, object> d)
        {
            if (d == null)
            {
                return null;
            }
            dynamic exp = engine.ManticoreJsObject._.construct();
            foreach (var kv in d)
            {
                if (kv.Value is Decimal)
                {
                    exp[kv.Key] = AsJsDecimal((Decimal)kv.Value);
                }
                else if (kv.Value is Dictionary<String, Object>)
                {
                    exp[kv.Key] = AsJsObject((Dictionary<String, Object>)kv.Value);
                }
                else if (kv.Value is DateTime)
                {
                    exp[kv.Key] = AsJsDate((DateTime)kv.Value);
                }
                else
                {
                    exp[kv.Key] = AsJs(kv.Value);
                }
            }
            return exp;
        }

        public IDictionary<String, Object> AsNativeObject(dynamic value)
        {
            if (value == null || value is Undefined)
            {
                return null;
            }
            var ret = new Dictionary<String, Object>();
            foreach (var p in value.GetDynamicMemberNames())
            {
                var v = (Object) value[p];
                if (v == null || v is Undefined)
                {
                    ret[p] = null;
                }
                else if (v is String)
                {
                    ret[p] = v.ToString();
                }
                else if (v is bool)
                {
                    ret[p] = (bool)v;
                }
                else if (v is double)
                {
                    ret[p] = (double)v;
                }
                else if (v is float)
                {
                    ret[p] = (float) v;
                }
                else if (v is int)
                {
                    ret[p] = (int) v;
                }
                else
                {
                    ret[p] = (string) ((dynamic)v).toString();
                }
            }
            return ret;
        }

        public List<T> ToNativeArray<T>(object value, Func<object, T> converter)
        {
            if (value == null || value is Undefined)
            {
                return null;
            }
            dynamic v = (dynamic)value;
            List<T> list = new List<T>();
            var len = v.length;
            for (var i = 0; i < len; i++)
            {
                list.Add(converter(v[i]));
            }
            return list;
        }

        public dynamic ToJsArray<T>(List<T> v, Func<T, dynamic> converter)
        {
            dynamic jsArray = engine.ManticoreJsObject._.array();
            for (var i = 0; i < v.Count; i++)
            {
                jsArray.push(converter(v[i]));
            }
            return jsArray;
        }

        public static void ParseResponseBody(ManticoreEngine engine, dynamic responseInfo, String format, byte[] response)
        {
            if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                String json = Encoding.UTF8.GetString(response);
                if (json.Length > 0)
                {
                    responseInfo.body = engine.v8.Script.JSON.parse(json);
                }
            }
            else if (!"binary".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                String bodyString = Encoding.UTF8.GetString(response);
                responseInfo.body = bodyString;
            }
            else
            {
                if (response.Length > 0)
                {
                    responseInfo.body = Convert.ToBase64String(response);
                }
            }
        }
    }
}

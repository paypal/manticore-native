using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Date;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Runtime;

namespace Manticore
{
    /**
     * A generic class in charge of converting to and from Javascript types.
     **/
    public class DefaultConverter<JsBaseClass> : IJsTypeConverter where JsBaseClass : class, new()
    {
        ManticoreEngine engine;
        Func<JsBaseClass, JsValue> jsExtractor;
        Func<ObjectInstance, Exception> jsException;

        /// <summary>
        /// One of the important goals of Manticore is to isolate the fact that you have chosen to use it
        /// for services you advertise to others (e.g. the SDK scenario). SO, you can't have your objects
        /// inherit from our base class. This converter constructor is your punishment. You need to provide
        /// us two simple lambdas - one that retrives the JS value from your objects and one that makes
        /// your exception class. This means that in most simple cases you won't have to derive from
        /// DefaultConverter, just create one.
        /// </summary>
        public DefaultConverter(ManticoreEngine engine, Func<JsBaseClass, JsValue> jsExtractor, Func<ObjectInstance, Exception> jsException)
        {
            this.engine = engine;
            this.jsExtractor = jsExtractor;
            this.jsException = jsException;
        }

        public JsValue AsJs(Object value)
        {
            if (value == null)
            {
                return JsValue.Null;
            }
            var jsv = value as JsBaseClass;
            if (jsv != null)
            {
                return jsExtractor((JsBaseClass)value);
            }
            return JsValue.FromObject(engine.jsEngine, value);
        }

        public DateTime AsNativeDate(JsValue value)
        {
            var valueAsDate = value.As<DateInstance>();
            if (valueAsDate != null)
            {
                return valueAsDate.ToDateTime();
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public JsValue AsJsDate(DateTime date)
        {
            return engine.jsEngine.Date.Construct(date);
        }

        public Decimal? AsNativeDecimal(JsValue value)
        {
            if (value.IsNull() || value.IsUndefined())
            {
                return null;
            }
            return Decimal.Parse(value.ToString());
        }

        public JsValue AsJsDecimal(Decimal? d)
        {
            return new JsValue(d.ToString());
        }

        public int AsNativeInt(JsValue value)
        {
            if (value.IsNull() || value.IsUndefined())
            {
                return 0;
            }
            return (int)Math.Round(value.AsNumber());
        }

        public JsValue AsJsInt(int i)
        {
            return new JsValue(i);
        }

        // We need a custom method for string because null != JSValue.null
        public JsValue AsJsString(string s)
        {
            if (s == null)
            {
                return JsValue.Null;
            }
            return new JsValue(s);
        }

        public JsValue AsJsBool(bool b) { return new JsValue(b); }

        public string AsNativeString(JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
            {
                return null;
            }
            if (jsValue.IsString())
            {
                return jsValue.AsString();
            }
            return jsValue.ToString();
        }

        public bool AsNativeBool(JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
            {
                return false;
            }
            return jsValue.AsBoolean();
        }

        public JsValue AsJsObject(IDictionary<String, Object> d)
        {
            if (d == null)
            {
                return JsValue.Null;
            }
            var jsD = engine.CreateJsObject();

            foreach (var kv in d)
            {
                // I think this isn't sufficient. We probably need
                // to inspect the type a bit more...
                JsValue value;
                if (kv.Value is Decimal)
                {
                    value = AsJsDecimal((Decimal)kv.Value);
                }
                else if (kv.Value is Dictionary<String, Object>)
                {
                    value = AsJsObject((Dictionary<String, Object>)kv.Value);
                }
                else if (kv.Value is DateTime)
                {
                    value = AsJsDate((DateTime)kv.Value);
                }
                else
                {
                    value = AsJs(kv.Value);
                }
                jsD.FastAddProperty(kv.Key, value, true, true, false);
            }

            return jsD;
        }

        public IDictionary<String, Object> AsNativeObject(JsValue jsValue)
        {
            if (jsValue.IsNull() || jsValue.IsUndefined())
            {
                return null;
            }
            var ret = new Dictionary<String, Object>();
            ObjectInstance value = jsValue.AsObject();
            foreach (var prop in value.GetOwnProperties())
            {
                var v = value.GetOwnProperty(prop.Key);
                if (v != null)
                {
                    var jsv = v.Value;
                    if (!jsv.HasValue || jsv.Value.IsNull())
                    {
                        ret[prop.Key] = null;
                    }
                    else if (jsv.Value.IsBoolean())
                    {
                        ret[prop.Key] = jsv.Value.AsBoolean();
                    }
                    else if (jsv.Value.IsNumber())
                    {
                        double d = jsv.Value.AsNumber();
                        if (d % 1 == 0)
                        {
                            ret[prop.Key] = (int)d;
                        }
                        else
                        {
                            ret[prop.Key] = d;
                        }
                    }
                    else if (jsv.Value.IsObject())
                    {
                        ret[prop.Key] = jsv.Value.AsObject().ToString();
                    }
                    else if (jsv.Value.IsString())
                    {
                        ret[prop.Key] = jsv.Value.AsString();
                    }
                }
            }
            return ret;
        }

        public List<T> ToNativeArray<T>(JsValue v, Func<JsValue, T> converter)
        {
            if (v.IsNull() || v.IsUndefined())
            {
                return null;
            }

            List<T> list = new List<T>();
            var array = v.AsArray();
            var len = (int)array.Get("length").AsNumber();
            for (var i = 0; i < len; i++)
            {
                list.Add(converter(array.Get(i.ToString())));
            }
            return list;
        }

        public ArrayInstance ToJsArray<T>(List<T> v, Func<T, JsValue> converter)
        {
            var jsArray = engine.jsEngine.Array.Construct(Arguments.Empty);
            engine.jsEngine.Array.PrototypeObject.Push(jsArray, v.Select(converter).ToArray());
            return (ArrayInstance)jsArray;
        }

        public Exception AsException(JsValue value)
        {
            return jsException(value.AsObject());
        }

        public static void ParseResponseBody(ManticoreEngine engine, ObjectInstance response, String format, Stream byteStream)
        {
            if ("json".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                String json = new StreamReader(byteStream, Encoding.UTF8).ReadToEnd();
                if (json != null && json.Length > 0)
                {
                    JsValue jsObject = engine.jsEngine.Json.Parse(JsValue.Null, new JsValue[] { json });
                    response.FastAddProperty("body", jsObject, false, true, false);
                }
            }
            else if (!"binary".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                String bodyString = new StreamReader(byteStream, Encoding.UTF8).ReadToEnd();
                response.FastAddProperty("body", new JsValue(bodyString), false, true, false);
            }
            else
            {
                byte[] bytes = ReadAllBytes(byteStream);
                if (bytes != null && bytes.Length > 0)
                {
                    response.FastAddProperty("body", new JsValue(Convert.ToBase64String(bytes)), false, true, false);
                }
            }
        }

        static byte[] ReadAllBytes(Stream stream)
        {
            const int bufferSize = 4096;
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[bufferSize];
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    ms.Write(buffer, 0, count);
                }
                return ms.ToArray();
            }
        }
    }
}

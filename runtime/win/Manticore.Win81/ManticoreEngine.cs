using Jint;
using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Date;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    public class ManticoreEngine
    {
        public static JsValue[] EmptyArgs = new JsValue[] { };

        public IJsTypeConverter Converter { get; set; }
        public Engine jsEngine { get; private set; }

        NativeServices nativeServices;
        internal Dictionary<string, JsValue> exportedItems;

        public ObjectInstance ManticoreJsObject { get; private set; }

        public ManticoreEngine()
        {
            nativeServices = new NativeServices();
            Start();
        }

        public void Start()
        {
            exportedItems = new Dictionary<string, JsValue>();
            jsEngine = new Engine();
            ManticoreJsObject = jsEngine.Object.Construct(EmptyArgs);
            jsEngine.SetValue("manticore", ManticoreJsObject);
            nativeServices.Register(this);
        }

        public bool IsStarted
        {
            get
            {
                return jsEngine != null;
            }
        }

        public void Shutdown()
        {
            if (exportedItems != null)
            {
                exportedItems.Clear();
                exportedItems = null;
            }
            jsEngine = null;
            ManticoreJsObject = null;
        }

        public void LoadScript(String script)
        {
            jsEngine.Execute(script);
        }

        private Exception ConvertException(JavaScriptException jse)
        {
            // TODO return better stuff.
            var jsv = new ObjectInstance(this.jsEngine);
            jsv.FastAddProperty("message", new JsValue(jse.Message), false, true, false);
            return new ManticoreException(jsv);
        }

        public T JsWithReturn<T>(Func<T> func)
        {
            lock (jsEngine)
            {
                try
                {
                    return func();
                }
                catch (JavaScriptException jse)
                {
                    throw ConvertException(jse);
                }
            }
        }

        public void Js(Action action)
        {
            lock (jsEngine)
            {
                try
                {
                    action();
                }
                catch (JavaScriptException jse)
                {
                    throw ConvertException(jse);
                }
            }
        }

        public ObjectInstance CreateJsObject(String name, JsValue[] args)
        {
            return JsWithReturn(() =>
            {
                ScriptFunctionInstance constructor = exportedItems[name].As<ScriptFunctionInstance>();
                return constructor.Construct(args);
            });
        }

        public ObjectInstance CreateJsObject()
        {
            return JsWithReturn(() => jsEngine.Object.Construct(EmptyArgs));
        }

        public ObjectInstance GetJsClass(String name)
        {
            return JsWithReturn(() =>
            {
                return exportedItems[name].As<ObjectInstance>();
            });
        }

        public ClrFunctionInstance AsJsFunction(Func<JsValue, JsValue[], JsValue> clrFunction)
        {
            return new ClrFunctionInstance(jsEngine, clrFunction);
        }


        public DelegateWrapper Wrap(Delegate d)
        {
            return new DelegateWrapper(jsEngine, d);
        }

        public bool IsNullOrUndefined(JsValue v)
        {
            return (v.IsNull() || v.IsUndefined());
        }

    }
}

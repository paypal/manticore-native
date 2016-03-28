using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Manticore
{
    public class ManticoreEngine
    {
        public static JsValue[] EmptyArgs = { };
        public IJsTypeConverter Converter { get; set; }
        public Engine jsEngine { get; private set; }
        private readonly object _locker = new object();
        private readonly NativeServices _nativeServices;
        internal Dictionary<string, JsValue> exportedItems;
        public ObjectInstance ManticoreJsObject { get; private set; }
        private const string LogComponentName = "net4.manticore.engine";

        public ManticoreEngine()
        {
            _nativeServices = new NativeServices();
            Start();
        }

        private void RunOnJsThread(Action action)
        {
            lock (_locker)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _nativeServices.JsLog(this, "ERROR", LogComponentName, string.Format("RunOnJsThread erred with:\n{0}", ex));
                    throw;
                }
            }
        }

        public void Start()
        {
            jsEngine = new Engine();
            RunOnJsThread(() =>
            {
                exportedItems = new Dictionary<string, JsValue>();
                ManticoreJsObject = jsEngine.Object.Construct(EmptyArgs);
                jsEngine.SetValue("manticore", ManticoreJsObject);
                jsEngine.SetValue("global", jsEngine.Global);
                _nativeServices.Register(this);
            });
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
            RunOnJsThread(() =>
            {
                jsEngine.Execute(script);
            });
        }

        private Exception ConvertException(JavaScriptException jse)
        {
            // TODO return better stuff.
            var jsv = new ObjectInstance(jsEngine);
            jsv.FastAddProperty("message", new JsValue(jse.Message), false, true, false);
            return new ManticoreException(jsv);
        }

        public T JsWithReturn<T>(Func<T> func)
        {
            var result = default(T);
            Exception ex = null;
            RunOnJsThread(() =>
            {
                try
                {
                    result = func();
                }
                catch (JavaScriptException jse)
                {
                    ex = ConvertException(jse);
                }
            });

            if (ex != null)
            {
                throw ex;
            }

            return result;
        }

        public void Js(Action action)
        {
            Exception ex = null;
            RunOnJsThread(() =>
            {
                try
                {
                    action();
                }
                catch (JavaScriptException jse)
                {
                    ex = ConvertException(jse);
                }
            });

            if (ex != null)
            {
                throw ex;
            }
        }

        public ObjectInstance CreateJsObject(String name, JsValue[] args)
        {
            return JsWithReturn(() =>
            {
                var constructor = exportedItems[name].As<ScriptFunctionInstance>();
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
            DelegateWrapper wrapper = null;
            RunOnJsThread(() =>
            {
                wrapper = new DelegateWrapper(jsEngine, d);
            });

            return wrapper;
        }

        public bool IsNullOrUndefined(JsValue v)
        {
            return (v.IsNull() || v.IsUndefined());
        }

    }
}

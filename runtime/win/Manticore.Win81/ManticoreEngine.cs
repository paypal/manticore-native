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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    public class ManticoreEngine
    {
        public static JsValue[] EmptyArgs = { };
        public IJsTypeConverter Converter { get; set; }
        public Engine jsEngine { get; private set; }

        private bool _loadedPolyfill;
        private readonly NativeServices _nativeServices;
        private ObjectInstance _exports;

        public ObjectInstance ManticoreJsObject { get; private set; }

        public ManticoreEngine()
        {
            _nativeServices = new NativeServices();
            Start();
        }

        public void Start()
        {
            jsEngine = new Engine();
            RunOnJsThread(() =>
            {
                ManticoreJsObject = jsEngine.Object.Construct(EmptyArgs);
                jsEngine.Global.FastAddProperty("manticore", ManticoreJsObject, false, true, false);
                jsEngine.Global.FastAddProperty("global", jsEngine.Global, false, false, false);
                _exports = jsEngine.Object.Construct(EmptyArgs);
                jsEngine.Global.FastAddProperty("exports", _exports, false, false, false);
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
            if (jsEngine != null)
            {
                jsEngine.Global.RemoveOwnProperty("global");
                jsEngine.Global.RemoveOwnProperty("manticore");
                jsEngine = null;
            }
            ManticoreJsObject = _exports = null;
            _loadedPolyfill = false;
        }

        public void LoadScript(String script)
        {
            String poly = null;
            if (!_loadedPolyfill)
            {
                _loadedPolyfill = true;
                poly = GetPolyfill();
            }
            RunOnJsThread(() =>
            {
                if (poly != null)
                {
                    jsEngine.Execute(poly);
                }
                jsEngine.Execute(script);
            });
        }

        protected Exception ConvertException(JavaScriptException jse)
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
                var constructor = _exports.Get(name).As<ScriptFunctionInstance>();
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
                return _exports.Get(name).As<ObjectInstance>();
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

#if DOTNET_4
        private readonly object _locker = new object();

        protected void RunOnJsThread(Action action)
        {
            lock (_locker)
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        protected string GetPolyfill()
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Manticore.polyfill.jint.pack.js"))
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    return reader.ReadToEnd();
                }
            }
        }
#else
        protected void RunOnJsThread(Action action)
        {
            lock (jsEngine)
            {
                try
                {
                    action();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        protected string GetPolyfill()
        {
            return new StreamReader(typeof(ManticoreEngine).GetTypeInfo().Assembly.GetManifestResourceStream("Manticore.polyfill.jint.pack.js")).ReadToEnd();
        }
#endif
    }
}
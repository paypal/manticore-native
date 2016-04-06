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
        public event EventHandler WillLoadPolyfill;
        public event EventHandler DidLoadPolyfill;
        public event ScriptEventHandler WillLoadScript;
        public event ScriptEventHandler DidLoadScript;

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

        public ManticoreEngine Start()
        {
            jsEngine = new Engine();
            jsEngine.ShouldCreateStackTrace = true;
            RunOnJsThread(() =>
            {
                ManticoreJsObject = jsEngine.Object.Construct(EmptyArgs);
                jsEngine.Global.FastAddProperty("manticore", ManticoreJsObject, false, true, false);
                jsEngine.Global.FastAddProperty("global", jsEngine.Global, false, false, false);
                _exports = jsEngine.Object.Construct(EmptyArgs);
                jsEngine.Global.FastAddProperty("exports", _exports, false, false, false);
                _nativeServices.Register(this);
            });
            return this;
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

        public void LoadScript(String script, String name)
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
                    if (this.WillLoadPolyfill != null)
                    {
                        this.WillLoadPolyfill(this, EventArgs.Empty);
                    }
                    jsEngine.Execute(poly, new Jint.Parser.ParserOptions
                    {
                        Source = "polyfill.pack.js"
                    });
                    if (this.DidLoadPolyfill != null)
                    {
                        this.DidLoadPolyfill(this, EventArgs.Empty);
                    }
                }
                ScriptEventArgs args = new ScriptEventArgs(name, script);
                if (this.WillLoadScript != null)
                {
                    this.WillLoadScript(this, args);
                }
                jsEngine.Execute(script, new Jint.Parser.ParserOptions
                {
                    Source = name
                });
                if (this.DidLoadScript != null)
                {
                    this.DidLoadScript(this, args);
                }
            });
        }

        protected Exception ConvertException(JavaScriptException jse)
        {
            // TODO return better stuff.
            var jsv = CreateJsObject();
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
            return JsWithReturn(() => _exports.Get(name).As<ObjectInstance>());
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

        public void ResolvePromise(JsValue promise, ClrFunctionInstance callback)
        {
            ManticoreJsObject.Get("asCallback").As<FunctionInstance>().Call(ManticoreJsObject, new JsValue[] { promise, callback });
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
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    public class ManticoreEngine
    {
        private bool loadedPolyfill;

        public IJsTypeConverter Converter { get; set; }
        public V8ScriptEngine v8 { get; private set; }
        NativeServices nativeServices;

        public dynamic ManticoreJsObject { get; private set; }

        public ManticoreEngine()
        {
            nativeServices = new NativeServices();
            Start();
        }

        public void Start()
        {
            v8 = new V8ScriptEngine();
            v8.AccessContext = typeof(ManticoreEngine);
            v8.Execute("var manticore = {platform:{name:\"win\"}};");
            ManticoreJsObject = v8.Script.manticore;            
            v8.Script.global = v8.Script;
            nativeServices.Register(this);
        }

        public bool IsStarted
        {
            get
            {
                return v8 != null;
            }
        }

        public void LoadScript(String script, String name)
        {
            if (!loadedPolyfill)
            {
                loadedPolyfill = true;
                String polyfill;
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Manticore.polyfill.clearscript.pack.js"))
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        polyfill = reader.ReadToEnd();
                    }
                }
                v8.Execute("polyfill.js", polyfill);
            }
            v8.Execute(name, script);
        }

        public void Shutdown()
        {
            if (v8 != null)
            {
                v8.Script.global = null;
            }
            loadedPolyfill = false;
            ManticoreJsObject = null;
            v8 = null;
        }

        public bool IsNullOrUndefined(dynamic v)
        {
            return v == null || v is Undefined;
        }

        private Exception ConvertException(ScriptEngineException se)
        {
            dynamic exp = new ExpandoObject();
            exp.message = se.Message;
            var dets = se.ErrorDetails;
            if (dets != null && dets.IndexOf('\n') > 0)
            {
                dets = dets.Substring(dets.IndexOf('\n'));
            }
            exp.stack = dets;
            return Converter.AsException(exp);
        }

        public T JsWithReturn<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Microsoft.ClearScript.ScriptEngineException se)
            {
                Exception rethrown = ConvertException(se);
                throw rethrown;
            }
        }

        public void Js(Action action)
        {
            try
            {
                action();
            }
            catch (Microsoft.ClearScript.ScriptEngineException se)
            {
                Exception rethrown = ConvertException(se);
                throw rethrown;
            }
        }

        public dynamic CreateJsObject()
        {
            return ManticoreJsObject._.construct();
        }

        public dynamic CreateJsObject(String className, dynamic args)
        {
            return ManticoreJsObject._.construct(className, args);
        }

        public dynamic GetJsClass(String className)
        {
            return ManticoreJsObject._.getClass(className);
        }

        public dynamic Array(params dynamic[] values)
        {
            var array = ManticoreJsObject._.array();
            if (values != null && values.Length > 0)
            {
                foreach (var v in values)
                {
                    array.push(v);
                }
            }
            return array;
        }

        public void ResolvePromise(dynamic promise, dynamic callback)
        {
            ManticoreJsObject.asCallback(promise, callback);
        }
    }
}

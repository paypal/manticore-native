using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    public class ManticoreEngine
    {
        public IJsTypeConverter Converter { get; set; }
        public V8ScriptEngine v8 { get; private set; }
        SingleThreadedReentrantScheduler executor;
        NativeServices nativeServices;
        internal Dictionary<string,object> exportedItems;

        public dynamic ManticoreJsObject { get; private set; }

        public ManticoreEngine()
        {
            nativeServices = new NativeServices();
            Start();
        }

        public void Start()
        {
            exportedItems = new Dictionary<string, object>();
            v8 = new V8ScriptEngine();
            v8.AccessContext = typeof(ManticoreEngine);
            v8.Execute("manticore = { _: { " +
                " array: function () { return []; }, " +
                " fn: function (fnlike,c) { return function () {"+
                "   var a = arguments; switch (c) { "+
                "     case 0: return fnlike(); case 1: return fnlike(a[0]); "+
                "     case 2: return fnlike(a[0],a[1]); case 3: return fnlike(a[0],a[1],a[2]); "+
                "     default: throw new Error('Do not make callbacks with so many arguments.');"+
                "   }"+ // switch
                "  };"+ // return
                " }," +
                " construct: function construct(C, a) {" +
                "   if (!C) return {};" +
                "   function F() { return C.apply(this, a); }" +
                "   F.prototype = C.prototype;" +
                "   return new F();" +
                "} } };");
            ManticoreJsObject = v8.Script.manticore;
            v8.Script.global = v8.Script;
            nativeServices.Register(this);
            executor = new SingleThreadedReentrantScheduler();
        }

        public bool IsStarted
        {
            get
            {
                return v8 != null;
            }
        }

        public void LoadScript(String script)
        {
            v8.Execute(script);
        }

        public void Shutdown()
        {
            if (executor != null)
            {
                executor.Stop();
                executor = null;
            }
            v8 = null;
            exportedItems = null;
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
            return ManticoreJsObject._.construct(exportedItems[className], args);
        }

        public dynamic GetJsClass(string className)
        {
            return exportedItems[className];
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
    }
}

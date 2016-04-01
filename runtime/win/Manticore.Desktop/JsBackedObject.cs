using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    /**
     * The JsBackedObject provides a common base class for your generated classes. If you are building an SDK and don't necessarily
     * want to expose the "Javascriptiness" of your system, you can define your own base class, make the methods only visible to your
     * package and just reimplement these methods with relevant custom types. Code generation and the compiler should collude to make the
     * rest of it just work...
     */
    public class JsBackedObject
    {
        protected dynamic impl;

        public JsBackedObject() { }

        protected JsBackedObject(object value)
        {
            if (value is JsValueHolder)
            {
                this.impl = ((JsValueHolder)value).jsValue;
            }
            else
            {
                this.impl = value;
            }
        }

        /// <summary>
        /// All the classes generated against a particular base class must share a single Manticore engine.
        /// This is because we expose raw constructors which need to instantiate Js objects without having
        /// to pass the engine around all over the place.
        /// </summary>
        public static ManticoreEngine Engine { get; private set; }

        public static void CreateManticoreEngine(String script, String name) {
            if (Engine != null && Engine.IsStarted)
            {
                throw new InvalidOperationException("You must shut down the existing engine before creating a new one.");
            }
            var e = new ManticoreEngine();
            e.Converter = new DefaultConverter<JsBackedObject>(e, (native) => native.impl, (jsErr) => new ManticoreException(jsErr));
            // Typically, you would add your own native method implementations to
            // the ManticoreJsObject here, before you load your script
            e.LoadScript(script, name);
            Engine = e;
        }

    }
}

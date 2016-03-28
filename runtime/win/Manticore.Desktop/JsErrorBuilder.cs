using System;
#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
using Jint.Native;
using Jint.Native.Error;
#else
using System.Dynamic;
#endif

namespace Manticore
{
    /// <summary>
    /// The purpose of JsErrorBuilder class is to build a JavaScript error object from .Net exception. The constructed object could be 
    /// returned to Javascript functions or passed as an argument to callbacks
    /// </summary>
    public class JsErrorBuilder
    {
        private readonly Exception _exception;
        private int? _code;

#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
        private readonly ManticoreEngine _engine;
        public JsErrorBuilder(ManticoreEngine manticoreEngine, Exception ex)
        {
            _engine = manticoreEngine;
            _exception = ex;
        }
#else
        public JsErrorBuilder(Exception ex)
        {
            _exception = ex;
        }
#endif

        public JsErrorBuilder SetErrorCode(int errorCode)
        {
            _code = errorCode;
            return this;
        }

#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
        public ManticoreJsError Build()
        {
            return new ManticoreJsError(_engine, _exception, _code);
        }
#else
        public ExpandoObject Build()
        {
            dynamic ex = new ExpandoObject();
            ex.message = _exception != null ? _exception.Message : string.Empty;
            ex.stack = _exception != null ? _exception.StackTrace : "native";
            if (_code.HasValue)
            {
                ex.code = _code.Value;
            }
            return ex; // TODO - Provide toString implementation
        }
#endif
    }

#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
    public class ManticoreJsError : ErrorInstance
    {
        internal ManticoreJsError(ManticoreEngine manticoreEngine, Exception ex, int? code = null)
            : base(manticoreEngine.jsEngine, ex.Message)
        {
            FastAddProperty("message", ex.Message, true, false, true);
            FastAddProperty("code", code.HasValue ? new JsValue(code.Value) : JsValue.Null, true, false, true);
            FastAddProperty("stack", ex.StackTrace, true, false, true);
            FastAddProperty("toString", manticoreEngine.AsJsFunction(((thisObj, args) => ex.ToString())), true, false, false);
        }
    }
#endif

}
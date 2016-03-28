using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
using Jint.Native.Object;
#endif

namespace Manticore
{
    public class ManticoreException : Exception
    {
        public string JavascriptStack { get; private set; }

        public ManticoreException(String errorMessage) : base(errorMessage)
        {

        }

#if WINDOWS_PHONE_APP || WINDOWS_APP || DOTNET_4
        public static ManticoreException NativeInstanceForObject(ObjectInstance error)
        {
            return new ManticoreException(error);
        }

        public ManticoreException(ObjectInstance error) : base(error.Get("message").AsString())
        {
            var stack = error.Get("stack");
            if (stack.IsString()) {
                JavascriptStack = stack.AsString();
            }
        }
#else
        public static ManticoreException NativeInstanceForObject(dynamic jsError) {
            return new ManticoreException(jsError);
        }

        public ManticoreException(dynamic jsError)
            : base(((object)jsError.message).ToString())
        {
        }
#endif
    }

    public enum ErrorCodes
    {
        NetworkOffline = -1001
    }
}

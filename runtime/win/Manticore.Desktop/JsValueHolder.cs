using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    /// <summary>
    /// Because of the way dynamic works in C#, you can't pass a dynamic object to a superclass constructor.
    /// This JsValueHolder basically exists to circumvent that and allow inheritance to work.
    /// </summary>
    public class JsValueHolder
    {
        public JsValueHolder(dynamic value)
        {
            this.jsValue = value;
        }

        public dynamic jsValue { get; private set; }
    }
}

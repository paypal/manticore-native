using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manticore
{
    /// <summary>
    /// Event arguments for script will/did load events on ManticoreEngine.
    /// </summary>
    public class ScriptEventArgs : EventArgs
    {
        public ScriptEventArgs(String name, String script)
        {
            this.Name = name;
            this.Script = script;
        }

        public String Name { get; private set; }
        public String Script { get; private set; }
    }

    public delegate void ScriptEventHandler(object sender, ScriptEventArgs e);
}

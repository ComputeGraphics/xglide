using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.system
{
    public static class ConProc
    {
        static List<DateTime> Stamp = [];
        static List<(byte, string)> _Log = [];
        //byte 0: Info, 1: Warning, 2: Error
        static int _MaxEntries = 100;

        /// <summary>
        /// Logs a message with a type (0: Info, 1: Warning, 2: Error). If the log exceeds the maximum number of entries, the oldest entry is removed.
        /// </summary>
        /// <param name="type">(0: Info, 1: Warning, 2: Error)</param>
        public static void Log(string message,byte type = 0)
        {
            if (_Log.Count >= _MaxEntries)
            {
                _Log.RemoveAt(0);
                Stamp.RemoveAt(0);
            }
            _Log.Add((type, message));
            Stamp.Add(DateTime.Now);

            if(GSettings.XConsoleOpen) XConsole.Update();
        }

        public static List<string> GetLog()
        {
            List<string> output = [];
            for (int i = 0; i < _Log.Count; i++)
            {
                string prefix = _Log[i].Item1 switch
                {
                    0 => "[INFO]",
                    1 => "[WARN]",
                    2 => "[ERR]",
                    _ => "[UNK]"
                };
                output.Add($"{Stamp[i]:yyyy-MM-dd HH:mm:ss} {prefix} {_Log[i].Item2}");
            }
            return output;
        }
    }
}

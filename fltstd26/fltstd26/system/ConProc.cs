using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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

        public static void ReportActionStack(string Name,Stack<List<DatabaseAction>> ActionStack,bool Lock)
        {
            System.Diagnostics.Debug.WriteLine($"{Name} Report:\n   {ActionStack.Count} Actions on the stack\n   {Lock}\n   Elements:");
            foreach (var item in ActionStack)
            {
                if (item.Count > 0)
                {
                    foreach (var action in item)
                    {
                        System.Diagnostics.Debug.WriteLine($"      -> {action.ID} Action {action.ActionID} performed - Data Type {action.DataType.Name} - Foreign Key Link {action.ForeignKeyName} - Linked Action {action.LinkAction} - Object ID {action.ObjectID}");

                        System.Diagnostics.Debug.Write($"         Previous Value:\n           ");
                        PropertyInfo[] props = action.DataType.GetProperties();
                        if (action.PreviousValue != null)
                        {
                            foreach (PropertyInfo propInfo in props)
                            {
                                System.Diagnostics.Debug.Write(propInfo.Name + ": " + propInfo.GetValue(action.PreviousValue) + ", ");
                            }
                            System.Diagnostics.Debug.WriteLine("");
                        }

                        System.Diagnostics.Debug.Write($"         Current Value:\n           ");
                        if (action.CurrentValue != null)
                        {
                            foreach (PropertyInfo propInfo in props)
                            {
                                System.Diagnostics.Debug.Write(propInfo.Name + ": " + propInfo.GetValue(action.CurrentValue) + ", ");
                            }
                            System.Diagnostics.Debug.WriteLine("");
                        }
                        else System.Diagnostics.Debug.WriteLine("null");
                    }
                }
                else System.Diagnostics.Debug.WriteLine($"   {item.LastOrDefault()?.ID} Action {item.LastOrDefault()?.ActionID} performed - Data Type {item.LastOrDefault()?.DataType.Name} - Object ID {item.LastOrDefault()?.ObjectID}");
                System.Diagnostics.Debug.WriteLine("");
            }
        }
    }
}

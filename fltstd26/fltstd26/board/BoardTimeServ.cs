using fltstd26.etc;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.board
{
    internal static class BoardTimeServ
    {
        private static Scheduler? clock = null;

        internal static void Init()
        {
            if (clock == null)
                clock = new(TimeSpan.FromMilliseconds(USettings.FlashInterval),TickHandler,true);
            else clock.Start();
        }

        internal static void Pause() => clock?.Pause();

        internal static Dictionary<Guid,Action> TimeListen = [];

        internal static void Clock(Guid id, Action a)
        {
            TimeListen.TryAdd(id,a);
        }

        internal static void Unload(Guid id)
        {
            TimeListen.Remove(id);
        }

        private static void TickHandler(object? sender, EventArgs e)
        {
            foreach (KeyValuePair<Guid,Action> t in TimeListen)
            {
                t.Value.Invoke();
            }
        }
    }
}

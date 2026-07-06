namespace fltstd26.system
{
    internal static class TimeServ
    {
        private static Scheduler? clock = null;
        private static int ActionCounter = 0;
        public static bool ClockActive => clock?.IsRunning ?? false;
        //Run ID, Run at Time, Action
        private static readonly SortedList<TimeSpan,Dictionary<int,Action>> ActionQeue = [];
        internal static void Init()
        {
            clock ??= new(TimeSpan.FromSeconds(5),(s,e) => Tick(),true,RoundToMinute(DateTime.Now) - DateTime.Now);
            ConProc.Log("[TIME] Uhr gestartet");
        }

        internal static int Schedule(TimeSpan executionTime,Action action)
        {
            TimeSpan round = RoundTo5S(executionTime);
            System.Diagnostics.Debug.WriteLine("An Action was placed to run at " + round.ToString("c"));
            if (ActionQeue.IndexOfKey(round) != -1) ActionQeue[round].Add(ActionCounter,action);
            else ActionQeue.Add(round,new() { { ActionCounter,action } });
            return ActionCounter++;
        }

        internal static void Unschedule(int id)
        {
            for (int i = 0; i < ActionQeue.Count; i++)
            {
                if (ActionQeue.GetValueAtIndex(i).Remove(id)) return;
            }
        }

        internal static void Reschedule(int id, TimeSpan time)
        {
            //Wenn ein KeyValue Pair mit der ID gefunden wird, wird das ereignis dort entfernt und an 
            Action? a = Seek(id);
            if (a != null)
            {
                Schedule(time,a);
                Unschedule(id);       
            }
        }

        internal static Action? Seek(int id)
        {
            for (int i = 0; i < ActionQeue.Count; i++)
            {
                if (ActionQeue.GetValueAtIndex(i).TryGetValue(id, out Action? a)) return a;
            }
            return null;
        }

        internal static void Tick()
        {
            //System.Diagnostics.Debug.WriteLine("Tick");
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (ActionQeue.TryGetValue(now,out Dictionary<int,Action>? q) && q != null)
            {
                System.Diagnostics.Debug.WriteLine("Invoking Action");
                q.Select(x => x.Value).ToList().ForEach(x => x.Invoke());
                ActionQeue.Remove(now);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Picking Leftover Action");
                KeyValuePair<TimeSpan,Dictionary<int,Action>?>? missedAction = ActionQeue.FirstOrDefault(x => x.Key < now);
                if (missedAction.HasValue && missedAction.Value.Value != null)
                {
                    missedAction.Value.Value.Select(x => x.Value).ToList().ForEach(x => x.Invoke());
                    ActionQeue.Remove(missedAction.Value.Key);
                }
            }
        }
        internal static DateTime RoundToMinute(DateTime dateTime)
        {
            if (dateTime.Second == 0 && dateTime.Millisecond == 0)
                return dateTime;

            return dateTime.AddMinutes(1)
                .AddSeconds(-dateTime.Second)
                .AddMilliseconds(-dateTime.Millisecond);
        }

        private static TimeSpan RoundTo5S(TimeSpan t)
        {
            if (t.Seconds % 5 == 0) return t;
            return t.Add(TimeSpan.FromSeconds((int)Math.Ceiling((double)t.Seconds / 5) * 5 - t.Seconds));
        }
    }
}

using fltstd26.board;
namespace fltstd26.system
{
    internal static class TimeServ
    {
        private static Scheduler? clock = null;
        private static int ActionCounter = 0;
        private static int LoopCounter = 0;
        public static bool ClockActive => clock?.IsRunning ?? false;
        //Run ID, Run at Time, Action
        private static readonly List<ScheduledAction> ActionQeue = [];
        //(Interval, LastExec),(ID, Action)
        private static readonly List<LoopAction> ActionLoop = [];
        internal static void Init()
        {
            clock ??= new(TimeSpan.FromSeconds(5),(s,e) => Tick(),true,RoundToMinute(DateTime.Now) - DateTime.Now - TimeSpan.FromSeconds(5));
            ConProc.Log("[TIME] Clock started");
        }

        internal static int Schedule(DateTime executionTime,Action action)
        {
            DateTime round = RoundTo5S(executionTime);
            System.Diagnostics.Debug.WriteLine("An Action was placed to run at " + round.ToString("g"));
            ActionQeue.Add(new() { ID = ActionCounter, ScheduledTime = round, ToDo = action});
            return ActionCounter++;
        }

        internal static void Clear()
        {
            ActionQeue.Clear();
            ActionLoop.Clear();
            BoardController.ClockID = -1;
        }

        internal static void Close()
        {
            clock?.Pause();
            clock = null;
        }

        internal static void Unschedule(int id)
        {
            ActionQeue.RemoveAt(ActionQeue.FindIndex(x => x.ID == id));
        }
        internal static int Reschedule(int id, DateTime time)
        {
            //Wenn ein KeyValue Pair mit der ID gefunden wird, wird das ereignis dort entfernt und an 
            ScheduledAction? a = Seek(id);
            if (time == a?.ScheduledTime) return id;
            if (a != null)
            {
                Unschedule(id);
                return Schedule(time,a.ToDo);       
            }
            return -1;
        }
        internal static ScheduledAction? Seek(int id) => ActionQeue.Find(x => x.ID == id);
        internal static void EarlyInvoke(int id)
        {
            ScheduledAction? a = ActionQeue.Find(x => x.ID == id);
            if (a != null)
            {
                a.ToDo.Invoke();
                ActionQeue.Remove(a);
            }
        }


        //Reoccuring
        internal static int ScheduleRO(TimeSpan interval,Action action,bool minute)
        {
            TimeSpan round = RoundTo5S(interval);
            System.Diagnostics.Debug.WriteLine($"A reoccuring action was placed with an interval of {round.TotalSeconds}s");
            ActionLoop.Add(new() { ID = LoopCounter,Interval = round,LastExec = minute ? RoundToMinute(DateTime.Now, true) : DateTime.Now,ToDo = action });
            return LoopCounter++;
        }

        internal static void UnscheduleRO(int id) => ActionLoop.RemoveAll(x => x.ID == id);

        internal static LoopAction? SeekRO(int id) => ActionLoop.Find(x => x.ID == id);
        internal static void EarlyInvokeRO(int id)
        {
            LoopAction? a = ActionLoop.Find(x => x.ID == id);
            if (a != null)
            {
                a.ToDo.Invoke();
                ActionLoop.Remove(a);
            }
        }

        internal static List<ScheduledAction> RequestQeue() => ActionQeue;
        internal static List<LoopAction> RequestQeueRO() => ActionLoop;

        internal static void Tick()
        {
            //System.Diagnostics.Debug.WriteLine("Tick");
            DateTime now = DateTime.Now;

            //Action Qeue
            ScheduledAction[] sas = [..ActionQeue.Where(x => x.ScheduledTime == now)];
            if (sas.Length > 0)
            {
                for (int i = 0; i < sas.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Invoking Action {sas[i].ID}: Current Time {now.ToLongTimeString()} - Scheduled Time {sas[i].ScheduledTime.ToLongTimeString()}");
                    sas[i].ToDo.Invoke();
                    ActionQeue.Remove(sas[i]);
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("Picking Leftover Action")
                ScheduledAction[] missed = [.. ActionQeue.Where(x => x.ScheduledTime < now)];
                for (int i = 0; i < missed.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine($"Invoking Leftover Action {missed[i].ID}: Current Time {now.ToLongTimeString()} - Scheduled Time {missed[i].ScheduledTime.ToLongTimeString()}");
                    missed[i].ToDo.Invoke();
                    ActionQeue.Remove(missed[i]);
                }
            }

            //Action Loop
            LoopAction[] readyActions = [..ActionLoop.Where(la => now - la.LastExec >= la.Interval)];
            for (int i = 0; i < readyActions.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine($"Invoking Loop Action {readyActions[i].ID}: Current Time {now.ToLongTimeString()} - Last Exec {readyActions[i].LastExec.ToLongTimeString()} - Interval of {readyActions[i].Interval.TotalSeconds}s");
                readyActions[i].ToDo.Invoke();
                readyActions[i].LastExec = now;
            }
        }

        internal static DateTime RoundToMinute(DateTime dateTime, bool down = false)
        {
            if (dateTime.Second == 0 && dateTime.Millisecond == 0)
                return dateTime;

            return dateTime.AddMinutes(down ? -1 : 1)
                .AddSeconds(-dateTime.Second)
                .AddMilliseconds(-dateTime.Millisecond);
        }

        private static DateTime RoundTo5S(DateTime t)
        {
            if (t.Second % 5 == 0) return t;
            return t.Add(TimeSpan.FromSeconds((int)Math.Ceiling((double)t.Second / 5) * 5 - t.Second));
        }
        private static TimeSpan RoundTo5S(TimeSpan t)
        {
            if (t.Seconds % 5 == 0) return t;
            return t.Add(TimeSpan.FromSeconds((int)Math.Ceiling((double)t.Seconds / 5) * 5 - t.Seconds));
        }
    }

    public class LoopAction
    {
        public int ID {  get; init; }
        public TimeSpan Interval { get; set; }
        public DateTime LastExec { get; set; }
        public required Action ToDo { get; set; }
    }

    public class  ScheduledAction
    {
        public DateTime ScheduledTime { get; set; }
        public int ID { get; init; }
        public required Action ToDo { get; set; }
    }
}

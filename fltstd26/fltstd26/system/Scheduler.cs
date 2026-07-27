namespace fltstd26.system
{
    internal class Scheduler
    {
        public bool IsRunning => timer?.IsRunning ?? false;
        private readonly IDispatcherTimer? timer;
        internal Scheduler(TimeSpan? iv,EventHandler ev,bool repeat,TimeSpan? synchronize = null,bool autostart = true)
        {
            System.Diagnostics.Debug.WriteLine("TimerSpan: " + iv?.ToString("G"));
            timer = Dispatcher.GetForCurrentThread()?.CreateTimer();
            if (timer is null) return;
            timer.Interval = iv ?? TimeSpan.FromSeconds(1);
            timer.Tick += ev;
            timer.IsRepeating = repeat;
            if(autostart)
            {
                if (synchronize != null)
                {
                    //Habe ich schon erwähnt, dass ich Rekursion liebe? ;-;
                    _ = new Scheduler(synchronize,(s,e) => Start(),false);
                }
                else Start();
            }
        }

        internal void Start() { timer?.Start(); }
        internal void Pause() { if(timer is not null && timer.IsRunning) timer.Stop(); }
    }
}

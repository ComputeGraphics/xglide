namespace fltstd26.system
{
    internal class Scheduler
    {
        private readonly IDispatcherTimer? timer;
        public Scheduler(TimeSpan? iv,EventHandler ev,bool repeat,TimeSpan? synchronize = null)
        {
            timer = Dispatcher.GetForCurrentThread()?.CreateTimer();
            if (timer is null) return;
            timer.Interval = iv ?? TimeSpan.FromSeconds(1);
            timer.Tick += ev;
            timer.IsRepeating = repeat;
            if (synchronize != null)
            {
                //Habe ich schon erwähnt, dass ich Rekursion liebe? ;-;
                _ = new Scheduler(synchronize,Start,false);
            }
            else Start(null,null);
        }

        private void Start(object? s, EventArgs? e) { timer?.Start(); System.Diagnostics.Debug.WriteLine("Synced"); }

        public void Terminate() { if(timer is not null && timer.IsRunning) timer.Stop(); }
    }
}

using System;

namespace fltstd26.system
{
    internal class Scheduler
    {
        Timer timer;
        public Scheduler(DateTime time, Action ac)
        { 
            timer = new Timer((e) =>
            {
                ac();
            }, null, time - DateTime.Now, TimeSpan.FromMilliseconds(-1));
        }

        public void Terminate()
        {
            timer.Dispose();
        }


    }
}

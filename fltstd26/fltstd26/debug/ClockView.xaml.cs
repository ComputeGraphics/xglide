using fltstd26.Resources.Texts;
using fltstd26.system;

namespace fltstd26.debug;

public partial class ClockView : ContentView
{
    private readonly ScheduledAction? LinkedScheduledAction;
    private readonly LoopAction? LinkedLoopAction;
    public ClockView(ScheduledAction? sc, LoopAction? lp)
	{          
		InitializeComponent();
        if (sc != null)
        {
            LinkedScheduledAction = sc;
            Icon.Source = "calendar.png";
            ID.Text = sc.ID.ToString();
            Info.Text = $"{Lang.execution_at}{sc.ScheduledTime:R}";
        }
        else if (lp != null)
        {
            LinkedLoopAction = lp;
            Icon.Source = "refresh.png";
            ID.Text = lp.ID.ToString();
            DateTime newexec = lp.LastExec + lp.Interval;
            Info.Text = $"{Lang.execution_last}{lp.LastExec:R}  -  {Lang.execution_at}{newexec:R}  -  {Lang.interval}: {lp.Interval:g}";
        }
    }

    private void DeleteClick(object sender, EventArgs e)
	{
        if (LinkedScheduledAction != null)
        {
            TimeServ.Unschedule(LinkedScheduledAction.ID);
        }
        else if (LinkedLoopAction != null)
        {
            TimeServ.UnscheduleRO(LinkedLoopAction.ID);
        }
	}

    private void TriggerClick(object sender,EventArgs e)
    {
        if (LinkedScheduledAction != null)
        {
            TimeServ.EarlyInvoke(LinkedScheduledAction.ID);
        }
        else if (LinkedLoopAction != null)
        {
            TimeServ.EarlyInvokeRO(LinkedLoopAction.ID);
        }
    }
}
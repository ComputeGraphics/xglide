using fltstd26.system;

namespace fltstd26.debug;

public partial class ClockCheck : ContentPage
{
    public ClockCheck()
    {
        InitializeComponent();
        Refresh();
    }

    private void Refresh()
    {
        EventStack.Clear();
        TimeServ.RequestQeueRO().ForEach(e => EventStack.Add(new ClockView(null,e)));
        TimeServ.RequestQeue().ForEach(e => EventStack.Add(new ClockView(e,null)));
    }

    private void RefreshClick(object sender,EventArgs e) => Refresh();
    private void RestartClick(object sender,EventArgs e)
    {
        TimeServ.Close();
        TimeServ.Init();
    }
    private void TriggerClick(object sender,EventArgs e) => TimeServ.Tick();
    private void ClearClick(object sender,EventArgs e) => TimeServ.Clear();
    private void QuitClicked(object sender,EventArgs e) => Navigation.PopModalAsync();
}
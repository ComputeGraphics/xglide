namespace fltstd26.system.modals;

public partial class WindowLock : ContentPage
{
	private readonly Action? ucp = null;
	public WindowLock(DateTime? unlockat, Action? usercancellationprocedure)
	{
		InitializeComponent();
		if(unlockat != null)
		{
			TimeServ.Schedule(unlockat.Value,() => Navigation.PopModalAsync());
		}
		ucp = usercancellationprocedure;
	}

	internal void ReleaseLock() => Navigation.PopModalAsync();

	private void OnOK(object sender,EventArgs e)
	{
        ucp?.Invoke();
		Navigation.PopModalAsync();
    }
}
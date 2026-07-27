namespace fltstd26.system.modals;

public partial class Entry : ContentPage
{
    private TaskCompletionSource<string?>? _tcs;
    public Entry(string Title,string Subtitle,string Placeholder,string? Preset)
	{
		InitializeComponent();
        ItemTitle.Text = Title;
        ItemSubtitle.Text = Subtitle;
        GenericEntry.Placeholder = Placeholder;
        if(Preset != null) GenericEntry.Text = Preset;
    }

    public Task<string?> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<string?>();
        return _tcs.Task;
    }

    private void OnConfirm(object sender,EventArgs args)
    {
        _tcs?.SetResult(GenericEntry.Text);
        Navigation.PopModalAsync();
    }

    private void OnCancel(object sender,EventArgs args)
    {
        _tcs?.SetResult(null);
        Navigation.PopModalAsync();
    }
}
using CommunityToolkit.Maui.Behaviors;
using fltstd26.etc;

namespace fltstd26.system.modals;

public partial class YesNo : ContentPage
{
    private TaskCompletionSource<bool>? _tcs;
    public YesNo(string Title, string Subtitle)
	{
		InitializeComponent();
        ItemTitle.Text = Title;
        ItemSubtitle.Text = Subtitle;
    }

    public Task<bool> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<bool>();
        return _tcs.Task;
    }

    private void OnConfirm(object sender,EventArgs args)
    {
        _tcs?.SetResult(true);
        Navigation.PopModalAsync();
    }

    private void OnCancel(object sender,EventArgs args)
    {
        _tcs?.SetResult(false);
        Navigation.PopModalAsync();
    }
}
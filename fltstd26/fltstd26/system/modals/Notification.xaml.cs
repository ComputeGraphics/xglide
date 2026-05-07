namespace fltstd26.system.modals;

public partial class Notification : ContentPage
{
	public Notification(string Title, string Subtitle)
	{
		InitializeComponent();
        ItemTitle.Text = Title;
        ItemSubtitle.Text = Subtitle;
    }

	public void OnOK(object sender, EventArgs e) => Navigation.PopModalAsync();
}
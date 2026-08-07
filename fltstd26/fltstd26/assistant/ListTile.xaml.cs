namespace fltstd26.assistant;

public partial class ListTile : ContentView
{
    public CheckBox Checked;
    private readonly Action?[] Buttons;
	public ListTile(bool showcheck, string icon, string title, string subtitle, Action?[] btn)
	{
		InitializeComponent();
        Checked = Check;
		Check.IsVisible = showcheck;
        Icon.Source = icon;
        Title.Text = title;
        Description.Text = subtitle;
        Buttons = btn;
        Play.IsEnabled = btn[0] != null;
        Play.IsVisible = btn[0] != null;
        View.IsEnabled = btn[1] != null;
        View.IsVisible = btn[1] != null;
        Modify.IsEnabled = btn[2] != null;
        Share.IsEnabled = btn[3] != null;
        Delete.IsEnabled = btn[4] != null;
	}

    private void PlayClick(object sender,EventArgs e) => Buttons[0]!.Invoke();
    private void ViewClick(object sender,EventArgs e) => Buttons[1]!.Invoke();
    private void ModifyClick(object sender,EventArgs e) => Buttons[2]!.Invoke();
    private void ShareClick(object sender,EventArgs e) => Buttons[3]!.Invoke();
    private void DeleteClick(object sender,EventArgs e) => Buttons[4]!.Invoke();
}
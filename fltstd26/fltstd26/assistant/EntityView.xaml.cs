namespace fltstd26.assistant.profiles;

public partial class EntityView : ContentView
{
	(Action, Action) Buttons;
    public EntityView(string icon,string title, string subtitle, string? addtional, (Action, Action) buttons)
	{
		InitializeComponent();
		Icon.Source = icon;
		Buttons = buttons;
		Title.Text = title;
		Subtitle.Text = subtitle;
		if(addtional != null) Additional.Text = addtional;

    }

	private void DeleteClick(object sender, EventArgs e) => Buttons.Item1.Invoke();
	private void ModifyClick(object sender, EventArgs e) => Buttons.Item2.Invoke();
}
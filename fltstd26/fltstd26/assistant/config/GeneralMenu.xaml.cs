using fltstd26.Resources.Texts;

namespace fltstd26.assistant.config;

public partial class GeneralMenu : ContentView
{
	public string Name => ConfigNameEntry.Text;
	public string Author => ConfigAuthorEntry.Text;

	private etc.ConfigSettings _instance;
    public GeneralMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
		ConfigNameEntry.Text = cfg.Name;
		ConfigAuthorEntry.Text = cfg.Creator;
		ConfigMetadataLabel.Text = $"{Lang.creation_doubledot} {cfg.Creation}\r\n{Lang.change_doubledot} {cfg.LastChange}";
	}

	private void TextChanged(object sender, EventArgs e)
	{
		_instance.Name = ConfigNameEntry.Text;
		_instance.Creator = ConfigAuthorEntry.Text;
	}
}
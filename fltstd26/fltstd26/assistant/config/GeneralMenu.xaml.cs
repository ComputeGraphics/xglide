using fltstd26.Resources.Texts;

namespace fltstd26.assistant.config;

public partial class GeneralMenu : ContentView
{
	public string Name => ConfigNameEntry.Text;
	public string Author => ConfigAuthorEntry.Text;

	private readonly etc.ConfigSettings _instance;
	private bool _afterinit = false;
    public GeneralMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
        Unloaded += (s,e) => _afterinit = false;
        Loaded += (s,e) => SyncViews();
	}

	private void SyncViews()
	{
        ConfigNameEntry.Text = _instance.Name;
        ConfigAuthorEntry.Text = _instance.Creator;
        ConfigMetadataLabel.Text = $"{Lang.creation_doubledot} {_instance.Creation}\r\n{Lang.change_doubledot} {_instance.LastChange}";
		_afterinit = true;
    }
	private void TextChanged(object sender, EventArgs e)
	{
		if (_afterinit)
		{
			_instance.Name = ConfigNameEntry.Text;
			_instance.Creator = ConfigAuthorEntry.Text;
		}
	}
}
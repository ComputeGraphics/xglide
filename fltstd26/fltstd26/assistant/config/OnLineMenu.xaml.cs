namespace fltstd26.assistant.config;

public partial class OnLineMenu : ContentView
{
    private readonly etc.ConfigSettings _instance;
    private bool _afterinit = false;
    public OnLineMenu(etc.ConfigSettings cfg)
	{
        _instance = cfg;
		InitializeComponent();

        Unloaded += (s,e) => _afterinit = false;
        Loaded += (s,e) => SyncViews();
    }

    private void SyncViews()
    {
        OGNStatusControl.IsToggled = _instance.OGNStatus;
        OGNSyncLevelControl.Value = _instance.OGNSyncLevel;
        OGNToleranceControl.Value = _instance.OGNTolerance;
        TakeoffDurationControl.Value = _instance.TakeoffDuration;
        HomebaseControl.Text = _instance.Homebase;
    }

    private void SyncSettings(object  sender, EventArgs e)
    {
        if (_afterinit)
        {
            _instance.OGNSyncLevel = (int)OGNSyncLevelControl.Value;
            _instance.OGNTolerance = (int)OGNToleranceControl.Value;
            _instance.TakeoffDuration = (int)TakeoffDurationControl.Value;
        }
    }

    private void SyncSwitch(object sender, EventArgs e)
    {
        if(_afterinit)
        {
            _instance.OGNStatus = OGNStatusControl.IsToggled;
        }
    }
    private void HomebaseControlChanged(object sender, EventArgs e)
    {
        if (_afterinit)
        {
            HomebaseCheck.IsVisible = true;
        }
    }

    private void HomebaseControlSync(object sender, EventArgs e)
    {
        if (_afterinit)
        {
            _instance.Homebase = HomebaseControl.Text;
            HomebaseCheck.IsVisible = false;
        }
    }
}
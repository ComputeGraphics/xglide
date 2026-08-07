namespace fltstd26.assistant.config;

public partial class PlannerMenu : ContentView
{
	private readonly etc.ConfigSettings _instance;
	private bool _afterinit = false;

    public PlannerMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
        Unloaded += (s,e) => _afterinit = false;
        Loaded += (s,e) => SyncViews();
	}

	private void SyncViews()
	{
        AutoASAPControl.IsToggled = _instance.AutoASAP;
        EnableSlotsControl.IsToggled = _instance.EnableSlots;
        AntiColControl.IsToggled = _instance.AntiCol;
        AutoTimeCheckControl.IsToggled = _instance.AutoTimeCheck;
        IgnoreTransactionLengthControl.IsToggled = _instance.IgnoreTransactionLength;
        IgnoreTransactionWeightControl.IsToggled = _instance.IgnoreTransactionWeight;
		_afterinit = true;
    }

	private void SyncSwitch(object sender, EventArgs e)
	{
		if (_afterinit)
		{
			_instance.AutoASAP = AutoASAPControl.IsToggled;
			_instance.EnableSlots = EnableSlotsControl.IsToggled;
			_instance.AntiCol = AntiColControl.IsToggled;
			_instance.AutoTimeCheck = AutoTimeCheckControl.IsToggled;
			_instance.IgnoreTransactionLength = IgnoreTransactionLengthControl.IsToggled;
			_instance.IgnoreTransactionWeight = IgnoreTransactionWeightControl.IsToggled;
		}
	}
}
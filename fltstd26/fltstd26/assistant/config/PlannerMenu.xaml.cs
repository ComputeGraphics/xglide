namespace fltstd26.assistant.config;

public partial class PlannerMenu : ContentView
{
	private readonly etc.ConfigSettings _instance;

    public PlannerMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
		AutoASAPControl.IsToggled = cfg.AutoASAP;
		EnableSlotsControl.IsToggled = cfg.EnableSlots;
		AntiColControl.IsToggled = cfg.AntiCol;
		AutoTimeCheckControl.IsToggled = cfg.AutoTimeCheck;
		IgnoreTransactionLengthControl.IsToggled = cfg.IgnoreTransactionLength;
		IgnoreTransactionWeightControl.IsToggled = cfg.IgnoreTransactionWeight;
	}

	private void SyncSwitch(object sender, EventArgs e)
	{
		_instance.AutoASAP = AutoASAPControl.IsToggled;
		_instance.EnableSlots = EnableSlotsControl.IsToggled;
		_instance.AntiCol = AntiColControl.IsToggled;
        _instance.AutoTimeCheck = AutoTimeCheckControl.IsToggled;
		_instance.IgnoreTransactionLength = IgnoreTransactionLengthControl.IsToggled;
        _instance.IgnoreTransactionWeight = IgnoreTransactionWeightControl.IsToggled;
	}
}
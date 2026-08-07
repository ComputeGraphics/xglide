namespace fltstd26.assistant.config;

public partial class UxMenu : ContentView
{
	private readonly etc.ConfigSettings _instance;
	private bool _afterinit;
	public UxMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
        Unloaded += (s,e) => _afterinit = false;
        Loaded += (s,e) => SyncViews();
	}

	private void SyncViews()
	{
        AskForNodeMoveControl.IsToggled = _instance.AskForNodeMove;
        AskForNodePriceChangeControl.IsToggled = _instance.AskForNodePriceChange;
        HidePastSlotsControl.IsToggled = _instance.HidePastSlots;
        SlotToleranceControl.Value = _instance.SlotTolerance;
		_afterinit = true;
    }

	private void SyncSwitch(object sender, EventArgs e)
	{
		if(_afterinit && HidePastSlotsControl != null)
		{
            _instance.AskForNodeMove = AskForNodeMoveControl.IsToggled;
            _instance.AskForNodePriceChange = AskForNodePriceChangeControl.IsToggled;
            _instance.HidePastSlots = HidePastSlotsControl.IsToggled;
        }
	}

	private void SyncSettings(object sender, EventArgs e)
	{
		if (_afterinit && SlotToleranceControl != null)
		{
			_instance.SlotTolerance = (int)SlotToleranceControl.Value;
		}
	}
}
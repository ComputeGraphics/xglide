namespace fltstd26.assistant.config;

public partial class UxMenu : ContentView
{
	private etc.ConfigSettings _instance;
	public UxMenu(etc.ConfigSettings cfg)
	{
		_instance = cfg;
		InitializeComponent();
		AskForNodeMoveControl.IsToggled = cfg.AskForNodeMove;
		AskForNodePriceChangeControl.IsToggled = cfg.AskForNodePriceChange;
		HidePastSlotsControl.IsToggled = cfg.HidePastSlots;
		SlotToleranceControl.Value = cfg.SlotTolerance;
	}

	private void SyncSwitch(object sender, EventArgs e)
	{
		if(HidePastSlotsControl != null)
		{
            _instance.AskForNodeMove = AskForNodeMoveControl.IsToggled;
            _instance.AskForNodePriceChange = AskForNodePriceChangeControl.IsToggled;
            _instance.HidePastSlots = HidePastSlotsControl.IsToggled;
        }
	}

	private void SyncSettings(object sender, EventArgs e)
	{
		if (SlotToleranceControl != null)
		{
			_instance.SlotTolerance = (int)SlotToleranceControl.Value;
		}
	}
}
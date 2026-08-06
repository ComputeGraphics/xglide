using fltstd26.core;

namespace fltstd26.assistant.config;

public partial class DefaultsMenu : ContentView
{
    private etc.ConfigSettings _instance;
    private List<Sheets.PriceCat> _pcs;
    private bool _afterinit = false;
    public DefaultsMenu(etc.ConfigSettings cfg)
    {

        _pcs = RData.GetPriceTable();
        _instance = cfg;
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("Initializing Defaults Menu");
        FallbackPriceCatControl.ItemsSource = _pcs.Select(c => c.Name).ToList();
        int i = _pcs.FindIndex(x => x.Id == _instance.FallbackPriceCat);
        if (i != -1) FallbackPriceCatControl.SelectedIndex = i;
        DelayToleranceControl.Value = _instance.DelayTolerance;
        MaxDelayControl.Value = _instance.MaxDelay;
        QuickToleranceControl.Value = _instance.QuickTolerance;
        DefaultTgtWeightControl.Text = _instance.DefaultTgtWeight.ToString();
        DefaultFltLengthControl.Text = _instance.DefaultFltLength.ToString();
        _afterinit = true;
    }

    private void SyncSettings(object sender,EventArgs e)
    {
        if (QuickToleranceControl != null)
        {
            _instance.DelayTolerance = (int)DelayToleranceControl.Value;
            _instance.MaxDelay = (int)MaxDelayControl.Value;
            _instance.QuickTolerance = (int)QuickToleranceControl.Value;
            if (FallbackPriceCatControl.SelectedIndex >= 0 && FallbackPriceCatControl.SelectedIndex < _pcs.Count)
            {
                _instance.FallbackPriceCat = _pcs[FallbackPriceCatControl.SelectedIndex].Id;
            }
        }
    }


    private void TgtWeightControlChanged(object sender,EventArgs e)
    {
        if (_afterinit) DefaultTgtWeightCheck.IsVisible = true;
    }

    private void TgtWeightControlSync(object sender,EventArgs e)
    {
        if (Int32.TryParse(DefaultTgtWeightControl.Text,out int length))
        {
            _instance.DefaultTgtWeight = length;
            DefaultTgtWeightCheck.IsVisible = false;
        }
        else DefaultTgtWeightControl.Text = _instance.DefaultTgtWeight.ToString();
    }

    private void FltLengthControlChanged(object sender,EventArgs e)
    {
        if (_afterinit) DefaultFltLengthCheck.IsVisible = true;
    }

    private void FltLengthControlSync(object sender,EventArgs e)
    {
        if (Int32.TryParse(DefaultFltLengthControl.Text,out int length))
        {
            _instance.DefaultTgtWeight = length;
            DefaultFltLengthCheck.IsVisible = false;
        }
        else DefaultFltLengthControl.Text = _instance.DefaultTgtWeight.ToString();
    }

}
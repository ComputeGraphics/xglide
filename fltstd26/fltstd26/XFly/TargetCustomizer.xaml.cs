using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;

namespace fltstd26.XFly;

public partial class TargetCustomizer : ContentPage
{
    private TaskCompletionSource<(Sheets.Target?, Sheets.Flt?)>? _tcs;

    private readonly Sheets.Target? _target = null;
    private readonly Sheets.Flt? _flight = null;
    private readonly string[]? _splitAdds = null;
    private readonly bool Swap;
    public TargetCustomizer(Sheets.Target? t = null,Sheets.Flt? f = null,bool HideEID = false,bool SwapLength = false,string Title = "N/A")
    {
        InitializeComponent();
        try
        {
            Swap = SwapLength;
            if (t != null)
            {
                _target = t;
                TargetOptionsStack.IsVisible = true;
                if (t.Name != null) TGT_Name_Entry.Text = t.Name;
                TGT_Persistent_Enable.IsChecked = t.Persistent;
                TGT_Price_Entry.Text = GSettings.UnformatPrice(t.Price);
                TGT_Quickticket_Enable.IsChecked = t.QuickTicket;
                TGT_Weight_Entry.Text = t.Weight.ToString();
            }
            if (f != null)
            {
                _flight = f;
                FlightOptionsStack.IsVisible = true;
                FLT_EID_Entry.IsVisible = !HideEID;
                if (f.EId != null) FLT_EID_Entry.Text = f.EId;
                FlightOptionHeader.Text = SwapLength ? Title + ":" : Lang.flight_option;
                FLT_EID_Entry.Placeholder = SwapLength ? Lang.xplan_length : Lang.fltno;
                FLT_Status_Dropdown.SelectedIndex = f.Status;
                _splitAdds = f.Add?.Split(';') ?? [];
                System.Diagnostics.Debug.WriteLine("SplitCount: " + _splitAdds.Length.ToString());
                for (int i = 0; i < USettings.Instance.Additionals.Count; i++) FLTAddsEntryContainer.Add(new Entry() { Placeholder = USettings.Instance.Additionals[i],Text = i < _splitAdds.Length ? _splitAdds[i] : null });
            }
        }
        catch (Exception ex)
        {
            ConProc.Log("[TGTCZR] Target Customizer crashed: " + ex.Message,2);
            _tcs?.SetResult((null, null));
            Navigation.PopModalAsync();
        }
    }

    public Task<(Sheets.Target?, Sheets.Flt?)> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<(Sheets.Target?, Sheets.Flt?)>();
        return _tcs.Task;
    }

    public void OnCancel(object sender,EventArgs e)
    {
        _tcs?.SetResult((null, null));
        Navigation.PopModalAsync();
    }
    public void OnConfirm(object sender,EventArgs e)
    {
        try
        {
            if (_target != null)
            {
                if (GSettings.ValueChanged(_target.Name,TGT_Name_Entry.Text)) _target.Name = TGT_Name_Entry.Text;
                _target.Persistent = TGT_Persistent_Enable.IsChecked;
                _target.QuickTicket = TGT_Quickticket_Enable.IsChecked;
                if (GSettings.ValueChanged(GSettings.UnformatPrice(_target.Price),TGT_Price_Entry.Text) && Int32.TryParse(TGT_Price_Entry.Text,out int p)) _target.Price = GSettings.InterpretePrice(TGT_Price_Entry.Text.Trim());
                if (GSettings.ValueChanged(_target.Weight.ToString(),TGT_Weight_Entry.Text) && Int32.TryParse(TGT_Weight_Entry.Text,out int w)) _target.Weight = w;
            }

            if (_flight != null)
            {
                if (GSettings.ValueChanged(_flight.EId,FLT_EID_Entry.Text))
                {
                    if (Swap && Int32.TryParse(FLT_EID_Entry.Text,out int length)) _flight.Slot = length;
                    else _flight.EId = FLT_EID_Entry.Text;
                }
                if (!FLT_Status_Dropdown_Enable.IsChecked) _flight.Status = (byte)FLT_Status_Dropdown.SelectedIndex;
                if (_splitAdds != null)
                {
                    Entry[] addEntries = [.. FLTAddsEntryContainer.Children.OfType<Entry>()];
                    System.Diagnostics.Debug.WriteLine("Add Entries: " + addEntries.Length.ToString());
                    if (addEntries.Length == _splitAdds.Length - 1)
                    {
                        for (int i = 0; i < addEntries.Length; i++)
                        {
                            System.Diagnostics.Debug.WriteLine(i.ToString() + " Looping Adds: " + addEntries[i].Text + " vs. " + _splitAdds[i]);
                            if (GSettings.ValueChanged(_splitAdds[i],addEntries[i].Text)) _splitAdds[i] = addEntries[i].Text ?? "";
                        }
                    }
                    string adds = string.Join(';',_splitAdds);
                    _flight.Add = adds == "" ? ";" : adds;
                }
            }

            _tcs?.SetResult((_target, _flight));
            Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            ConProc.Log("[TGTCZR] Target Customizer confirmation failed: " + ex.Message,2);
            _tcs?.SetResult((null, null));
            Navigation.PopModalAsync();
        }
    }
}
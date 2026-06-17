using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;

namespace fltstd26.XFly;

public partial class TargetCustomizer : ContentPage
{
    private TaskCompletionSource<(Sheets.Target?, Sheets.Flt?)>? _tcs;

    private readonly Sheets.Target? _target = null;
    private readonly Sheets.Flt? _flight = null;
    private readonly string[]? _splitAdds = null;
    bool Swap;
    public TargetCustomizer(Sheets.Target? t = null,Sheets.Flt? f = null,bool HideEID = false, bool SwapLength = false, string Title = "N/A")
    {
        InitializeComponent();
        Swap = SwapLength;
        if (t != null)
        {
            _target = t;
            TargetOptionsStack.IsVisible = true;
            TGT_Name_Entry.Text = t.Name;
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
            FLT_EID_Entry.Text = f.EId;
            FlightOptionHeader.Text = SwapLength ? Title + ":" : Lang.flight_option;
            FLT_EID_Entry.Placeholder = SwapLength ? Lang.xplan_length : Lang.fltno;
            FLT_Status_Dropdown.SelectedIndex = f.Status;
            _splitAdds = f.Add?.Split(';') ?? [];
            for (int i = 0; i < _splitAdds.Length; i++) FLTAddsEntryContainer.Add(new Entry() { Placeholder = USettings.Additionals[i],Text = _splitAdds[i] });
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
        if (_target != null)
        {
            if (ValueChanged(_target.Name,TGT_Name_Entry.Text)) _target.Name = TGT_Name_Entry.Text;
            _target.Persistent = TGT_Persistent_Enable.IsChecked;
            _target.QuickTicket = TGT_Quickticket_Enable.IsChecked;
            if (ValueChanged(GSettings.UnformatPrice(_target.Price),TGT_Price_Entry.Text) && Int32.TryParse(TGT_Price_Entry.Text,out int p)) _target.Price = GSettings.InterpretePrice(TGT_Price_Entry.Text.Trim());
            if (ValueChanged(_target.Weight.ToString(),TGT_Weight_Entry.Text) && Int32.TryParse(TGT_Weight_Entry.Text,out int w)) _target.Weight = w;
        }

        if (_flight != null)
        {
            if (ValueChanged(_flight.EId,FLT_EID_Entry.Text))
            {
                if(Swap && Int32.TryParse(FLT_EID_Entry.Text, out int length)) _flight.Slot = length;
                else _flight.EId = FLT_EID_Entry.Text;
            }
            if (!FLT_Status_Dropdown_Enable.IsChecked) _flight.Status = (byte)FLT_Status_Dropdown.SelectedIndex;
            if (_splitAdds != null)
            {
                Entry[] addEntries = [.. FLTAddsEntryContainer.Children.OfType<Entry>()];
                if (addEntries.Length == _splitAdds.Length) for (int i = 0; i < _splitAdds.Length; i++) if (ValueChanged(_splitAdds[i],addEntries[i].Text)) _splitAdds[i] = addEntries[i].Text ?? "";
                _flight.Add = string.Join(';',_splitAdds);
            }
        }

        _tcs?.SetResult((_target, _flight));
        Navigation.PopModalAsync();
    }

    static Func<string?,string?,bool> ValueChanged => (prev,aft) =>  !string.IsNullOrEmpty(aft) && (prev == null || prev.Trim() != aft.Trim());
}
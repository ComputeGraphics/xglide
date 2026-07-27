using fltstd26.core;
using fltstd26.etc;

namespace fltstd26.assistant.profiles;

public partial class SlotCreator : ContentPage
{
    private TaskCompletionSource<Sheets.Slot?>? _tcs;
    private readonly Sheets.Slot? s;
	public SlotCreator(Sheets.Slot? preslot)
	{
		InitializeComponent();
		if(preslot != null)
		{
			SDatePicker.Date = preslot.STime.Date;
			STimePicker.Time = preslot.STime.TimeOfDay;
            FDatePicker.Date = preslot.FTime.Date;
            FTimePicker.Time = preslot.FTime.TimeOfDay;
            LengthEntry.Text = preslot.Length.ToString();
            s = preslot;
        }
	}

	private void AddClick(object sender, EventArgs e)
	{
        //System.Diagnostics.Debug.WriteLine("TimeChange Fired: " + GSettings.TimeChanged(s?.STime.TimeOfDay,STimePicker.Time).ToString());
        DateTime stimedate = GSettings.DateChanged(s?.STime,SDatePicker.Date) ? SDatePicker.Date : s?.STime.Date ?? DateTime.Now.Date;
        DateTime stime = stimedate.Add(GSettings.TimeChanged(s?.STime.TimeOfDay,STimePicker.Time) ? STimePicker.Time : s?.STime.TimeOfDay ?? DateTime.Now.TimeOfDay);

        DateTime ftimedate = GSettings.DateChanged(s?.FTime,FDatePicker.Date) ? FDatePicker.Date : s?.FTime.Date ?? DateTime.Now.Date;
        DateTime ftime = ftimedate.Add(GSettings.TimeChanged(s?.FTime.TimeOfDay,FTimePicker.Time) ? FTimePicker.Time : s?.FTime.TimeOfDay ?? DateTime.Now.TimeOfDay);


        Sheets.Slot fts = new()
        {
            Id = s?.Id ?? 0,
            Delay = s?.Delay ?? false,
            Length = GSettings.ValueChanged(s?.Length.ToString(),LengthEntry.Text) && Int32.TryParse(LengthEntry.Text,out int l) ? l : s?.Length ?? 0,
            STime = stime,
            FTime = ftime,
        };
        _tcs?.SetResult(fts);
        Navigation.PopModalAsync();
	}

    private void CancelClick(object sender,EventArgs e)
    {
        _tcs?.SetResult(null);
        Navigation.PopModalAsync();
    }

    public Task<Sheets.Slot?> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<Sheets.Slot?>();
        return _tcs.Task;
    }


}
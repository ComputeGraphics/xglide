using fltstd26.core;
using fltstd26.etc;

namespace fltstd26.assistant.profiles;

public partial class AircraftCreator : ContentPage
{
    private TaskCompletionSource<Sheets.Lfz?>? _tcs;
    private readonly List<Sheets.PriceCat> pcs;
    private readonly List<Sheets.Slot> slts;
    private readonly Sheets.Lfz? ac = null;
    private readonly List<byte> availslots = [];

    public AircraftCreator(List<Sheets.Slot> slots, List<Sheets.PriceCat> pricecats, Sheets.Lfz? prelfz)
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("Open ID: " + prelfz?.Id ?? "null");
        PriceCatPicker.ItemsSource = pricecats.Select(x => $"{x.Name} ({GSettings.UnformatPrice(x.Price)})").ToList();
        pcs = pricecats;
        slts = slots;
        foreach(Sheets.Slot s in slots)
        {
            Grid g = new()
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto),new ColumnDefinition(GridLength.Star) },
            };
            Label l = new()
            {
                Text = $"{s.STime:G} - {s.FTime:G}  ({s.Length}min)",
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center,
            };
            g.Add(l,1);
            CheckBox cb = new()
            {
                IsChecked = prelfz?.AvailTimes?.Where(x => x == s.Id).Any() ?? false,
                VerticalOptions = LayoutOptions.Center
            };
            g.Add(cb);
            SlotStack.Add(g);
        }

        PriceCatPicker.SelectedIndex = 0;


        if (prelfz != null)
        {
            NameEntry.Text = prelfz.Reg;
            TypeEntry.Text = prelfz.Type;
            AutoAssignToggle.IsChecked = prelfz.AutoAssign;
            SeatsEntry.Text = prelfz.Seats.ToString();
            IntervalEntry.Text = prelfz.Interval.ToString();
            if (prelfz.OGN != null) OGNEntry.Text = prelfz.OGN;
            PriceCatPicker.SelectedIndex = pricecats.FindIndex(x => x.Id == prelfz.PriceCat);
            //if (prelfz.AvailTimes != null) availslots.AddRange([.. prelfz.AvailTimes]);
            ac = prelfz;
        }
    }

    private void AddClick(object sender,EventArgs e)
    {
       
        for (int i = 0; i < slts.Count; i++)
        {
            if (slts[i].Id < 255 && (SlotStack.Children.OfType<Grid>().ElementAt(i).Children.OfType<CheckBox>().FirstOrDefault()?.IsChecked ?? false)) availslots.Add((byte)slts[i].Id);
        }
        System.Diagnostics.Debug.WriteLine("Avail Times: " + string.Join(',',availslots));
        Sheets.Lfz lfz = new()
        {
            Id = ac == null ? 0 : ac.Id,
            Reg = ac == null || GSettings.ValueChanged(ac.Reg,NameEntry.Text) ? NameEntry.Text : ac.Reg,
            Type = ac == null || GSettings.ValueChanged(ac.Type,TypeEntry.Text) ? TypeEntry.Text : ac.Type,
            PriceCat = pcs[PriceCatPicker.SelectedIndex].Id,
            OGN = OGNEntry.Text == "" ? null : ac?.OGN,
            AutoAssign = AutoAssignToggle.IsChecked,
            Interval = GSettings.ValueChanged(ac?.Interval.ToString(),IntervalEntry.Text) && Int32.TryParse(IntervalEntry.Text,out int interval) ? interval : ac?.Interval ?? 0,
            Seats = GSettings.ValueChanged(ac?.Seats.ToString(),SeatsEntry.Text) && Int32.TryParse(SeatsEntry.Text,out int seats) ? seats : ac?.Seats ?? 0,
            AvailTimes = [.. availslots]
        };
        _tcs?.SetResult(lfz);
        Navigation.PopModalAsync();
    }

    private void CancelClick(object sender,EventArgs e)
    {
        _tcs?.SetResult(null);
        Navigation.PopModalAsync();
    }

    public Task<Sheets.Lfz?> ShowAndSelect()
    {
        _tcs = new TaskCompletionSource<Sheets.Lfz?>();
        return _tcs.Task;
    }


}
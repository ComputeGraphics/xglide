using fltstd26.etc.online;
using System.Security.Cryptography.X509Certificates;

namespace fltstd26.debug;

public partial class OnlineFetch : Window
{
    public OnlineFetch()
    {
        InitializeComponent();
    }

    public async void Refresh_Click(object sender,EventArgs e)
    {
        FetchStatus.Text = "Fetching...";
        FetcherGrid.Clear();
        FetcherGrid.ColumnDefinitions.Clear();
        FetcherGrid.RowDefinitions.Clear();
        if (ViewRaw.IsChecked)
        {
            FetcherGrid.Add(new Label() { Text = await OGN.GetRaw(AP.Text) });
            FetchStatus.Text = "Finished";
        }
        else
        {
            OGN.OGNLogbook? logbook = await OGN.Get(AP.Text);

            if (logbook == null)
            {
                FetchStatus.Text = "No data found.";
                return;
            }

            var prop = new OGN.Flight().GetType().GetProperties();
            Label ap_label = new() { Text = $"Airfield: {logbook.airfield.name} ({logbook.airfield.code}), Country: {logbook.airfield.country}" };
            Label date_label = new() { Text = $"Date: {logbook.date:d}, Dawn: {logbook.airfield.time_info.dawn}, Dusk: {logbook.airfield.time_info.twilight}" };
            FetcherGrid.Add(ap_label);
            FetcherGrid.SetColumnSpan(ap_label, prop.Length);
            FetcherGrid.Add(date_label,0,1);
            FetcherGrid.SetColumnSpan(date_label,prop.Length);
            for (int i = 0; i < prop.Length; i++)
            {
                FetcherGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                FetcherGrid.Add(new Label() { Text = $"{prop[i].Name}" },i,2);
            }
            if (logbook.flights != null && logbook.devices != null)
            {
                for (int i = 0; i < logbook.flights.Count; i++)
                {
                    FetcherGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    FetcherGrid.Add(new Label() { Text = logbook.flights[i].device.HasValue ? logbook.devices[logbook.flights[i].device!.Value].registration : "N/A", FontAttributes = FontAttributes.Bold },0,i+3);
                    for (int colIndex = 1; colIndex < prop.Length; colIndex++)
                    {
                        object? value = prop[colIndex].GetValue(logbook.flights[i]);
                        Label cellLabel = new()
                        {
                            Text = value is byte[] b ? string.Join(", ",b) : value?.ToString() ?? "",
                            Margin = new Thickness(0,5,0,5),
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };
                        FetcherGrid.Add(cellLabel,colIndex,i+3);
                    }
                }
            }
            FetchStatus.Text = "Finished";
        }
    }
}
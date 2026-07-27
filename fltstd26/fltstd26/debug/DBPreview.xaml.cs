using fltstd26.system;
using fltstd26.core;
using System.Reflection;
using fltstd26.etc;

namespace fltstd26.debug;

public partial class DBPreview : Window
{
    private readonly Button[] Tabs;
    private int SelectedTab = 0;
    private readonly List<Sheets.Flt>? offlineFLT = null;
    private readonly List<Sheets.Target>? offlineTGT = null;
    private readonly List<Sheets.Lfz>? offlineLFZ = null;
    private readonly List<Sheets.Slot>? offlineSLT = null;
    private readonly List<Sheets.PriceCat>? offlinePRC = null;
    public DBPreview(string? db = null)
    {
        InitializeComponent();
        Tabs = [FlightButton,TargetButton,SlotButton,AircraftButton,PriceButton];
        if (db != null)
        {
            bool DBOpen = RData.Active();
            RData.Close();
            RData.Init(db);
            offlineFLT = RData.GetFlightTable();
            offlineTGT = RData.GetTargetTable();
            offlineLFZ = RData.GetAircraftTable();
            offlineSLT = RData.GetSlotsTable();
            offlinePRC = RData.GetPriceTable();
            RData.Close();
            if (DBOpen) RData.Init();
        }
        UpdateSelection(0);
    }

    private void UpdateSelection(int num)
    {
        //TXT: {AppThemeBinding Light={StaticResource White}, Dark={StaticResource PrimaryDarkText}}
        //BG: {AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource PrimaryDark}}
        Tabs[SelectedTab].BackgroundColor = Colors.Transparent;
        Tabs[SelectedTab].TextColor = GSettings.DarkMode ? GSettings.GetColour("Gray500") : GSettings.GetColour("Gray800");
        Tabs[num].BackgroundColor = GSettings.DarkMode ? GSettings.GetColour("PrimaryDark") : GSettings.GetColour("Primary");
        Tabs[num].TextColor = GSettings.DarkMode ? GSettings.GetColour("PrimaryDarkText") : GSettings.GetColour("White");
        SelectedTab = num;
        Refresh_Click(null,null);
    }

    private void FlightClick(object sender,EventArgs e) => UpdateSelection(0);
    private void TargetClick(object sender,EventArgs e) => UpdateSelection(1);
    private void SlotClick(object sender,EventArgs e) => UpdateSelection(2);
    private void AircraftClick(object sender,EventArgs e) => UpdateSelection(3);
    private void PriceClick(object sender,EventArgs e) => UpdateSelection(4);

    private void Refresh_Click(object? sender,EventArgs? e)
    {
        DBPreviewGrid.Clear();
        DBPreviewGrid.RowDefinitions.Clear();
        DBPreviewGrid.ColumnDefinitions.Clear();
        switch (SelectedTab)
        {
            case 0:
                List<Sheets.Flt> fltList = offlineFLT ?? RData.GetFlightTable();
                Draw(fltList);
                break;
            case 3:
                List<Sheets.Lfz> lfzList = offlineLFZ ?? RData.GetAircraftTable();
                Draw(lfzList);
                break;
            case 2:
                List<Sheets.Slot> slotsList = offlineSLT ?? RData.GetSlotsTable();
                Draw(slotsList);
                break;
            case 1:
                List<Sheets.Target> targetList = offlineTGT ?? RData.GetTargetTable();
                Draw(targetList);
                break;
            case 4:
                List<Sheets.PriceCat> priceList = offlinePRC ?? RData.GetPriceTable();
                Draw(priceList);
                break;
            default:
                ConProc.Log("[DBPVW] Invalid Configuration",1);
                break;
        }
    }

    private void Draw<T>(List<T> flts)
    {
        try
        {
            DBPreviewGrid.RowDefinitions.Add(new RowDefinition());
            PropertyInfo[] props = flts[0]!.GetType().GetProperties();

            foreach (PropertyInfo propInfo in props)
            {
                DBPreviewGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Label lbl = new() {
                    Text = propInfo.Name,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    BackgroundColor = GSettings.DarkMode ? GSettings.GetColour("Gray900") : GSettings.GetColour("Gray300"),
                    Margin = new Thickness(0,0,0,10),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18,
                };
                DBPreviewGrid.Add(lbl,DBPreviewGrid.ColumnDefinitions.Count - 1,0);
            }
            DBPreviewGrid.ColumnDefinitions.Add(new ColumnDefinition());

            int rowIndex = 1;
            foreach (var flt in flts)
            {
                DBPreviewGrid.RowDefinitions.Add(new RowDefinition());

                for (int colIndex = 0; colIndex < props.Length; colIndex++)
                {
                    object? value = props[colIndex].GetValue(flt);
                    Label cellLabel = new()
                    {
                        Text = value is byte[] b ? string.Join(", ",b) : value?.ToString() ?? "null",
                        Margin = new Thickness(0,5,0,5),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Start
                    };
                    DBPreviewGrid.Add(cellLabel,colIndex,rowIndex);
                }
                rowIndex++;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            ConProc.Log("[DBPVW] Nothing to display",0);
        }
        catch (Exception e)
        {
            ConProc.Log($"[DBPVW] Error displaying the Database: {e.Message}",2);
        }
    }
}
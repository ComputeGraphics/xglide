using fltstd26.system;
using fltstd26.core;
using System.Reflection;

namespace fltstd26.debug;

public partial class DBPreview : Window
{
	public DBPreview()
	{
		InitializeComponent();
		

	}

	public void Refresh_Click(object sender,EventArgs e)
	{
        DBPreviewGrid.Clear();
        DBPreviewGrid.RowDefinitions.Clear();
        DBPreviewGrid.ColumnDefinitions.Clear();
        switch (DBPreviewPicker.SelectedIndex)
        {
            case 0:
                List<Sheets.Flt> fltList = RData.GetFlightTable();
                Draw(fltList);
                break;
            case 1:
                List<Sheets.Lfz> lfzList = RData.GetAircraftTable();
                Draw(lfzList);
                break;
            case 2:
                List<Sheets.Slots> slotsList = RData.GetSlotsTable();
                Draw(slotsList);
                break;
            case 3:
                List<Sheets.Target> targetList = RData.GetTargetTable();
                Draw(targetList);
                break;
            case 4:
                List<Sheets.PriceCat> priceList = RData.GetPriceTable();
                Draw(priceList);
                break;
            default:
                ConProc.Log("[DBPVW] Invalid Configuration",1);
                break;
        }
    }

    public void Draw<T>(List<T> flts)
    {
        try
        {
            DBPreviewGrid.RowDefinitions.Add(new RowDefinition());
            PropertyInfo[] props = flts[0]!.GetType().GetProperties();

            foreach (PropertyInfo propInfo in props)
            {
                DBPreviewGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Label lbl = new() { Text = propInfo.Name,VerticalOptions = LayoutOptions.Center,HorizontalOptions = LayoutOptions.Center };
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
                        Text = value?.ToString() ?? "null",
                        Margin = new Thickness(0,5,0,5),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
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
using fltstd26.core;
using fltstd26.XFly;
using System.Security.Cryptography;
using fltstd26.Resources.Texts;
namespace fltstd26.etc.online;

public partial class FBorder : ContentView
{
    public int FlightID;

    private int dbstatus;
    private readonly Color color = GSettings.DarkMode ? GSettings.GetColour("Gray900") : GSettings.GetColour("Gray100");
    private readonly Border border;
    private readonly Picker status;
    public FBorder(Sheets.Flt flt, int occupied, int cap)
    {
        /*Label s = new()
        {
            FontAttributes = FontAttributes.Bold,
            Text = GSettings.Status[flt.Status],
            HorizontalTextAlignment = TextAlignment.Center,
        };*/
        Picker s = new()
        {
            ItemsSource = GSettings.Status,
            Margin = new Thickness(20,8),
            SelectedIndex = flt.Status,
            HorizontalOptions = LayoutOptions.Fill,
        };

        Grid g = new()
        {
            ColumnDefinitions = { new ColumnDefinition(),new ColumnDefinition() },
            Margin = new Thickness(20,8),
        };
        g.Add(new Label
        {
            Text = Lang.fltno + $": {flt.EId ?? flt.Id.ToString()}",
            HorizontalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold,
            TextColor = GSettings.DarkMode ? Colors.White : Colors.Black,
        });
        g.Add(new Label
        {
            Text = $"{occupied}/{cap} {Lang.xplan_weight}",
            HorizontalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold,
            TextColor = GSettings.DarkMode ? Colors.White : Colors.Black,
        },1);

        s.SelectedIndexChanged += SelectedIndexChanged;
        Border b = new()
        {
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = color,
            StrokeThickness = 0,
            Content = new VerticalStackLayout
            {
                Children = {
                    g,s
                }
            }
        };
        Content = b;
        border = b;
        status = s;
        FlightID = flt.Id;
        dbstatus = flt.Status;
        UpdateStatus(flt.Status);
        /*VerticalStackLayout vsl = [];
        FBorder outer = new()
        {
            FltId = ID,
            StrokeThickness = 0,
            Content = vsl
        };
        VerticalStackLayout inner = [];
        Label fltLabel = new()
        {
            Text = EID ?? ID.ToString(),
            HorizontalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold,
            TextColor = GSettings.DarkMode ? Colors.Black : Colors.White,
        };
        Border fltTextFrame = new()
        {
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = GSettings.DarkMode ? Colors.White : Colors.Black,
            StrokeThickness = 0,
            Content = fltLabel
        };
        vsl.Add(fltTextFrame);
        vsl.Add(inner);*/
    }

    public void UpdateStatus(int code)
    {
        status.SelectedIndex = code;
        border.BackgroundColor = (code > 8 && code < 13) || code == 3 ? GSettings.RedStatusColour : (code < 7 && code > 3 ? GSettings.ActiveStatusColour : (code == 2 ? GSettings.GreenStatusColour : color));
    }

    private void SelectedIndexChanged(object? sender,EventArgs e)
    {
        Manager.StatusChange(FlightID,dbstatus,status.SelectedIndex);
        dbstatus = status.SelectedIndex;
    }
}
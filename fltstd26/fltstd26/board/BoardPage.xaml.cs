using fltstd26.etc;
using fltstd26.system;

namespace fltstd26.board;

public partial class BoardPage : ContentPage
{
	public BoardPage()
	{
		InitializeComponent();

        system.Scheduler scheduler = new(TimeSpan.FromMinutes(1),(s,e) =>
        {
            BoardTime.Text = DateTime.Now.ToString("t");
            BoardDate.Text = DateTime.Now.ToString("d");
        },true,RoundToMinute(DateTime.Now) - DateTime.Now);

        TopIconL.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"dark.png") : Path.Combine(DskMan.IDynIcons,"light.png");
        XBoardBuilder();
    }
    //

    private void XBoardBuilder()
    {
        if(USettings.FlashingLights)
        {
            //Twice size of Flash Light Column below
            XTitles.ColumnDefinitions.Add(new ColumnDefinition());
        }
        foreach (string col in USettings.Columns[..^1].Concat(USettings.Additionals).Append(USettings.Columns.Last()))
        {
            XTitles.ColumnDefinitions.Add(new ColumnDefinition());
            Label l = new() { Text = col, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Start };
            XTitles.Add(l, XTitles.ColumnDefinitions.Count - 1);
        }
        
    }


    private static DateTime RoundToMinute(DateTime dateTime)
    {
        if (dateTime.Second == 0 && dateTime.Millisecond == 0)
            return dateTime;

        return dateTime.AddMinutes(1)
            .AddSeconds(-dateTime.Second)
            .AddMilliseconds(-dateTime.Millisecond);
    }

    public void AutoScaling()
    {
        double fullWidth = Window.Width;
        double fullHeight = Window.Height;
    }
}
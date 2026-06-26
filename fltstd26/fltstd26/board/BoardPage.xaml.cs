using fltstd26.etc;
using fltstd26.system;
using System.Linq;

namespace fltstd26.board;

public partial class BoardPage : ContentPage
{
	public BoardPage()
	{
		InitializeComponent();

        Scheduler scheduler = new(TimeSpan.FromMinutes(1),(s,e) => UpdateTime(),true,RoundToMinute(DateTime.Now) - DateTime.Now);
        TopIconL.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"ll_dark.png") : Path.Combine(DskMan.IDynIcons,"ll_light.png");
        TopIconR.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"lr_dark.png") : Path.Combine(DskMan.IDynIcons,"lr_light.png");
        BoardTitle.Text = USettings.BoardTitle;
        UpdateTime();
        XBoardBuilder();

        //6 Column


        List<Label> l = [];
        for(int i = 0; i < 6; i++)
        {
            Label a = new()
            {
                Text = "Test" + i.ToString(),
                //BackgroundColor = Colors.Red,
                TextColor = Colors.White
            };
            l.Add(a);
        }
        XBoard.Add(new BoardView([..l],[50,50,50,50,50,50],3));

    }
    //

    private void XBoardBuilder()
    {


        if (USettings.TargetOriented)
        {
            
        }
        else
        {
            if (USettings.FlashingLights)
            {
                //Twice size of Flash Light Column below
                XTitles.ColumnDefinitions.Add(new ColumnDefinition(80));
            }
            foreach (string col in USettings.Columns[..^1].Select(x => x.Item1).Concat(USettings.Additionals).Append(USettings.Columns.Last().Item1))
            {
                System.Diagnostics.Debug.WriteLine(col);
                XTitles.ColumnDefinitions.Add(new ColumnDefinition());
                Label l = new() { Text = col,FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start };
                XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
            }
        }
        
    }

    private void UpdateTime()
    {
        BoardTime.Text = DateTime.Now.ToString("t");
        BoardDate.Text = DateTime.Now.ToString("d");
    }

    private static DateTime RoundToMinute(DateTime dateTime)
    {
        if (dateTime.Second == 0 && dateTime.Millisecond == 0)
            return dateTime;

        return dateTime.AddMinutes(1)
            .AddSeconds(-dateTime.Second)
            .AddMilliseconds(-dateTime.Millisecond);
    }
}
using fltstd26.etc;
using fltstd26.system;
using System.Linq;
using System.Runtime.CompilerServices;

namespace fltstd26.board;

public partial class BoardPage : ContentPage
{
    internal static Dictionary<int,Border> TargetTags = [];
    internal static Dictionary<int,BoardView> FlightTags = [];
    private List<double> exportWidths = [];

    public BoardPage()
    {
        InitializeComponent();
        BoardController.Board = this;
        BoardController.XBoard = XBoard;
        BoardTimeServ.Init();
        Scheduler scheduler = new(TimeSpan.FromMinutes(1),(s,e) => UpdateTime(),true,TimeServ.RoundToMinute(DateTime.Now) - DateTime.Now);
        TopIconL.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"ll_dark.png") : Path.Combine(DskMan.IDynIcons,"ll_light.png");
        TopIconR.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"lr_dark.png") : Path.Combine(DskMan.IDynIcons,"lr_light.png");
        BoardTitle.Text = USettings.BoardTitle;
        //WindowWidth = w.Width;
        //ColumnSizes = [.. USettings.Columns.Select(x => GetColumnWidth(x.Item3))];
        UpdateTime();

        //6 Column




    }

    public static void Refresh()
    {
        if (!double.IsNaN(BoardController.WindowWidth))
        {
            BoardController.ColumnSizes = [.. USettings.Columns.Select(x => BoardController.GetColumnWidth(x.Item3))];
            System.Diagnostics.Debug.WriteLine("Window Width: " + BoardController.WindowWidth.ToString());
            System.Diagnostics.Debug.WriteLine("[{0}]",string.Join(", ",BoardController.ColumnSizes));
            BoardController.Board?.XBoardBuilder();
        }

    }

    private void XBoardBuilder()
    {
        XTitles.Clear();
        XTitles.ColumnDefinitions.Clear();
        XBoard.Clear();

        if (USettings.TargetOriented)
        {

        }
        else
        {
            //Twice size of Flash Light Column below
            XTitles.ColumnDefinitions.Add(new ColumnDefinition(90));

            for (int i = 0; i < USettings.Columns.Count; i++)
            {
                if (USettings.Columns[i].Item2 == "Ctr.Add")
                {
                    double addwidth = BoardController.ColumnSizes[i] / USettings.Additionals.Count;
                    for (int j = 0; j < USettings.Additionals.Count; j++)
                    {
                        XTitles.ColumnDefinitions.Add(new ColumnDefinition(addwidth));
                        Label l = new() { Text = USettings.Additionals[j],FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start };
                        XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
                    }
                }
                else
                {
                    XTitles.ColumnDefinitions.Add(new ColumnDefinition(BoardController.ColumnSizes[i]));
                    Label l = new() { Text = USettings.Columns[i].Item1,FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start };
                    XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
                }
            }




            double[] widths = [.. XTitles.ColumnDefinitions.Skip(1).Select(x => x.Width).Select(x => x.Value)];
            XBoard.Add(new BoardView(TestLabels(),widths,0));
            XBoard.Add(new BoardView(TestLabels(),widths,1));
            XBoard.Add(new BoardView(TestLabels(),widths,2));
            XBoard.Add(new BoardView(TestLabels(),widths,4));
            XBoard.Add(new BoardView(TestLabels(),widths,5));
            XBoard.Add(new BoardView(TestLabels(),widths,6));

            /*USettings.Columns.ForEach(x => cols.AddRange(x.Item2 != "Ctr.Add" ? [x.Item1] : [.. USettings.Additionals]));
            for(int i = 0; i < cols.Count && i < BoardController.ColumnSizes.Length + BoardController.AddColumnsSizes.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(cols[i]);
                XTitles.ColumnDefinitions.Add(new ColumnDefinition(BoardController.ColumnSizes[i]));
                Label l = new() { Text = cols[i],FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start };
                XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
            }*/
        }

    }

    private View[] TestLabels()
    {
        List<Label> ls = [];
        for (int i = 0; i < XTitles.ColumnDefinitions.Count - 1; i++)
        {
            Label a = new()
            {
                Text = "Test" + i.ToString(),
                //BackgroundColor = Colors.Red,
                TextColor = Colors.White
            };
            ls.Add(a);
        }
        return [.. ls];
    }

    private void UpdateTime()
    {
        BoardTime.Text = DateTime.Now.ToString("t");
        BoardDate.Text = DateTime.Now.ToString("d");
    }




}
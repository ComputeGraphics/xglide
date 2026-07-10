using fltstd26.core;
using fltstd26.etc;
using fltstd26.system;
using System.Linq;
using System.Runtime.CompilerServices;

namespace fltstd26.board;

public partial class BoardPage : ContentPage
{
    //internal static Dictionary<int,Border> TargetTags = [];
    public readonly int BoardIndex;
    private readonly List<double> exportWidths = [];
    internal Dictionary<int,BoardView> FlightTags = [];
    internal double[] ColumnSizes = [];
    internal double WindowWidth = 0;
    private int PreviousTip = 0;

    private double ScrollPosition = 0;
    public BoardPage()
    {
        InitializeComponent();
        BoardController.Boards.Add(this);
        BoardIndex = BoardController.Boards.Count - 1;
        if (BoardController.ClockID == -1)
        {
            BoardController.ClockID = TimeServ.ScheduleRO(TimeSpan.FromMinutes(1),BoardController.UpdateTime,true);
            BoardController.TickID = TimeServ.ScheduleRO(TimeSpan.FromSeconds(10),BoardController.Tick,false);
            BoardTimeServ.Init();
        }

        //Scheduler scheduler = new(TimeSpan.FromMinutes(1),(s,e) => UpdateTime(),true,TimeServ.RoundToMinute(DateTime.Now) - DateTime.Now);
        TopIconL.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"ll_dark.png") : Path.Combine(DskMan.IDynIcons,"ll_light.png");
        TopIconR.Source = GSettings.DarkMode ? Path.Combine(DskMan.IDynIcons,"lr_dark.png") : Path.Combine(DskMan.IDynIcons,"lr_light.png");
        BoardTitle.Text = USettings.BoardTitle;
        //WindowWidth = w.Width;
        //ColumnSizes = [.. USettings.Columns.Select(x => GetColumnWidth(x.Item3))];

        UpdateTime();

        //6 Column




    }

    //Alle 10S

    private async void AutoScroll()
    {
        double pages = double.Floor(XBoardScroll.ContentSize.Height / XBoardScroll.Height);

        double? singleFlightTagHeight = FlightTags.FirstOrDefault().Value.Height;
        bool heightmatch = singleFlightTagHeight * FlightTags.Count == XBoardScroll.ContentSize.Height;

        double scrollDistance = ScrollPosition * (singleFlightTagHeight.HasValue && heightmatch ? (double.Floor(XBoardScroll.Height / singleFlightTagHeight.Value) * singleFlightTagHeight.Value) : XBoardScroll.Height);

        //System.Diagnostics.Debug.WriteLine(pages.ToString() + " Pages to scroll. Scroll Distance in DIU: " + scrollDistance.ToString());
        if (pages > 1)
        {
            ScrollPosition = ScrollPosition < pages ? ScrollPosition + 1 : 0;
            //System.Diagnostics.Debug.WriteLine("Scroll to Page " + ScrollPosition.ToString());
            await XBoardScroll.ScrollToAsync(0,scrollDistance,true);

            //Scrolling notwendig
        }
    }

    private void UpdateMessageCenter()
    {
        if (USettings.MSGCenterTips.Count > 0)
        {
            int post = 0;
            for (int a = 0; a < 3 && post == PreviousTip; a++)
            {
                post = new Random().Next(0,USettings.MSGCenterTips.Count - 1);
            }

            msgcenter_icon.Source = USettings.MSGCenterTips[post].Item1 != null ? Path.Combine(DskMan.IDynIcons,USettings.MSGCenterTips[post].Item1!) : "info_big.png";
            msgcenter_title.Text = USettings.MSGCenterTips[post].Item2 ?? USettings.MSGCenterDefaultTitle;
            msgcenter_text.Text = USettings.MSGCenterTips[post].Item3;
            PreviousTip = post;
        }
    }
    internal void BoardCylce()
    {
        AutoScroll();
        UpdateMessageCenter();
    }

    internal void UpdateContent()
    {
        XBoard.Clear();
        foreach (var item in USettings.SortByTime ? FlightTags.Select(x => x.Value).OrderBy(x => x.StartTime) : FlightTags.Select(x => x.Value))
        {
            XBoard.Add(item);
        }
        /*for (int i = 0; i < 30; i++)
        {
            BoardView bvtest = TestLabels(i);
            FlightTags.Add(i + 99,bvtest);
            XBoard.Add(bvtest);
        }*/
    }

    internal void UpdateStatus(List<Sheets.Flt> flts)
    {
        foreach (var flt in flts)
        {
            if (FlightTags.TryGetValue(flt.Id,out BoardView? bv) && bv != null)
            {
                // 0 - Neutral, 1 - Green, 2 - Red
                byte FL = 0;
                int status = GSettings.StatusLink.TryGetValue(flt.Id,out int s) ? s : flt.Status;

                if (USettings.Status_Red.Contains(status)) FL = 2;
                else if (USettings.Status_Green.Contains(status)) FL = 1;
                else if (USettings.Status_RedBlink.Contains(status)) FL = 5;
                else if (USettings.Status_GreenBlink.Contains(status)) FL = 4;
                else if (USettings.Status_Switch.Contains(status)) FL = 6;

                bv.UpdateFlash(FL);
                bv.UpdateStatus(status);
            }
        }
    }

    internal void XBoardBuilder()
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
                    double addwidth = ColumnSizes[i] / USettings.Additionals.Count;
                    for (int j = 0; j < USettings.Additionals.Count; j++)
                    {
                        XTitles.ColumnDefinitions.Add(new ColumnDefinition(addwidth));
                        Label l = new() { Text = USettings.Additionals[j],FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start,FontSize = USettings.CaptionSize };
                        XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
                    }
                }
                else
                {
                    XTitles.ColumnDefinitions.Add(new ColumnDefinition(ColumnSizes[i]));
                    Label l = new() { Text = USettings.Columns[i].Item1,FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start,FontSize = USettings.CaptionSize };
                    XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
                }
            }




            double[] widths = [.. XTitles.ColumnDefinitions.Skip(1).Select(x => x.Width).Select(x => x.Value)];
            /*XBoard.Add(new BoardView(TestLabels(),widths,0, DateTime.Now));
            XBoard.Add(new BoardView(TestLabels(),widths,1));
            XBoard.Add(new BoardView(TestLabels(),widths,2));
            XBoard.Add(new BoardView(TestLabels(),widths,4));
            XBoard.Add(new BoardView(TestLabels(),widths,5));
            XBoard.Add(new BoardView(TestLabels(),widths,6));*/

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

    private BoardView TestLabels(int Enum)
    {
        List<Border> ls = [];
        for (int i = 0; i < XTitles.ColumnDefinitions.Count - 1; i++)
        {
            Border b = new()
            {
                Padding = 5,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                StrokeThickness = 2,
                Content = new Label()
                {
                    Padding = 5,
                    Text = "Test " + Enum.ToString(),
                    FontSize = USettings.ElementSize,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontFamily = USettings.UseTargetSquareFont ? "SquareSans" : "ZenDots"
                }
            };
            ls.Add(b);
        }

        return new BoardView([.. ls],ColumnSizes,0,DateTime.Now);
    }

    public void UpdateTime()
    {
        BoardTime.Text = DateTime.Now.ToString("t");
        BoardDate.Text = DateTime.Now.ToString("d");
    }

    internal void Terminate()
    {
        if (XBoard == null) return;
        foreach (BoardView bw in XBoard.Children.OfType<BoardView>())
        {
            bw.TerminateFlash();
        }
    }
}
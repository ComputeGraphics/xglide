using CommunityToolkit.Maui.Behaviors;
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
    internal List<BoardView> FlightTags = [];
    internal List<BoardView> VisibleTags = [];
    internal double[] ColumnSizes = [];
    internal double AddColumns = 0;
    internal double WindowWidth = 0;
    private int PreviousTip = 0;
    internal readonly bool IsFlip;

    private double ScrollPosition = 0;
    public BoardPage(bool flip)
    {
        IsFlip = flip;
        bool dark = flip || GSettings.DarkMode;
        InitializeComponent();
        //BindingContext = USettings.Instance;
        BoardController.Boards.Add(this);
        BoardIndex = BoardController.Boards.Count - 1;
        BoardController.Init();
        TopIconL.Source = dark ? Path.Combine(DskMan.IDynIcons,"ll_dark.png") : Path.Combine(DskMan.IDynIcons,"ll_light.png");
        TopIconR.Source = dark ? Path.Combine(DskMan.IDynIcons,"lr_dark.png") : Path.Combine(DskMan.IDynIcons,"lr_light.png");
        BackgroundColor = dark ? GSettings.GetColour("Gray800") : GSettings.GetColour("White");
        TitleBG.BackgroundColor = flip ? GSettings.GetColour("Black") : (GSettings.DarkMode ? GSettings.GetColour("SecondaryBg") : GSettings.GetColour("SecondaryDark"));
        //Scheduler scheduler = new(TimeSpan.FromMinutes(1),(s,e) => UpdateTime(),true,TimeServ.RoundToMinute(DateTime.Now) - DateTime.Now);

        BoardTitle.Text = USettings.Instance.BoardTitle;
        //WindowWidth = w.Width;
        //ColumnSizes = [.. USettings.Instance.Columns.Select(x => GetColumnWidth(x.Item3))];

        UpdateTime();

    }

    private async void AutoScroll()
    {
        if (FlightTags.Count == 0) return;
        double pages = double.Floor(XBoardScroll.ContentSize.Height / XBoardScroll.Height);

        double? singleFlightTagHeight = FlightTags.FirstOrDefault()?.Height;
        bool heightmatch = singleFlightTagHeight * FlightTags.Count == XBoardScroll.ContentSize.Height;

        double scrollDistance = ScrollPosition * (singleFlightTagHeight.HasValue && heightmatch ? (double.Floor(XBoardScroll.Height / singleFlightTagHeight.Value) * singleFlightTagHeight.Value) : XBoardScroll.Height);

        System.Diagnostics.Debug.WriteLine(pages.ToString() + " Pages to scroll. Scroll Distance in DIU: " + scrollDistance.ToString());
        if (pages > 0)
        {
            ScrollPosition = ScrollPosition < pages ? ScrollPosition + 1 : 0;
            System.Diagnostics.Debug.WriteLine("Scroll to Page " + ScrollPosition.ToString());
            await XBoardScroll.ScrollToAsync(0,scrollDistance,!IsFlip);

            //Scrolling notwendig
        }
    }

    private void UpdateMessageCenter()
    {
        if (USettings.Instance.MSGCenterTips.Count > 0)
        {
            int a = 0;
            int post;
            do
            {
                post = new Random().Next(0,USettings.Instance.MSGCenterTips.Count);
                a++;
            }
            while (post == PreviousTip && a < 3);

            msgcenter_icon.Source = USettings.Instance.MSGCenterTips[post].Item1 != null ? Path.Combine(DskMan.IDynIcons,USettings.Instance.MSGCenterTips[post].Item1!) : "info_big.png";
            msgcenter_title.Text = USettings.Instance.MSGCenterTips[post].Item2 ?? USettings.Instance.MSGCenterDefaultTitle;
            msgcenter_text.Text = USettings.Instance.MSGCenterTips[post].Item3;
            PreviousTip = post;
        }
    }
    internal void BoardCylce()
    {
        AutoScroll();
        UpdateMessageCenter();
    }

    internal void PushNotification(string icon, string title, string subtitle, Color background, Color foreground, bool tint)
    {
        msgcenter_bg.BackgroundColor = background;
        msgcenter_icon.Source = icon;
        msgcenter_title.Text = title;
        msgcenter_text.Text = subtitle;
        msgcenter_text.TextColor = foreground;
        msgcenter_title.TextColor = foreground;
        if (tint)
        {
            msgcenter_icon.Behaviors.Clear();
            IconTintColorBehavior t = new()
            {
                TintColor = foreground,
            };
            msgcenter_icon.Behaviors.Add(t);
        }
    }

    internal void FreeMessageCenter()
    {
        msgcenter_bg.BackgroundColor = GSettings.DarkMode ? GSettings.GetColour("Gray050") : GSettings.GetColour("Gray800");
        msgcenter_title.TextColor = GSettings.DarkMode ? GSettings.GetColour("Black") : GSettings.GetColour("White");
        msgcenter_text.TextColor = GSettings.DarkMode ? GSettings.GetColour("Black") : GSettings.GetColour("White");
        msgcenter_icon.Behaviors.Clear();
    }

    internal void UpdateContent()
    {
        XBoard.Clear();
        /*FlipChar fc = new(USettings.Oberservables.elementSize);
        XBoard.Add(fc);*/
        foreach (var item in USettings.Instance.SortByTime ? FlightTags.OrderBy(x => x.StartTime) : FlightTags.AsEnumerable())
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

    internal void AddView(BoardView bv)
    {
        //System.Diagnostics.Debug.WriteLine("Adding " + bv.FlightID.ToString() + " to the views");
        
        if (USettings.Instance.SortByTime)
        {
            FlightTags = [.. FlightTags.OrderBy(x => x.StartTime)];
            XBoard.Insert(FlightTags.IndexOf(bv),bv);
        }
        else { XBoard.Add(bv); }
        FlightTags.Add(bv);
    }

    internal void RemoveView(int flightId)
    {
        BoardView? bv = FlightTags.Find(x => x.FlightID == flightId);
        if(bv != null)
        {
            FlightTags.Remove(bv);
            XBoard.Remove(bv);
        }
    }

    internal void UpdateStatus(List<Sheets.Flt> flts)
    {
        foreach (var flt in flts)
        {
            BoardView? bv = FlightTags.Find(x => x.FlightID == flt.Id);
            if (bv != null)
            {
                // 0 - Neutral, 1 - Green, 2 - Red
                byte FL = 0;
                int status = GSettings.StatusLink.TryGetValue(flt.Id,out int s) ? s : flt.Status;

                if (USettings.Instance.Status_Red.Contains(status)) FL = 2;
                else if (USettings.Instance.Status_Green.Contains(status)) FL = 1;
                else if (USettings.Instance.Status_RedBlink.Contains(status)) FL = 5;
                else if (USettings.Instance.Status_GreenBlink.Contains(status)) FL = 4;
                else if (USettings.Instance.Status_Switch.Contains(status)) FL = 6;

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

        if (USettings.Instance.TargetOriented)
        {

        }
        else
        {
            //Twice size of Flash Light Column below
            XTitles.ColumnDefinitions.Add(new ColumnDefinition(90));

            for (int i = 0; i < USettings.Instance.Columns.Count; i++)
            {
                if (USettings.Instance.Columns[i].Link == "Ctr.Add")
                {
                    AddColumns = ColumnSizes[i] / USettings.Instance.Additionals.Count;
                    for (int j = 0; j < USettings.Instance.Additionals.Count; j++)
                    {
                        XTitles.ColumnDefinitions.Add(new ColumnDefinition(AddColumns));
                        Label l = new() { Text = USettings.Instance.Additionals[j],FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start,FontSize = USettings.Instance.CaptionSize,LineBreakMode = LineBreakMode.NoWrap };
                        XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
                    }
                }
                else
                {
                    XTitles.ColumnDefinitions.Add(new ColumnDefinition(ColumnSizes[i]));
                    Label l = new() { Text = USettings.Instance.Columns[i].Name,FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start,FontSize = USettings.Instance.CaptionSize,LineBreakMode = LineBreakMode.NoWrap };
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

            /*USettings.Instance.Columns.ForEach(x => cols.AddRange(x.Item2 != "Ctr.Add" ? [x.Item1] : [.. USettings.Instance.Additionals]));
            for(int i = 0; i < cols.Count && i < BoardController.ColumnSizes.Length + BoardController.AddColumnsSizes.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(cols[i]);
                XTitles.ColumnDefinitions.Add(new ColumnDefinition(BoardController.ColumnSizes[i]));
                Label l = new() { Text = cols[i],FontAttributes = FontAttributes.Bold,HorizontalOptions = LayoutOptions.Start };
                XTitles.Add(l,XTitles.ColumnDefinitions.Count - 1);
            }*/
        }

    }

    /*private BoardView TestLabels(int Enum)
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
                    FontSize = USettings.Instance.ElementSize,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    FontFamily = USettings.Instance.UseTargetSquareFont ? "SquareSans" : "ZenDots"
                }
            };
            ls.Add(b);
        }

        return new BoardView([.. ls],ColumnSizes,0,DateTime.Now);
    }*/

    public void UpdateTime()
    {
        BoardTime.Text = DateTime.Now.ToString("t");
        BoardDate.Text = DateTime.Now.ToString("d");
    }

    internal void Terminate()
    {
        if (XBoard == null) return;
        ClearFlightTags();
    }

    internal void ClearFlightTags()
    {
        foreach (BoardView bw in XBoard.Children.OfType<BoardView>())
        {
            bw.TerminateFlash();
        }
        XBoard.Clear();
        FlightTags.Clear();
    }

    internal void CloseWindow()
    {
        Application.Current?.CloseWindow(Window);
    }
}
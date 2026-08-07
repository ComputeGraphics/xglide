using fltstd26.core;
using fltstd26.etc;
using fltstd26.system;
using System;
using System.Reflection;

namespace fltstd26.board
{
    internal static class BoardController
    {
        internal static List<BoardPage> Boards = [];

        //internal static List<BoardPage> Board = [];
        //internal static VerticalStackLayout? XBoard = null;
        internal static int ClockID = -1;
        internal static int TickID = -1;
        internal static bool FreeCycle = true;
        //BoardController.ClockID = TimeServ.ScheduleRO(TimeSpan.FromMinutes(1),UpdateTime,true);

        //20 für Padding + 80 für FlashingLight Column + 5 Safe und Scrollbar
        public static double GetColumnWidth(BoardPage board,int percentage) => (board.WindowWidth - 115) / 100 * percentage;

        public static void Tick()
        {
            if (FreeCycle)
            {
                foreach (BoardPage page in Boards)
                {
                    page.BoardCylce();
                }
            }
        }

        public static void PushNotification(TimeSpan duration, string icon,string title,string subtitle,Color background,Color foreground,bool tint)
        {
            foreach (BoardPage page in Boards)
            {
                page.PushNotification(icon,title, subtitle, background, foreground, tint);
            }
            FreeCycle = false;
            TimeServ.Schedule(DateTime.Now.Add(duration),ReleaseNotification);
        }

        public static void ReleaseNotification()
        {
            foreach (BoardPage page in Boards)
            {
                page.FreeMessageCenter();
            }
            FreeCycle = true;
        }

        internal static void Init()
        {
            if (ClockID == -1) ClockID = TimeServ.ScheduleRO(TimeSpan.FromMinutes(1),BoardController.UpdateTime,true);
            if (TickID == -1) TickID = TimeServ.ScheduleRO(TimeSpan.FromSeconds(10),BoardController.Tick,false);
            BoardTimeServ.Init();
        }

        public static void UpdateTime()
        {
            System.Diagnostics.Debug.WriteLine("Board Time Update");
            Boards.ForEach(board => board.UpdateTime());
        }

        public static void Refresh(BoardPage Board)
        {
            if (!double.IsNaN(Board.WindowWidth))
            {
                Board.ColumnSizes = [.. USettings.Instance.Columns.Select(x => GetColumnWidth(Board,x.Width))];
                System.Diagnostics.Debug.WriteLine("Window Width: " + Board.WindowWidth.ToString());
                System.Diagnostics.Debug.WriteLine("[{0}]",string.Join(", ",Board.ColumnSizes));
                Board.ClearFlightTags();
                Board.XBoardBuilder();
            }
        }

        internal static void Terminate(BoardPage Board)
        {
            Board.Terminate();
            Boards.Remove(Board);
            if (Boards.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("Ending FIDS Session");
                TimeServ.UnscheduleRO(ClockID);
                TimeServ.UnscheduleRO(TickID);
                ClockID = -1;
                TickID = -1;
                BoardTimeServ.Pause(true);
            }
        }

        internal static void Close(int BoardNumber)
        {
            if (BoardNumber != -1 && BoardNumber < Boards.Count)
            {
                Boards[BoardNumber].CloseWindow();
            }
        }

        internal static void StopClock() => TimeServ.UnscheduleRO(ClockID);

        internal static void SynchronizeWithFlight(List<Sheets.Flt> flts,List<Sheets.Target> tgts)
        {
            List<Sheets.Slot> slots = RData.GetSlotsTable();
            List<Sheets.Lfz> acs = RData.GetAircraftTable();
            foreach (var board in Boards)
            {
                if (board.IsFlip) FlipBoardController.SyncBoard(board,flts,tgts,slots,acs);
                else SyncBoard(board,flts,tgts,slots,acs);
            }
        }

        internal static void SyncBoard(BoardPage board,List<Sheets.Flt>? fltsn,List<Sheets.Target>? tgtsn,List<Sheets.Slot>? slotsn,List<Sheets.Lfz>? acsn)
        {
            try
            {
                board.ClearFlightTags();
                List<Sheets.Slot> slots = slotsn ?? RData.GetSlotsTable();

                List<Sheets.Lfz> acs = acsn ?? RData.GetAircraftTable();
                List<Sheets.Flt> flts = fltsn ?? RData.GetFlightTable();

                List<Sheets.Target> tgts = tgtsn ?? RData.GetTargetTable();

                if (USettings.Instance.HideInactiveFlights != -1)
                {
                    flts = [.. flts.Where(x => slots.Select(x => x.Id).Contains(x.Slot))];
                    slots = [.. slots.Where(x => !(x.FTime + TimeSpan.FromMinutes(USettings.Instance.HideInactiveFlights) < DateTime.Now))];
                }
                if (USettings.Instance.TargetOriented)
                {

                }
                else
                {
                    foreach (Sheets.Flt flt in flts)
                    {
                        BoardView? bv = Translate(flt,acs.Find(x => x.Id == flt.Lfz),slots.Find(x => x.Id == flt.Slot),tgts.Where(x => x.LId == flt.Id),board.ColumnSizes);
                        if (bv == null) continue;
                        board.FlightTags.Add(bv);
                    }
                    board.UpdateContent();
                    board.UpdateStatus(flts);
                    //Status Update
                }
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBOARD-CTR] Board Sync failed: " + ex.Message);
            }
        }
        internal static void SynchronizeWithStatus(List<Sheets.Flt> flts)
        {
            System.Diagnostics.Debug.WriteLine("Synchronizing Boards with Status change");
            foreach (var board in Boards)
            {
                board.UpdateStatus(flts);
            }
        }

        internal static BoardView? Translate(Sheets.Flt flt,Sheets.Lfz? ac,Sheets.Slot? slot,IEnumerable<Sheets.Target> tgts,double[] columns)
        {
            if (ac == null || slot == null) return null;
            List<View> Columns = [];
            foreach (BoardColumn column in USettings.Instance.Columns)
            {
                int substringIndex = column.Link.IndexOf('.');
                string dir = column.Link[..substringIndex];
                if (dir != "Ctr")
                {
                    object pass = dir == "Flt" ? flt : (dir == "Lfz" ? ac : slot);
                    Columns.Add(GetInfo(column.Link[(substringIndex + 1)..],pass,pass.GetType()));
                }
                else
                {
                    Columns.AddRange(GetCtr(column.Link[(substringIndex + 1)..],flt,tgts));
                }
            }
            return new(flt.Id,[.. Columns],columns,USettings.Instance.Columns.FindIndex(x => x.Link == "Ctr.Add"),0,slot.STime);
        }

        private static Label GetInfo(string prop,object obj,Type type,bool time = false)
        {
            Label lbl = new()
            {
                FontSize = USettings.Instance.ElementSize,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                FontFamily = "ZenDots",
                Text = "N/A"
            };
            try
            {
                if (time) lbl.FontFamily = "SquareSans";
                lbl.Text = ReflectInfo(prop,obj,type);
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBRD-CTR] Fehler: " + ex.Message,2);
            }
            return lbl;
        }

        public static string? ReflectInfo(string prop,object obj,Type type)
        {
            object? res = GetProp(prop,obj,type);
            for (int attempts = 0; attempts < 4 && res == null; attempts++)
            {
                if (GSettings.FallbackBoardProps.TryGetValue(prop,out string? n) && n != null) res = GetProp(n,obj,type);
                else if (attempts == 3) res = "N/A";
            }
            if (res is DateTime s)
            {
                res = s.ToShortTimeString();
            }
            return res!.ToString();
        }

        public static object? GetProp(string prop,object obj,Type type)
        {
            try
            {
                PropertyInfo? p = type.GetProperty(prop) ?? throw new Exception($"Eigenschaft {prop} in {type.Name} nicht gefunden");
                return p.GetValue(obj);
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBRD-CTR] Fehler: " + ex.Message,2);
                return null;
            }
        }

        // Views are the cells and byte the code for the flashing lights
        private static List<View> GetCtr(string cat,Sheets.Flt obj,IEnumerable<Sheets.Target> tgts)
        {
            List<View> views = [];
            switch (cat)
            {
                case "Add":
                    if (obj.Add == null) break;
                    string[] adds = obj.Add.Split(';');
                    for (int i = 0; i < USettings.Instance.Additionals.Count; i++)
                    {
                        views.Add(new Label()
                        {
                            VerticalOptions = LayoutOptions.Center,
                            LineBreakMode = LineBreakMode.NoWrap,
                            FontSize = USettings.Instance.ElementSize,
                            FontFamily = "ZenDots",
                            Text = adds[i],
                        });
                    }
                    break;
                case "Status":
                    int status = obj.Status;
                    if (obj.Status == 13) status = GSettings.StatusLink.TryGetValue(obj.Id,out int ls) ? ls : 11;
                    views.Add(new Border()
                    {
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        StrokeThickness = 0,
                        //BackgroundColor = status == 2 ? Colors.ForestGreen : (status == 9 || status == 10 || status == 12 ? Colors.IndianRed : Colors.Transparent),
                        Content = new Label()
                        {
                            Text = GSettings.Status[status],
                            FontSize = USettings.Instance.ElementSize,
                            LineBreakMode = LineBreakMode.NoWrap,
                            TextTransform = TextTransform.Uppercase,
                            FontAttributes = FontAttributes.Bold,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                        }
                    });
                    break;
                case string s when s.StartsWith("Target"):
                    views.Add(GetTarget(s[(s.LastIndexOf('.') + 1)..],tgts));
                    break;
            }
            return views;
        }

        private static View GetTarget(string cat,IEnumerable<Sheets.Target> tgt)
        {
            System.Diagnostics.Debug.WriteLine($"Drawing Target Table as Cat: " + cat);
            if (tgt != null && tgt.Any())
            {
                List<Border> elements = [];
                foreach (Sheets.Target? t in tgt)
                {
                    if (t != null)
                    {
                        Border b = new()
                        {
                            Padding = 5,
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill,
                            StrokeThickness = USettings.Instance.TargetBorderThickness,
                            Content = new Label()
                            {
                                Padding = 5,
                                Text = t.Name,
                                LineBreakMode = LineBreakMode.NoWrap,
                                FontSize = USettings.Instance.ElementSize,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Center,
                                FontFamily = USettings.Instance.UseTargetSquareFont ? "SquareSans" : "ZenDots"
                            }
                        };
                        elements.Add(b);
                    }
                }

                switch (cat)
                {
                    case "VSL":
                        VerticalStackLayout vstack = [.. elements];
                        return (View)vstack;
                    case "HSL":
                        HorizontalStackLayout hstack = [.. elements];
                        return (View)hstack;
                }
            }
            return new Label() { Text = "N/A" };
        }
    }
}

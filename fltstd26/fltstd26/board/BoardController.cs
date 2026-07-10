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
        //BoardController.ClockID = TimeServ.ScheduleRO(TimeSpan.FromMinutes(1),UpdateTime,true);

        //20 für Padding + 80 für FlashingLight Column + 5 Safe und Scrollbar
        public static double GetColumnWidth(int board,int percentage) => (Boards[board].WindowWidth - 115) / 100 * percentage;

        public static void Tick()
        {
            foreach (BoardPage page in Boards)
            {
                page.BoardCylce();
            }
        }

        public static void UpdateTime()
        {
            System.Diagnostics.Debug.WriteLine("Board Time Update");
            Boards.ForEach(board => board.UpdateTime());
        }

        public static void Refresh(int BoardNumber)
        {
            if (BoardNumber != -1 && BoardNumber < Boards.Count && !double.IsNaN(Boards[BoardNumber].WindowWidth))
            {
                Boards[BoardNumber].ColumnSizes = [.. USettings.Columns.Select(x => GetColumnWidth(BoardNumber,x.Item3))];
                System.Diagnostics.Debug.WriteLine("Window Width: " + Boards[BoardNumber].WindowWidth.ToString());
                System.Diagnostics.Debug.WriteLine("[{0}]",string.Join(", ",Boards[BoardNumber].ColumnSizes));
                Boards[BoardNumber].XBoardBuilder();
            }
        }

        public static void Terminate(int BoardNumber)
        {
            if (BoardNumber != -1 && BoardNumber < Boards.Count)
            {
                Boards[BoardNumber].Terminate();
                Boards.RemoveAt(BoardNumber);
            }
            if(Boards.Count == 0)
            {
                TimeServ.UnscheduleRO(ClockID);
                TimeServ.UnscheduleRO(TickID);
                ClockID = -1;
                TickID = -1;
                BoardTimeServ.Pause();
            }
        }

        internal static void StopClock() => TimeServ.UnscheduleRO(ClockID);

        internal static void SynchronizeWithFlight(List<Sheets.Flt> flts,List<Sheets.Target> tgts)
        {
            List<Sheets.Slot> slots = RData.GetSlotsTable();
            List<Sheets.Lfz> acs = RData.GetAircraftTable();
            foreach (var board in Boards)
            {
                SyncBoard(board,flts,tgts,slots,acs);
            }
        }
        internal static void SyncBoard(BoardPage board,List<Sheets.Flt>? fltsn,List<Sheets.Target>? tgtsn,List<Sheets.Slot>? slotsn,List<Sheets.Lfz>? acsn)
        {
            board.FlightTags.Clear();
            List<Sheets.Slot> slots = slotsn ?? RData.GetSlotsTable();
            
            List <Sheets.Lfz> acs = acsn ?? RData.GetAircraftTable();
            List <Sheets.Flt> flts = fltsn ?? RData.GetFlightTable();
            
            List<Sheets.Target> tgts = tgtsn ?? RData.GetTargetTable();

            if(USettings.HideInactiveFlights != -1)
            {
                flts = [.. flts.Where(x => slots.Select(x => x.Id).Contains(x.Slot))];
                slots = [.. slots.Where(x => x.FTime + TimeSpan.FromMinutes(USettings.HideInactiveFlights) < DateTime.Now)];
            }

            foreach (Sheets.Flt flt in flts)
            {
                BoardView? bv = Translate(flt,acs.Find(x => x.Id == flt.Lfz),slots.Find(x => x.Id == flt.Slot),tgts.Where(x => x.LId == flt.Id),board.ColumnSizes);
                if (bv == null) continue;
                board.FlightTags.Add(flt.Id,bv);
            }
            board.UpdateContent();
            board.UpdateStatus(flts);
            //Status Update
        }
        internal static void SyncBoardStatus(BoardPage board, List<Sheets.Flt> flts)
        {
            board.UpdateStatus(flts);
        }

        internal static void SynchronizeWithStatus(List<Sheets.Flt> flts)
        {
            foreach (var board in Boards)
            {
                SyncBoardStatus(board,flts);
            }
        }

        internal static BoardView? Translate(Sheets.Flt flt,Sheets.Lfz? ac,Sheets.Slot? slot,IEnumerable<Sheets.Target> tgts,double[] columns)
        {
            if (ac == null || slot == null) return null;
            List<View> Columns = [];
            foreach ((string, string, int) column in USettings.Columns)
            {
                int substringIndex = column.Item2.IndexOf('.');
                string dir = column.Item2[..substringIndex];
                if (dir != "Ctr")
                {
                    object pass = dir == "Flt" ? flt : (dir == "Lfz" ? ac : slot);
                    Columns.Add(GetInfo(column.Item2[(substringIndex + 1)..],pass,pass.GetType()));
                }
                else
                {
                    Columns.AddRange(GetCtr(column.Item2[(substringIndex + 1)..],flt,tgts));
                }
            }
            return new([.. Columns],columns,0,slot.STime);
        }

        private static Label GetInfo(string prop,object obj,Type type,bool time = false)
        {
            Label lbl = new()
            {
                FontSize = USettings.ElementSize,
                VerticalOptions = LayoutOptions.Center,
                FontFamily = "ZenDots",
                Text = "N/A"
            };
            try
            {
                object? res = GetProp(prop,obj,type);
                for (int attempts = 0; attempts < 4 && res == null; attempts++)
                {
                    if (GSettings.FallbackBoardProps.TryGetValue(prop,out string? n) && n != null) res = GetProp(n,obj,type);
                    else if (attempts == 3) res = "N/A";
                }
                if (res is DateTime s)
                {
                    if (time) lbl.FontFamily = "SquareSans";
                    res = s.ToShortTimeString();
                }
                lbl.Text = res!.ToString();
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBRD-CTR] Fehler: " + ex.Message,2);
            }
            return lbl;
        }

        private static object? GetProp(string prop,object obj,Type type)
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
                    for (int i = 0; i < USettings.Additionals.Count; i++)
                    {
                        views.Add(new Label()
                        {
                            VerticalOptions = LayoutOptions.Center,
                            FontSize = USettings.ElementSize,
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
                            FontSize = USettings.ElementSize,
                            TextTransform = TextTransform.Uppercase,
                            FontAttributes = FontAttributes.Bold,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                        }
                    });
                    break;
                case string s when s.StartsWith("Target"):
                    views.Add(GetTarget(s[(s.LastIndexOf('.')+1)..],tgts));
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
                            StrokeThickness = 2,
                            Content = new Label()
                            {
                                Padding = 5,
                                Text = t.Name,
                                FontSize = USettings.ElementSize,
                                VerticalOptions = LayoutOptions.Center,
                                HorizontalOptions = LayoutOptions.Center,
                                FontFamily = USettings.UseTargetSquareFont ? "SquareSans" : "ZenDots"
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

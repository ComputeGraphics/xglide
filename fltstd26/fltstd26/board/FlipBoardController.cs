using fltstd26.core;
using fltstd26.etc;
using fltstd26.system;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SQLite.TableMapping;

namespace fltstd26.board
{
    internal class FlipBoardController
    {
        private static Scheduler? _flipclock = null;

        internal static void SyncBoard(BoardPage board,List<Sheets.Flt>? fltsn,List<Sheets.Target>? tgtsn,List<Sheets.Slot>? slotsn,List<Sheets.Lfz>? acsn)
        {
            System.Diagnostics.Debug.WriteLine("Syncing Flip Board");
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
                //Deleted Flights
                IEnumerable<int> msflt = board.FlightTags.Select(x => x.FlightID).Except(flts.Select(x => x.Id));
                foreach (int id in msflt)
                {
                    board.RemoveView(id);
                }
                foreach (Sheets.Flt flt in flts)
                {
                    Sheets.Slot? slt = slots.Find(x => x.Id == flt.Slot);
                    BoardView? bv = board.FlightTags.Find(x => x.FlightID == flt.Id);
                    if (bv != null)
                    {
                        //Es gibt den Flug schon!
                        InitRefresh(flt,acs.Find(x => x.Id == flt.Lfz),slt,tgts.Where(x => x.LId == flt.Id),bv);
                    }
                    else
                    {
                        //Flug noch nicht da
                        System.Diagnostics.Debug.WriteLine("New flight");
                        BoardView? b = CreateView(flt.Id,slt?.STime,board.ColumnSizes);
                        if (b != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Adding View");
                            board.AddView(b);
                            InitRefresh(flt,acs.Find(x => x.Id == flt.Lfz),slt,tgts.Where(x => x.LId == flt.Id),b);
                        }
                    }

                }
            }
            board.UpdateStatus(flts);
            RefresherClock();
        }

        internal static void InitRefresh(Sheets.Flt flt,Sheets.Lfz? ac,Sheets.Slot? slt,IEnumerable<Sheets.Target>? tgts,BoardView bv)
        {
            if (ac == null || slt == null || tgts == null) return;
            IList<HorizontalStackLayout> cols = [.. bv.GetColumns().OfType<HorizontalStackLayout>()];
            for (int i = 0; i < cols.Count; i++)
            {
                //System.Diagnostics.Debug.WriteLine("Looping through Columns " + i.ToString() + "/" + cols.Count.ToString());
                //System.Diagnostics.Debug.WriteLine("Add Column: " + bv.AddColumn.ToString());
                if (i == bv.AddColumn && flt.Add != null)
                {
                    int addcount = USettings.Instance.Additionals.Count;
                    for (; i - bv.AddColumn < addcount; i++)
                    {
                        string[] adds = flt.Add.Split(';');
                        if (i - bv.AddColumn < adds.Length)
                        {
                            SetFlipHSLTarget(cols[i],adds[i]);
                        }
                    }
                    i--;
                }
                else
                {
                    //Process text content
                    string link = i < USettings.Instance.Columns.Count ? USettings.Instance.Columns[i].Link : "";
                    int substringIndex = USettings.Instance.Columns[i].Link.IndexOf('.');
                    string dir = link[..substringIndex];
                    if (!dir.StartsWith("Ctr"))
                    {
                        object pass = dir == "Flt" ? flt : (dir == "Lfz" ? ac : slt);
                        SetFlipHSLTarget(cols[i],BoardController.ReflectInfo(link[(substringIndex + 1)..],pass,pass.GetType()) ?? "N/A");
                    }
                    else if (link.Contains("Target"))
                    {
                        SetFlipHSLTarget(cols[i],string.Join(",",tgts.Select(x => x.Name)));
                    }
                }

            }
        }


        private static readonly List<IEnumerable<List<FlipChar>>> _chars = [];
        private static void RefresherClock()
        {
            System.Diagnostics.Debug.WriteLine("Starting Refresh Clock");
            _chars.Clear();
            foreach (BoardPage bp in BoardController.Boards.Where(x => x.IsFlip))
            {
                _chars.Add(bp.FlightTags.Select(x => x.GetColumnFlips()));
            }
            _flipclock ??= new(TimeSpan.FromMilliseconds(USettings.Instance.FlipCycleSpeed),RefresherTick,true,null,false);
            _flipclock.Start();
        }

        private static void RefresherTick(object? sender,EventArgs e)
        {
            bool remaining = false;
            for (int i = 0; i < _chars.Count; i++)
            {
                foreach (List<FlipChar> lc in _chars[i])
                {
                    //lc.ForEach(x => x.UpdateLetter());
                    foreach (FlipChar c in lc.Where(x => !x.AtTarget))
                    {
                        //System.Diagnostics.Debug.WriteLine("Child Type: " + c.GetType().Name);
                        c.UpdateLetter();
                        remaining = true;
                    }
                    //System.Diagnostics.Debug.WriteLine("Refreshing Char");
                }
            }


            /*foreach (BoardPage bp in BoardController.Boards.Where(x => x.IsFlip))
            {
                FlipChar? hsl = bp.FlightTags.Select(x => x.GetColumnFlips()).First().First();
                System.Diagnostics.Debug.WriteLine("Column Type: " + hsl.Get);
                
                IEnumerable<IList<IView>> iv = bp.FlightTags.Select(x => x.GetColumns());
                foreach (var item in iv)
                {
                    foreach (var item1 in item)
                    {
                        System.Diagnostics.Debug.WriteLine("Column Type: " + item1.GetType().Name);
                    }
                }
                IEnumerable<IList<IView>> ch = iv.OfType<HorizontalStackLayout>();
                System.Diagnostics.Debug.WriteLine("Child Count: " + ch.Count());
                foreach (var item in ch)
                {
                    System.Diagnostics.Debug.WriteLine("Child Upper Type: " + item.GetType().Name);
                    foreach (var item1 in item)
                    {
                        System.Diagnostics.Debug.WriteLine("Child Type: " + item1.GetType().Name);
                    }
                }
                IEnumerable<IEnumerable<FlipChar>> chars = bp.FlightTags.Select(x => x.GetColumnFlips());

        }*/
            if (!remaining)
            {
                System.Diagnostics.Debug.WriteLine("Refresh Clock ended");
                _flipclock?.Pause();
            }
        }

        private static void SetFlipHSLTarget(HorizontalStackLayout hsl,string txti)
        {
            //System.Diagnostics.Debug.WriteLine("Prev Text: " + txti.ToUpper());
            string txt = new([.. from c1 in txti.ToUpperInvariant().ToCharArray()
                                 join c2 in USettings.Instance.Alphabet.ToCharArray() on c1 equals c2
                                 select c1]);

            IList<FlipChar> chars = [.. hsl.OfType<FlipChar>()];
            //System.Diagnostics.Debug.WriteLine("Proc Text: " + txt);

            //string lx = "";
            for (int i = 0; i < chars.Count; i++)
            {
                chars[i].SetTarget(i < txt.Length ? txt[i] : ' ');
                //lx += USettings.Instance.Alphabet[x];
            }

        }

        internal static BoardView? CreateView(int id,DateTime? stime,double[] columns)
        {
            if (stime == null) return null;
            List<View> Columns = [];
            foreach (BoardColumn column in USettings.Instance.Columns)
            {
                int substringIndex = column.Link.IndexOf('.');
                string dir = column.Link[..substringIndex];
                if (dir != "Ctr")
                {
                    Columns.Add(GetFlipText(column.MaxChars));
                }
                else
                {
                    Columns.AddRange(InitCtr(column.Link[(substringIndex + 1)..],column.MaxChars));
                }
            }
            return new(id,[.. Columns],columns,USettings.Instance.Columns.FindIndex(x => x.Link == "Ctr.Add"),0,stime ?? DateTime.Now);
        }

        // Views are the cells and byte the code for the flashing lights
        private static List<View> InitCtr(string cat,int charcount)
        {
            List<View> views = [];
            switch (cat)
            {
                case "Add":
                    int c = USettings.Instance.Additionals.Count;
                    int cc = charcount / c;
                    for (int i = 0; i < c; i++)
                    {
                        views.Add(GetFlipText(cc));
                    }
                    break;
                case "Status":
                    views.Add(new Border()
                    {
                        VerticalOptions = LayoutOptions.Fill,
                        HorizontalOptions = LayoutOptions.Fill,
                        StrokeThickness = 0,
                        //BackgroundColor = status == 2 ? Colors.ForestGreen : (status == 9 || status == 10 || status == 12 ? Colors.IndianRed : Colors.Transparent),
                        Content = new Label()
                        {
                            Text = GSettings.Status[11],
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
                    views.Add(GetFlipText(charcount));
                    break;
            }
            return views;
        }


        private static HorizontalStackLayout GetFlipText(int charcount)
        {
            HorizontalStackLayout hsl = new()
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 2,
            };
            for (int i = 0; i < charcount; i++)
            {
                hsl.Add(new FlipChar(USettings.Oberservables.elementSize));
            }
            return hsl;
        }
    }
}

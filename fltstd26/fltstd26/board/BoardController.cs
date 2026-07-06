using fltstd26.core;
using fltstd26.etc;
using fltstd26.system;
using System.Reflection;

namespace fltstd26.board
{
    internal static class BoardController
    {
        internal static BoardPage? Board = null;
        internal static VerticalStackLayout? XBoard = null;

        internal static double[] ColumnSizes = [];
        internal static double WindowWidth = 0;
        //20 für Padding + 80 für FlashingLight Column
        public static double GetColumnWidth(int percentage) => (WindowWidth - 100) / 100 * percentage;

        internal static void Terminate()
        {
            if (XBoard == null) return;
            foreach (BoardView bw in XBoard.Children.OfType<BoardView>())
            {
                bw.TerminateFlash();
            }
        }

        internal static void Translate(Sheets.Flt obj)
        {
            foreach ((string, string, int) column in USettings.Columns)
            {

            }
        }

        private static Label GetInfo<T>(string prop,T obj)
        {
            Label lbl = new()
            {
                FontSize = USettings.ElementSize,
                FontFamily = "ZenDots"
            };
            try
            {
                object? res = GetProp(prop,obj);
                for (int attempts = 0; attempts < 4 && res == null; attempts++)
                {
                    if (GSettings.FallbackBoardProps.TryGetValue(prop,out string? n) && n != null) res = n;
                    else if (attempts == 3) res = "N/A";
                }
                lbl.Text = res!.ToString();
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBRD-CTR] Fehler: " + ex.Message,2);
            }
            return lbl;
        }

        private static object? GetProp<T>(string prop,T obj)
        {
            try
            {
                PropertyInfo? p = typeof(T).GetProperty(prop) ?? throw new Exception("Eigenschaft nicht gefunden");
                return p.GetValue(obj);
            }
            catch (Exception ex)
            {
                ConProc.Log("[XBRD-CTR] Fehler: " + ex.Message,2);
                return null;
            }
        }

        // Views are the cells and byte the code for the flashing lights
        private static List<View> GetCtr(string cat,Sheets.Flt obj)
        {
            List<View> views = [];
            switch (cat)
            {
                case "Add":
                    if (obj.Add == null) break;
                    foreach (string add in obj.Add.Split(';'))
                    {
                        views.Add(new Label()
                        {
                            FontSize = USettings.ElementSize,
                            Text = add
                        });
                    }
                    break;
                case "Status":
                    int status = obj.Status;
                    if (obj.Status == 13) status = GSettings.StatusLink.TryGetValue(obj.Id,out int ls) ? ls : 11;
                    views.Add(new Border()
                    {
                        BackgroundColor = status == 2 ? Colors.ForestGreen : (status == 9 || status == 10 || status == 12 ? Colors.IndianRed : Colors.Transparent),
                        Content = new Label()
                        {
                            Text = GSettings.Status[status],
                            HorizontalOptions = LayoutOptions.Center,
                        }
                    });
                    break;
                case string s when s.StartsWith("Target"):
                    views.Add(GetTarget(s[s.LastIndexOf('.')..] + 1,obj));
                    break;
            }
            return views;
        }

        private static View GetTarget(string cat,Sheets.Flt obj)
        {
            List<Sheets.Target?>? tgt = RData.GetWhere<Sheets.Target>($"lid = {obj.Id}");
            if (tgt != null && tgt.Count > 0)
            {
                List<Border> elements = [];
                foreach (Sheets.Target? t in tgt)
                {
                    if (t != null)
                    {
                        Border b = new()
                        {
                            Padding = 4,
                            HorizontalOptions = LayoutOptions.Fill,
                            StrokeThickness = 2,
                            Content = new Label()
                            {
                                Text = t.Name,
                                FontSize = USettings.ElementSize,
                                FontFamily = USettings.UseTargetSquareFont ? "SquareSans" : "ZenDots"
                            }
                        };
                        BoardPage.TargetTags.TryAdd(t.Id,b);
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

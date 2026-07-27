/*  y        Lang.xplan_status_ontime, //GREEN
    y        Lang.xplan_status_waiting, //BOTH
    y        Lang.xplan_status_ready, //GREEN BLINK
    y        Lang.xplan_status_delayed, //RED BLINK
            Lang.xplan_status_dpt,
            Lang.xplan_status_airborne,
            Lang.xplan_status_app,
            Lang.xplan_status_finished,
            Lang.xplan_status_fuel,
            Lang.xplan_status_outofservice, //RED
            Lang.xplan_status_unavail, //RED
            Lang.xplan_status_unclear, //BOTH BLINK
            Lang.xplan_status_cancel,
            AUTO

-1
*/

using fltstd26.core;
using fltstd26.Resources.Texts;

namespace fltstd26.etc
{
    class GSettings
    {
        internal static Dictionary<string,string> FallbackBoardProps = new()
        {
            { "EId" , "Id" },
            { "Name", "Id" },
            { "Reg", "Type" },
            { "Type", "Id" },
            { "Id", "OGN" },
            { "OGN", "Id" }
        };


        internal static INavigation? nav;
        
        // 13 for AUTO
        internal static string[] Status = [
            Lang.xplan_status_ontime, //GREEN                       // 0 x
            Lang.xplan_status_waiting,                              // 1
            Lang.xplan_status_ready, //GREEN BLINK (BG Green)       // 2 x
            Lang.xplan_status_delayed, //RED BLINK (BG Red)         // 3 x
            Lang.xplan_status_dpt, //SWITCH BLINK                   // 4 x
            Lang.xplan_status_airborne, //SWITCH BLINK              // 5 x
            Lang.xplan_status_app, //GREEN                          // 6 x
            Lang.xplan_status_finished,                             // 7
            Lang.xplan_status_fuel,                                 // 8
            Lang.xplan_status_outofservice, //RED (BG Red)          // 9 
            Lang.xplan_status_unavail, //RED (BG Red)               //10 
            Lang.xplan_status_unclear, //RED BLINK                  //11 
            Lang.xplan_status_cancel, //RED (BG Red)                //12 
            "AUTO"
        ];

        internal static Type[] NecessaryResetTypes = [typeof(Sheets.Lfz),typeof(Sheets.PriceCat),typeof(Sheets.Slot)];
        internal static Dictionary<int,int> StatusLink = [];

        internal static readonly ImageSource[] TargetAttribIcons = ["quick.png","pin.png","notify.png","flag.png"];

        public static bool XConsoleOpen = false;

        public static Dictionary<string,string> Paths = [];

        public static Func<string,int> FormatPrice = price => Single.TryParse(price.Trim(),System.Globalization.NumberStyles.Currency,System.Globalization.CultureInfo.CurrentCulture,out Single pc) ? (int)(pc * 100) : -1;
        public static Func<int,string> UnformatPrice = price => ((double)price / 100).ToString("C",System.Globalization.CultureInfo.CurrentCulture);
        public static Func<string,int> InterpretePrice = price => (price.Contains(',') || price.Contains('.')) ? FormatPrice(price) : (Int32.TryParse(price,out int ParsePrice) ? ParsePrice*100 : -USettings.Instance.FallbackPriceCat);
        public static Func<string?,bool,bool> GetBoolean = static (value,fallback) => value != null ? !(value.Trim().Equals("false",StringComparison.OrdinalIgnoreCase) || (!value.Trim().Equals("true",StringComparison.OrdinalIgnoreCase) && !fallback)) : fallback;

        public static bool DateChanged(DateTime? prev,DateTime? now) => now != null && (prev == null || prev.Value.Date != now.Value.Date);
        public static bool TimeChanged(TimeSpan? prev,TimeSpan? now) => now != null && (prev == null || prev.Value != now.Value);
        public static Func<string?,string?,bool> ValueChanged => (prev,aft) => !string.IsNullOrEmpty(aft) && (prev == null || prev.Trim() != aft.Trim());

        public static bool DarkMode = Application.Current!.RequestedTheme == AppTheme.Dark;
        public static Func<string,Color> GetColour = c => (Color)Application.Current!.Resources[c];
        public static Color InactiveIcon = DarkMode ? GSettings.GetColour("Gray500") : GSettings.GetColour("Gray400");
        public static Color PrimaryColour = DarkMode ? GSettings.GetColour("SecondaryDark") : GSettings.GetColour("Primary");
        public static Color NodeForegroundColour = DarkMode ? GSettings.GetColour("Gray100") : GSettings.GetColour("Gray900");
        public static Color NodeBackgroundColour = DarkMode ? GSettings.GetColour("Gray950") : GSettings.GetColour("Gray200");

        //public static Color CellBackgroundNeutralColour = DarkMode ? GSettings.GetColour("OffBlack") : GSettings.GetColour("White");
        public static Color CellBackgroundNeutralColour = Colors.Transparent;
        public static Color CellBackgroundActiveColour = DarkMode ? GSettings.GetColour("SecondaryDarkBg") : GSettings.GetColour("SecondaryDarkText");
        public static Color CellBackgroundPassedColour = DarkMode ? GSettings.GetColour("Gray800") : GSettings.GetColour("Gray300");

        public static Color RedStatusColour = DarkMode ? Color.FromArgb("#FF5E1111") : Color.FromArgb("#FFF26B6B");
        public static Color GreenStatusColour = DarkMode ? Color.FromArgb("#FF204F27") : Color.FromArgb("#FF70FF8B");
        public static Color ActiveStatusColour = DarkMode ? GetColour("PrimaryBg") : GetColour("SecondaryDark");
    }
}
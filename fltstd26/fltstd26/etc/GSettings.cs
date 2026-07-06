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
            Lang.xplan_status_ontime, //GREEN
            Lang.xplan_status_waiting,
            Lang.xplan_status_ready, //GREEN BLINK (BG Green)
            Lang.xplan_status_delayed, //RED BLINK (BG Red)
            Lang.xplan_status_dpt,
            Lang.xplan_status_airborne, //SWITCH BLINK
            Lang.xplan_status_app,
            Lang.xplan_status_finished,
            Lang.xplan_status_fuel,
            Lang.xplan_status_outofservice, //RED (BG Red)
            Lang.xplan_status_unavail, //RED (BG Red)
            Lang.xplan_status_unclear, //RED BLINK
            Lang.xplan_status_cancel, //RED (BG Red)
            "AUTO"
        ];
        internal static Dictionary<int,int> StatusLink = [];

        internal static readonly ImageSource[] TargetAttribIcons = ["quick.png","pin.png","notify.png","flag.png"];

        public static bool XConsoleOpen = false;

        public static Dictionary<string,string> Paths = [];

        public static Func<string,int> FormatPrice = price => Single.TryParse(price.Trim(),System.Globalization.NumberStyles.Currency,System.Globalization.CultureInfo.CurrentCulture,out Single pc) ? (int)(pc * 100) : -1;
        public static Func<int,string> UnformatPrice = price => (price / 100).ToString("C",System.Globalization.CultureInfo.CurrentCulture);
        public static Func<string,int> InterpretePrice = price => (price.Contains(',') || price.Contains('.')) ? FormatPrice(price) : (Int32.TryParse(price,out int ParsePrice) ? ParsePrice : -USettings.FallbackPriceCat);
        public static Func<string?,bool,bool> GetBoolean = static (value,fallback) => value != null ? !(value.Trim().Equals("false",StringComparison.OrdinalIgnoreCase) || (!value.Trim().Equals("true",StringComparison.OrdinalIgnoreCase) && !fallback)) : fallback;

        public static bool DarkMode = Application.Current!.RequestedTheme == AppTheme.Dark;
        public static Func<string,Color> GetColour = c => (Color)Application.Current!.Resources[c];
        public static Color InactiveIcon = DarkMode ? GSettings.GetColour("Gray500") : GSettings.GetColour("Gray400");
        public static Color PrimaryColour = DarkMode ? GSettings.GetColour("Primary") : GSettings.GetColour("PrimaryDark");
        public static Color NodeForegroundColour = DarkMode ? GSettings.GetColour("Gray100") : GSettings.GetColour("Gray900");
        public static Color NodeBackgroundColour = DarkMode ? GSettings.GetColour("Gray950") : GSettings.GetColour("Gray200");
    
    }
}
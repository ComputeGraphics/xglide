using fltstd26.Resources.Texts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.etc
{
    class GSettings
    {
        internal static INavigation? nav;
        
        public static string[] Status = [
            Lang.xplan_status_ontime,
            Lang.xplan_status_waiting,
            Lang.xplan_status_ready,
            Lang.xplan_status_delayed,
            Lang.xplan_status_dpt,
            Lang.xplan_status_airborne,
            Lang.xplan_status_app,
            Lang.xplan_status_finished,
            Lang.xplan_status_fuel,
            Lang.xplan_status_outofservice, 
            Lang.xplan_status_unavail,
            Lang.xplan_status_unclear,
            Lang.xplan_status_cancel,
            "AUTO"
        ];
        public static ImageSource[] TargetAttribIcons = ["quick.png","pin.png","notify.png","flag.png"];

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
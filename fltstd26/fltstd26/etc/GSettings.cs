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
        public static string[] Status = [
            Lang.xplan_status_airborne,
            Lang.xplan_status_app,
            Lang.xplan_status_cancel,
            Lang.xplan_status_delayed,
            Lang.xplan_status_dpt,
            Lang.xplan_status_finished,
            Lang.xplan_status_fuel,
            Lang.xplan_status_ontime,
            Lang.xplan_status_outofservice,
            Lang.xplan_status_ready,
            Lang.xplan_status_unavail,
            Lang.xplan_status_unclear,
            Lang.xplan_status_waiting
        ];
        public static ImageSource[] TargetAttribIcons = ["quick.png","pin.png","notify.png","flag.png"];


        public static bool XConsoleOpen = false;

        public static bool AutoASAP = false; //Keine Fragen. Einfach machen
        public static bool AutoTimeCheck = false;

        public static Dictionary<string,string> Paths = [];

        public static bool DarkMode = Application.Current!.RequestedTheme == AppTheme.Dark;
        public static Func<string,Color> GetColour = (string c) => (Color)Application.Current!.Resources[c];
        public static Func<int,string> FormatPrice = (int p) => p.ToString().Insert(p.ToString().Length - 2,",");
        public static Color InactiveIcon = Application.Current!.RequestedTheme == AppTheme.Dark ? GSettings.GetColour("Gray500") : GSettings.GetColour("Gray400");
        public static Color PrimaryColour = Application.Current!.RequestedTheme == AppTheme.Dark ? GSettings.GetColour("Primary") : GSettings.GetColour("PrimaryDark");
        public static Color NodeColour = Application.Current!.RequestedTheme == AppTheme.Dark ? GSettings.GetColour("Gray100") : GSettings.GetColour("Gray900");

    }
}
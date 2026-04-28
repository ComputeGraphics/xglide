using fltstd26;
using fltstd26.Resources.Texts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace fltstd26.etc
{
    internal class USettings
    {
        internal static INavigation? nav;



        public static bool EnableSlots = true;
        public static bool AntiCol = false;
        public static int DefaultCeil = 15;
        public static int QuickTolerance = 5;
        public static bool FlashingLights = true; //IS THIS A KANYE REFERENCE?
        public static int DefaultFltLength = 15; //in min
        public static int DefaultTgtWeight = 1;
        public static List<string> Columns = ["eID","Aircraft","Target","Time","Status"];
        public static List<string> Additionals = ["Test"];
    }
}

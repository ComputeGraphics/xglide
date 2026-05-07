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

        internal static bool AutoASAP = false; //Keine Fragen. Einfach machen
        internal static bool AutoTimeCheck = false;
        internal static bool EnableSlots = true; // To Implement
        internal static bool AntiCol = false; // To Implement
        internal static int DefaultCeil = 15; // To Implement
        internal static int QuickTolerance = 5;
        internal static bool FlashingLights = true; //IS THIS A KANYE REFERENCE?
        internal static int DefaultFltLength = 15; //in min
        internal static int FallbackPriceCat = 1;
        internal static int DefaultTgtWeight = 1;
        internal static List<string> Additionals = ["Test"];

        internal static List<string> Columns = ["eID","Aircraft","Target","Time","Status"];
        
    }
}

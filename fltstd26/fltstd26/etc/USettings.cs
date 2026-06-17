using fltstd26;
using fltstd26.Resources.Texts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace fltstd26.etc
{
    /*public sealed class USettings
    {
        static USettings() { }
        private USettings() { }
        public static USettings Instance { get; } = new USettings();

        internal string Name { get; init; } = "Standard";
        internal string Creator = "Arthur Hildebrand";
        internal DateTime LastChange = new(2026,6,1,14,53,0);

        internal bool AskForNodeMove = true;
        internal bool AskForNodePriceChange = true;
        internal bool AutoASAP = false; //Keine Fragen. Einfach machen
        internal bool AutoTimeCheck = false;
        internal bool EnableSlots = true; // To Implement
        internal bool AntiCol = false; // To Implement
        internal int DefaultCeil = 15; // To Implement
        internal int QuickTolerance = 5;
        internal bool FlashingLights = true; //IS THIS A KANYE REFERENCE?
        internal int DefaultFltLength = 15; //in min
        internal int FallbackPriceCat = 1;
        internal int DefaultTgtWeight = 1;
        internal List<string> Additionals = ["Test"];

        internal List<string> Columns = ["eID","Aircraft","Target","Time","Status"];
    }*/


    public static class USettings
    {
        //Meta
        internal static string Name = "Standard";
        internal static string Creator = "Arthur Hildebrand";
        internal static DateTime LastChange = new(2026, 6, 1, 14, 53, 0);

        //General
        internal static bool AskForNodeMove = true;
        internal static bool AskForNodePriceChange = true;
        internal static bool FlashingLights = true; //IS THIS A KANYE REFERENCE?

        //Properties
        internal static List<string> Additionals = ["Test"];

        //XBOARD
        internal static List<string> Columns = ["eID","Aircraft","Target","Time","Status"];

        //XFLY
        //Manager
        internal static bool AutoASAP = false; //Keine Fragen. Einfach machen
        internal static bool AutoTimeCheck = false;
        internal static bool EnableSlots = true; // To Implement
        internal static bool AntiCol = false; // To Implement
        //Defaults
        internal static int DefaultCeil = 15; // To Implement
        internal static int QuickTolerance = 5;
        internal static int DefaultFltLength = 15; //in min
        internal static int FallbackPriceCat = 1;
        internal static int DefaultTgtWeight = 1;



        //NEU NEU NEU
        internal static bool IgnoreTransactionWeight = false;
        internal static bool IgnoreTransactionLength = false;
        internal static string Homebase = "EDFP";
 
    }
}

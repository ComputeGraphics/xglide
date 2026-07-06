using fltstd26;
using fltstd26.Resources.Texts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        //Properties
        internal static List<string> Additionals = ["Test"];

        //XBOARD
        // Name, Link, Width
        internal static List<(string, string, int)> Columns = [("eID", "Flt.EId", 10), ("Aircraft", "Lfz.Reg", 10),("Target", "Ctr.Target.VSL", 40), ("Time", "Slot.STime" , 10),("Add", "Ctr.Add",15), ("Status","Ctr.Status", 15)];

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

        //Angesetzte Minutenanzahl durch SyncLevel
        internal static int OGNSyncLevel = 1;
        internal static int OGNTolerance = 7;
        internal static int TakeoffDuration = 2; // in min

        internal static bool TargetOriented = false;
        internal static bool SortByTime = true;
        internal static bool UseTargetSquareFont = true;
        internal static string BoardTitle = "Tag der offenen Tür 2026";
        //Optimiert für FHD

        internal static short LogoSize_L = 144;
        internal static short LogoSize_R = 144;
        internal static short TitleSize = 54;
        internal static short ClockSize = 48;
        internal static short DateSize = 24;
        internal static short FlashSize = 36;
        internal static short ElementSize = 24;
        internal static short TargetBorderThickness = 2;
        
        internal static short FlashInterval = 500;
    }
}

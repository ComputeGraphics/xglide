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
        internal static DateTime LastChange = new(2026,6,1,14,53,0);

        //General
        internal static bool AskForNodeMove = true;
        internal static bool AskForNodePriceChange = true;

        //Properties
        internal static List<string> Additionals = ["Test"];

        //XBOARD
        // Name, Link, Width
        internal static List<(string, string, int)> Columns = [("eID", "Flt.EId", 10),("Aircraft", "Lfz.Reg", 10),("Target", "Ctr.Target.HSL", 40),("Time", "Slot.STime", 10),("Add", "Ctr.Add", 15),("Status", "Ctr.Status", 15)];

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


        internal static int SlotTolerance = 1;
        internal static bool HidePastSlots = false;
        //NEU NEU NEU
        internal static bool IgnoreTransactionWeight = false;
        internal static bool IgnoreTransactionLength = false;
        internal static int DelayTolerance = 5;
        internal static int MaxDelay = 30;
        internal static string Homebase = "EDFP";

        //Angesetzte Minutenanzahl durch SyncLevel
        internal static bool OGNStatus = true;
        internal static int OGNSyncLevel = 1;
        internal static int OGNTolerance = 7;
        internal static int TakeoffDuration = 2; // in min

        internal static bool TargetOriented = false; // Not implemented
        internal static bool SortByTime = true;
        internal static bool UseTargetSquareFont = false;
        internal static string BoardTitle = "Tag der offenen Tür 2026";


        internal static int[] Status_RedBlink = [3,11];
        internal static int[] Status_GreenBlink = [2];
        internal static int[] Status_Green = [0,6];
        internal static int[] Status_Red = [9,10,12];
        internal static int[] Status_Switch = [4,5];

        internal static int[] StatusBG_Green = [2];
        internal static int[] StatusBG_Red = [3,9,10,12];

        //Optimiert für FHD

        internal static short LogoSize_L = 144;
        internal static short LogoSize_R = 144;
        internal static short TitleSize = 54;
        internal static short ClockSize = 48;
        internal static short DateSize = 24;
        internal static short FlashSize = 38;
        internal static short CaptionSize = 26;
        internal static short ElementSize = 22;
        internal static short TargetBorderThickness = 2;

        internal static short MSGCenterTitleSize = 32;
        internal static short MSGCenterTextSize = 50;
        internal static short MSGCenterIconSize = 90;
        internal static string MSGCenterDefaultTitle = "Informationen vom AEC Bad Nauheim";
        //Icon (info when null), Title (default when null), Text
        internal static List<(string?, string?, string)> MSGCenterTips =
            [
            (null,null,"Schön, dass du bei uns gelandet bist!"),
            (null,null,"Jetzt im Aero-Club Bad Nauheim fliegen lernen"),
            ("plane.png",null,"Uns verbindet die Freude in der Luft zu sein"),
            (null,null,"Wir bieten für Interessierte auch Schnuppertage an!"),
            ("net.png",null,"Besuchen Sie uns doch auf www.aecbn.de im Internet"),

            ("warning.png",null,"Bitte warten Sie 10 Minuten vor Rundflugzeit im Wartebereich"),
            ("warning.png",null,"Bitte bleiben Sie im Wartebereich bis wir Sie aufrufen"),
            ("warning.png",null,"Behalten Sie die Tafel für weitere Information im Blick"),


            ("ph-braasch.png","PETER H: BRAASCH","Ihr Partner für Luftfahrzeugversicherungen"),
            ("michael-bund.png","Michael Bund Haustechnik","Ihr Fachbetrieb aus Wöllstadt für Haustechnik"),
            ("radio-frankfurt.png","RADIO FRANKFURT","Dein Radio für Frankfurt."),
            ("manitou.png","MANITOU","Handling your world"),
            ("ovag.png","OVAG","Ihr moderner Energieversorger mit langer Tradition"),
            ("imaxx.png","IMAXX","Wir finden passende Immobilien in Hessen"),
            ("additive.png","ADDITIVE","Soft- und Hardware für Technik und Wissenschaft"),
            ("licher.jpg","Licher","Aus dem Herzen der Natur."),
            ("mts.png","MTS AUTOMOBILE","Bewegt dein Leben"),
            ("jakobi.png","JAKOBI & JAKOBI","INDIVIDUELLE MODERNISIERUNG"),

            ];

        internal static short FlashInterval = 500;
        internal static short HideInactiveFlights = 10;

        internal static void FinalizeConfig()
        {
            Columns.TrimExcess();
            MSGCenterTips.TrimExcess();
            Additionals.TrimExcess();
        }
    }
}

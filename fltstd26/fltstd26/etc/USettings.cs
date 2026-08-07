using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace fltstd26.etc
{

    public static class USettings
    {
        internal static string ConfigName = "Standard";
        internal static ConfigSettings Instance = new();
        internal static ObservableSettings Oberservables = GetObservables(Instance);
        internal static bool AutoLoad = false;

        //returned den vorherigen wert
        internal static object? UpdateSetting(string? key, object? value, Type type)
        {
            FieldInfo? field = typeof(ConfigSettings).GetFields().FirstOrDefault(x => x.Name == key);
            object? prev = field?.GetValue(USettings.Instance);
            if (prev?.GetType() == type)
            {
                field?.SetValue(USettings.Instance,value);
                return prev;
            }
            return null;
        }

        internal static ObservableSettings GetObservables(ConfigSettings instance)
        {
            return new()
            {
                logoSize_L = instance.LogoSize_L,
                logoSize_R = instance.LogoSize_R,
                titleSize = instance.TitleSize,
                clockSize = instance.ClockSize,
                dateSize = instance.DateSize,
                flashSize = instance.FlashSize,
                captionSize = instance.CaptionSize,
                elementSize = instance.ElementSize,
                targetBorderThickness = instance.TargetBorderThickness,
                msgCenterTitleSize = instance.MSGCenterTitleSize,
                msgCenterTextSize = instance.MSGCenterTextSize,
                msgCenterIconSize = instance.MSGCenterIconSize,
            };
        }


        /*internal string Name { get; init; } = "Standard";
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

        internal List<string> Columns = ["eID","Aircraft","Target","Time","Status"];*/
    }


    public partial class ConfigSettings
    {

        public ConfigSettings() { FinalizeConfig(); }

        //Meta
        public string Name = "Standard";
        public string Creator = "Arthur Hildebrand";
        public DateTime LastChange = new(2026,6,1,14,53,0);
        public DateTime Creation = new(2026,6,1,14,53,0);

        public string Homebase = "EDFP";
        //public List<string> Additionals = ["Test","DoubleTest"];
        public List<string> Additionals = [];

        //XBOARD
        // Name, Link, Width, MaxChars
        //public List<(string, string, int)> Columns = [("Flugno.", "Flt.EId", 10),("Flugzeug", "Lfz.Reg", 10),("Tickets", "Ctr.Target.HSL", 40),("Zeit", "Slot.STime", 10),("Add", "Ctr.Add", 15),("Status", "Ctr.Status", 15)];
        //public List<(string, string, int)> Columns = [("Flugno.", "Flt.EId", 15),("Flugzeug", "Lfz.Reg", 15),("Tickets", "Ctr.Target.HSL", 40),("Zeit", "Slot.STime", 15),("Status", "Ctr.Status", 15)];
        public List<BoardColumn> Columns =
        [
            new() { Name = "Flugno.", Link = "Flt.EId", Width = 12, MaxChars = 4 },
            new() { Name = "Lfz.", Link = "Lfz.Reg", Width = 17, MaxChars = 6 },
            new() { Name = "Tickets", Link = "Ctr.Target.HSL", Width = 42, MaxChars = 16 },
            new() { Name = "Zeit", Link = "Slot.STime", Width = 14, MaxChars = 5 },
            new() { Name = "Status", Link = "Ctr.Status", Width = 15, MaxChars = -1 },
        ];
        
        public string BoardTitle = "Tag der offenen Tür 2026";
        public bool SortByTime = false;
        public bool UseTargetSquareFont = false;
        public bool TargetOriented = false; //To Implement
        //minutes
        public short HideInactiveFlights = -1;
        public short FlashInterval = 500; //in ms

        public short LogoSize_L = 144;
        public short LogoSize_R = 144;
        public short TitleSize = 54;
        public short ClockSize = 48;
        public short DateSize = 24;
        public short FlashSize = 38;
        public short CaptionSize = 26;
        public short ElementSize = 20;
        public short TargetBorderThickness = 2;
        public short MSGCenterTitleSize = 32;
        public short MSGCenterTextSize = 50;
        public short MSGCenterIconSize = 90;

        public int[] Status_RedBlink = [3, 11];
        public int[] Status_GreenBlink = [2];
        public int[] Status_Green = [0, 6];
        public int[] Status_Red = [9, 10, 12];
        public int[] Status_Switch = [4, 5];
        public int[] StatusBG_Green = [2];
        public int[] StatusBG_Red = [3, 9, 10, 12];

        //FLIPBOARD
        //Last if not found
        public string Alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ,;.:-_!?&€()/<> ";
        public short LetterWidth = 42;
        public short FlipCycleSpeed = 25; //in ms

        //MSGCENTER
        public string MSGCenterDefaultTitle = "Informationen vom AEC Bad Nauheim";
        //Icon (info when null), Title (default when null), Text
        public List<(string?, string?, string)> MSGCenterTips =
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


        public bool AllowNEXUS = false;
        public bool AutoASAP = false; //Keine Fragen. Einfach machen
        public bool AutoTimeCheck = false;
        public bool EnableSlots = true; // To Implement
        public bool AntiCol = false; // To Implement
       
        public bool HidePastSlots = false;



        public bool LogFile = true;
        public bool IgnoreTransactionWeight = false;
        public bool IgnoreTransactionLength = false;
        
        public bool AskForNodeMove = true;
        public bool AskForNodePriceChange = true;

        public int DefaultCeil = 15; // To Implement
        public int QuickTolerance = 5;
        public int DefaultFltLength = 15; //in min
        public int FallbackPriceCat = 1;
        public int DefaultTgtWeight = 1;
        public bool OGNStatus = false;

        //Angesetzte Minutenanzahl durch SyncLevel
        public int OGNSyncLevel = 1; //todo
        public int OGNTolerance = 7;
        public int TakeoffDuration = 2; // in min
        public int LogCapacity = 1024;
        public int DelayTolerance = 5;
        public int MaxDelay = 30;
        public int SlotTolerance = 1;


        internal void FinalizeConfig()
        {
            Columns.TrimExcess();
            MSGCenterTips.TrimExcess();
            Additionals.TrimExcess();
        }
    }

    public partial class ObservableSettings : ObservableObject
    {
        [ObservableProperty]
        internal short logoSize_L = 0;
        [ObservableProperty]
        internal short logoSize_R = 0;
        [ObservableProperty]
        internal short titleSize = 0;
        [ObservableProperty]
        internal short clockSize = 0;
        [ObservableProperty]
        internal short dateSize = 0;
        [ObservableProperty]
        internal short flashSize = 0;
        [ObservableProperty]
        internal short captionSize = 0;
        [ObservableProperty]
        internal short elementSize = 0;
        [ObservableProperty]
        internal short targetBorderThickness = 0;
        [ObservableProperty]
        internal short msgCenterTitleSize = 0;
        [ObservableProperty]
        internal short msgCenterTextSize = 0;
        [ObservableProperty]
        internal short msgCenterIconSize = 0;
    }

    public struct BoardColumn
    {
        //Name
        public string Name { get; set; }
        //Data Link
        public string Link { get; set; }
        //Width
        public int Width { get; set; }
        //MaxChar -> FlippyFloppy Screen
        //-1 wenn ohne FlipFlop Dingens
        //Gleichmäßig aufgeteilt unter multicols
        public int MaxChars {  get; set; }
    }
}


/*internal static Dictionary<string,bool> SwitchSettings = new()
{
    //GENERAL
    {"AskForNodeMove", true },
    {"AskForNodePriceChange", true },
    {"LogFile", true },

    //XFLY
    {"AutoASAP", true }, //Flight creation in the earliest possible slot
    {"AutoTimeCheck", false }, //Flights only in the future
    {"EnableSlots", true }, //Todo
    {"AntiCol", false }, //Todo
    {"IgnoreTransactionWeight", false }, //Dont care for weight limits on transactions
    {"IgnoreTransactionLength", false }, //Ignore length on transactions

    //XPLAN
    {"HidePastSlots", false }, //Hide Past Slots on FID

    //XBOARD
    {"TargetOriented", false }, //Todo
    {"SortByTime", true }, //Sort Board by time
    {"UseTargetSquareFont", false }, //Square Fonts for Boards

    //NEXUS
    {"AllowNEXUS", false }, //Use integrated handling or nexus

    //
    {"OGNStatus", false } //Use OGN for Status Determination
};

internal static Dictionary<string,int> ValueSettings = new()
{
    //GENERAL
    {"LogCapacity", 1024 }, //Maximum Lines in Log

    //XFLY
    {"DefaultCeil", 15 }, //Todo
    {"QuickTolerance", 5 }, //QuickTicket Creation Tolerance
    {"DefaultFltLength", 15 },
    {"FallbackPriceCat", 1 },
    {"DefaultTgtWeight", 1 }, //QuickTicket Creation Tolerance
    {"SlotTolerance", 1 }, //Slot Invoking Tolerance in min
    {"DelayTolerance", 5 }, //After how many minutes a delay procedure shall be invoked
    {"MaxDelay", 30 }, //Delay can't get higher

    //BOARD
    //Optimiert für FHD
    {"LogoSize_L", 144 },
    {"LogoSize_R", 144 },
    {"TitleSize", 54 },
    {"ClockSize", 48 },
    {"DateSize", 24 },
    {"FlashSize", 38 },
    {"CaptionSize", 26 },
    {"ElementSize", 22 },
    {"TargetBorderThickness", 2 },
    {"MSGCenterTitleSize", 32 },
    {"MSGCenterTextSize", 48 },
    {"MSGCenterTextSize", 90 },
    {"FlashInterval", 500 }, //Flash Light Interval in ms
    {"HideInactiveFlights", -1 }, //Hide flights in xplan after n min

    //OGN
    {"OGNSyncLevel", 1 }, //Todo
    {"OGNTolerance", 7 }, //Max Difference between OGN and Plan start
    {"TakeoffDuration", 2 }, //How long the takeoff usually takes

};

internal static Dictionary<string,int[]> MultiSettings = new()
{
    //BOARD
    {"Status_RedBlink", [3,11] },
    {"Status_GreenBlink", [2] },
    {"Status_Green", [0,6] },
    {"Status_Red", [9,10,12] },
    {"Status_Switch", [4,5] },
    {"StatusBG_Green", [2] },
    {"StatusBG_Red", [3,9,10,12] },
};

internal static Dictionary<string,string> TextSettings = new()
{
    //OGN
    {"Homebase", "EDFP" }, //ICAO of Home airbase

    //BOARD
    {"BoardTitle", "Tag der offenen Tür 2026" }, //ICAO of Home airbase
    {"MSGCenterDefaultTitle", "Informationen vom AEC Bad Nauheim" }, //ICAO of Home airbase
};

private static Dictionary<Type,object> _typelink = new()
{
    { typeof(string), TextSettings },
    { typeof(int[]), MultiSettings },
    { typeof(int), TextSettings },
    { typeof(bool), SwitchSettings },
};

/*internal static T? Get<T>(string key)
{
    return _typelink.TryGetValue(typeof(T),out object? t) && t != null && t is Dictionary<string,T> d && d.TryGetValue(key,out T? val) ? val : default;
} */

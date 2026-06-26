using fltstd26.etc;
using System.Globalization;
using System.Xml.Linq;

namespace fltstd26.system
{
    internal class DskMan
    {
        public static string[] IAppDataFolders = ["Database","Config","Media"];
        public static string[] ICacheFolders = ["Backup","Temp","Logs"];

        public static readonly string IAppData = FileSystem.Current.AppDataDirectory;
        public static readonly string IDynIcons = Path.Combine(FileSystem.Current.AppDataDirectory,"Media");
        public static readonly string ICache = FileSystem.Current.CacheDirectory;
        public static bool Init()
        {
            try
            {
                foreach (string folder in IAppDataFolders)
                {
                    string path = Path.Combine(IAppData,folder);
                    GSettings.Paths.Add(folder,path);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                foreach (string folder in ICacheFolders)
                {
                    string path = Path.Combine(ICache,folder);
                    GSettings.Paths.Add(folder,path);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                ConProc.Log($"[DSKMAN] Initialized succesfully");
                return true;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Initialization failed: {ex.Message}",2);
                return false;
            }
        }

        /// <summary>
        /// Opens the Data Folder or the Cache Folder
        /// </summary>
        /// <param name="cache">false -> Data, true -> Cache</param>
        public static void OpenFolder(bool cache)
        {
            try
            {
                string folderPath = cache ? ICache : IAppData;
                if (Directory.Exists(folderPath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else
                {
                    ConProc.Log($"[DSKMAN] Directory does not exist: {folderPath}",2);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Error Opening Directory: {ex.Message}",2);
            }
        }

        public static bool SaveConfig(string name)
        {
            try
            {
                XElement uconfig = new("FlightStudioConfiguration",
                    new XElement("Meta",
                        new XElement("Name",USettings.Name),
                        new XElement("Creator",USettings.Creator),
                        new XElement("LastChange",USettings.LastChange.ToString("O",CultureInfo.InvariantCulture))
                    ),
                    new XElement("General",
                        new XElement("AskForNodeMove",USettings.AskForNodeMove),
                        new XElement("AskForNodePriceChange",USettings.AskForNodePriceChange),
                        new XElement("FlashingLights",USettings.FlashingLights)
                    ),
                    new XElement("Properties",
                        new XElement("Additionals",string.Join(';', USettings.Additionals))
                    ),
                    /*new XElement("XBOARD",
                        new XElement("Columns",string.Join(';', USettings.Columns))
                    ),*/
                    new XElement("XFLY",
                        new XElement("Manager",
                            new XElement("AutoASAP",USettings.AutoASAP),
                            new XElement("AutoTimeCheck",USettings.AutoTimeCheck),
                            new XElement("EnableSlots",USettings.EnableSlots),
                            new XElement("AntiCol",USettings.AntiCol)
                        ),
                        new XElement("Defaults",
                            new XElement("DefaultCeil",USettings.DefaultCeil),
                            new XElement("QuickTolerance",USettings.QuickTolerance),
                            new XElement("DefaultFltLength",USettings.DefaultFltLength),
                            new XElement("FallbackPriceCat",USettings.FallbackPriceCat),
                            new XElement("DefaultTgtWeight",USettings.DefaultTgtWeight)
                        )
                    )
                ); 
                StreamWriter stream = new(Path.Combine(GSettings.Paths["Config"],name.Trim() + ".xml"));
                uconfig.Save(stream);
                stream.Close();
                ConProc.Log($"[DSKMAN] A configuration was saved: {name}");
                return true;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Error saving configuration: {ex.Message}",2);
                return false;
            }
        }

        internal static void LoadConfig(string name)
        {
            try
            {
                string path = Path.Combine(GSettings.Paths["Config"],name.Trim() + ".xml");
                if (File.Exists(path))
                {
                    XElement uconfig = XElement.Load(path);

                    //Meta
                    USettings.Name = uconfig.Element("Name")?.Value ?? "N/A";
                    USettings.Creator = uconfig.Element("Creator")?.Value ?? "N/A";
                    USettings.LastChange = DateTime.ParseExact(uconfig.Element("LastChange")?.Value ?? "1970-01-01T17:00:01.0000000", "O", CultureInfo.InvariantCulture);

                    //General
                    USettings.AskForNodeMove = GSettings.GetBoolean(uconfig.Element("AskForNodeMove")?.Value, true);
                    USettings.AskForNodePriceChange = GSettings.GetBoolean(uconfig.Element("AskForNodePriceChange")?.Value, true);
                    USettings.FlashingLights = GSettings.GetBoolean(uconfig.Element("FlashingLights")?.Value,true);

                    //Properties
                    USettings.Additionals = uconfig.Element("Additionals")?.Value.Split(';').ToList() ?? [];

                    //XBOARD
                    //USettings.Columns = uconfig.Element("Columns")?.Value.Split(';').ToList() ?? [];

                    //XFLY
                    //Manager
                    USettings.AutoASAP = GSettings.GetBoolean(uconfig.Element("AutoASAP")?.Value,false);
                    USettings.AutoTimeCheck = GSettings.GetBoolean(uconfig.Element("AutoTimeCheck")?.Value,true);
                    USettings.EnableSlots = GSettings.GetBoolean(uconfig.Element("EnableSlots")?.Value,true);
                    USettings.AntiCol = GSettings.GetBoolean(uconfig.Element("AntiCol")?.Value,false);
                    //Defaults
                    USettings.DefaultCeil = Int32.TryParse(uconfig.Element("DefaultCeil")?.Value,out int p) ? p : 15;
                    USettings.QuickTolerance = Int32.TryParse(uconfig.Element("QuickTolerance")?.Value,out p) ? p : 5;
                    USettings.DefaultFltLength = Int32.TryParse(uconfig.Element("DefaultFltLength")?.Value,out p) ? p : 15;
                    USettings.FallbackPriceCat = Int32.TryParse(uconfig.Element("FallbackPriceCat")?.Value,out p) ? p : 1;
                    USettings.DefaultTgtWeight = Int32.TryParse(uconfig.Element("DefaultTgtWeight")?.Value,out p) ? p : 1;

                    ConProc.Log($"[DSKMAN] A configuration was applied: {name}",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Error reading configuration: {ex.Message}",2);
            }
        }

       
    }
}

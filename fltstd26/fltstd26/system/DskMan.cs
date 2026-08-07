using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using Microsoft.Win32.SafeHandles;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace fltstd26.system
{
    internal class DskMan
    {
        public static string[] IAppDataFolders = ["Database","Config","Media"];
        public static string[] ICacheFolders = ["Backup","Temp","Logs"];

        public static readonly string IAppData = FileSystem.Current.AppDataDirectory;
        public static readonly string IDynIcons = Path.Combine(IAppData,"Media");
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
        public static void OpenFolder(bool cache, string? together)
        {
            try
            {
                string folderPath = cache ? ICache : IAppData;
                if(together != null) folderPath = Path.Combine(folderPath,together);
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

        internal static void Delete(string name, bool config)
        {
            try
            {
                string path = Path.Combine(IAppData,IAppDataFolders[config ? 1 : 0],name);
                if (File.Exists(path))
                {
                    string newfile = $"Deleted{(config ? "Config" : "Profile")} -" + DateTime.Now.ToString("dd-MM-yy-HH-mm") + (config ? ".xml" : ".sqlite");
                    File.Move(path,Path.Combine(ICache,ICacheFolders[1],newfile));
                }
                else
                {
                    ConProc.Log($"[DSKMAN] {(config ? "Config" : "Profile")} does not exist: {path}",2);
                }
                
            }
            catch(Exception e)
            {
                ConProc.Log($"[DSKMAN] Error Deleting {(config ? "Config" : "Profile")}: {e.Message}",2);
            }
        }

        //true - config, false - profile
        internal static List<IFile> GetFolder(bool config)
        {
            List<IFile> files = [];
            try
            {
                if (!config && RData.Active()) throw new Exception("RDATA is blocking the Database");
                string path = Path.Combine(IAppData,IAppDataFolders[config ? 1 : 0]);
                if (Directory.Exists(path))
                { 
                    string[] paths = Directory.GetFiles(path);
                    foreach(string file in paths)
                    {
                        SafeFileHandle handle = File.OpenHandle(file);
                        string context = Lang.last_change + ": " + File.GetLastWriteTime(handle).ToString("G");
                        string name = file.Replace(path,string.Empty)[1..];
                        files.Add(new() { Context = context,Location = file,Name = name });
                        handle.Close();
                    }
                }
                else
                {
                    ConProc.Log($"[DSKMAN] Profile Directory does not exist: {path}",2);
                }

            }
            catch (Exception e)
            {
                ConProc.Log($"[DSKMAN] Error Requesting Profiles: {e.Message}",2);
            }
            files.TrimExcess();
            return files;
        }

        public static string? SaveConfig(string name,bool backup = false,bool absolute = false)
        {
            try
            {
                string path = absolute ? name : Path.Combine(backup ? GSettings.Paths["Backup"] : GSettings.Paths["Config"],name.Trim());
                if (!path.EndsWith(".xml")) path += ".xml";
                StreamWriter stream = new(path);
                XmlSerializer xml = new(typeof(ConfigSettings));
                xml.Serialize(stream,USettings.Instance);
                stream.Close();
                ConProc.Log($"[DSKMAN] A configuration was saved: {name}");
                return path;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Error saving configuration: {ex.Message}",2);
                return null;
            }
        }

        internal static void LoadConfig(string name, bool isAbsolute)
        {
            try
            {
                string path = isAbsolute ? name : Path.Combine(GSettings.Paths["Config"],name.Trim() + ".xml");

                if (File.Exists(name))
                {
                    StreamReader stream = new(name);
                    XmlSerializer xml = new(typeof(ConfigSettings));
                    object? deserialized = xml.Deserialize(stream);
                    if (deserialized != null && deserialized is ConfigSettings config)
                    {
                        USettings.Instance = config;
                        USettings.Oberservables = USettings.GetObservables(config);
                        ConProc.Log($"[DSKMAN] A configuration was applied: {name}",1);
                        System.Diagnostics.Debug.WriteLine("Config Creator is: " + USettings.Instance.Creator);
                    }
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Error reading configuration: {ex.Message}",2);
            }
        }

        internal static async Task AutoPermSaveFile(MemoryStream stream,string path)
        {
            var readPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
            var writePermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

            if (readPermissionStatus != PermissionStatus.Granted ||
                writePermissionStatus != PermissionStatus.Granted)
            {
                ConProc.Log("[DSKMAN] No storage permission.",2);
                return;
            }

            await SaveFile(stream,path);
        }

        internal static async Task SaveFile(MemoryStream stream, string path)
        {
            FileSaverResult fileSaverResult = await FileSaver.Default.SaveAsync(path,stream);
            if (fileSaverResult.IsSuccessful)
            {
                ConProc.Log("[DSKMAN] File saved at: " + fileSaverResult.FilePath,0);
            }
            else
            {
                ConProc.Log("[DSKMAN] Failed to save file: " + fileSaverResult.Exception.Message,2);
            }
        }
    }

    internal struct IFile
    {
        public required string Name { get; init; }
        public required string Context { get; set; }
        public required string Location { get; init; }
    }
}

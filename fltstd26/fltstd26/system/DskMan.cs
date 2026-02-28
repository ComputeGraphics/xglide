using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.system
{
    public static class DskMan
    {
        public static string IAppData = FileSystem.Current.AppDataDirectory;
        public static string ICache = FileSystem.Current.CacheDirectory;

        public static string[] IAppDataFolders = ["Database", "Logs", "Config"];
        public static string[] ICacheFolders = ["Temp", "Downloads"];
        public static bool Init()
        {
            try
            {
                foreach (string folder in IAppDataFolders)
                {
                    string path = Path.Combine(IAppData,folder);
                    GSettings.Paths.Add(folder, path);
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
                return true;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DSKMAN] Initialization failed: {ex.Message}");
                return false;
            }
        }

       
    }
}

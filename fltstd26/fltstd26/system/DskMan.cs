using fltstd26.etc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.system
{
    internal class DskMan
    {
        public static string[] IAppDataFolders = ["Database","Logs","Config"];
        public static string[] ICacheFolders = ["Temp","Downloads"];

        public static readonly string IAppData = FileSystem.Current.AppDataDirectory;
        public static readonly string IDynIcons = Path.Combine(FileSystem.Current.AppDataDirectory,"Config");
        public static readonly string ICache = FileSystem.Current.CacheDirectory;
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
                string folderPath = cache ? ICache: IAppData;
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
                ConProc.Log($"[DSKMAN] Error Opening Directory: {ex}",2);
            }
        }
       
    }
}

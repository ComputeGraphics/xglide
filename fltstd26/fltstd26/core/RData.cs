using SQLite;
using fltstd26.etc;
using fltstd26.system;
using System.Dynamic;
using System.Reflection;

namespace fltstd26.core
{
    //Runtime Database Access
    internal static class RData
    {
        public static bool Active => rdb != null ;

        private static SQLiteConnection? rdb;
        private static readonly string DatabaseFilename = "RData.db3";
        private static string DatabasePath =>
            Path.Combine(GSettings.dbpath, DatabaseFilename);

        public static DBSys.Handler? Handler;

        public static void Init()
        {
            try
            {
                rdb = new SQLiteConnection(DatabasePath);
                rdb.CreateTable<Sheets.Flt>();
                rdb.CreateTable<Sheets.Lfz>();
                rdb.CreateTable<Sheets.Slots>();
                rdb.CreateTable<Sheets.Target>();
                rdb.CreateTable<Sheets.PriceCat>();
                Handler = new DBSys.Handler(rdb);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Initialization failed: {ex.Message}",2);
            }
        }

        public static void Reset()
        {
            try
            {
                if (Active)
                {
                    rdb?.DropTable<Sheets.Flt>();
                    rdb?.DropTable<Sheets.Lfz>();
                    rdb?.DropTable<Sheets.Slots>();
                    rdb?.DropTable<Sheets.Target>();
                    rdb?.DropTable<Sheets.PriceCat>();
                    rdb?.CreateTable<Sheets.Flt>();
                    rdb?.CreateTable<Sheets.Lfz>();
                    rdb?.CreateTable<Sheets.Slots>();
                    rdb?.CreateTable<Sheets.Target>();
                    rdb?.CreateTable<Sheets.PriceCat>();
                    rdb?.Execute("VACUUM");
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Reset failed: {ex.Message}",2);
            }
        }

        public static void Backup(string name)
        {
            try
            {
                if (Active)
                {
                    rdb?.Backup(Path.Combine(GSettings.dbpath,name));
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Backup failed: {ex.Message}",2);
            }
        }

        public static void Close()
        {
            try
            {
                if (Active)
                {
                    rdb?.Close();
                    rdb = null;
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Close failed: {ex.Message}",2);
            }
        }

        //////////////////////////////////////////////LANG REDIRECTION//////////////////////////////////////////////

        public static List<Sheets.Flt> GetFlightTable() => (Active ? rdb?.Table<Sheets.Flt>().ToList() : []) ?? [];
        public static List<Sheets.Slots> GetSlotsTable() => (Active ? rdb?.Table<Sheets.Slots>().ToList() : []) ?? [];
        public static List<Sheets.Lfz> GetAircraftTable() => (Active ? rdb?.Table<Sheets.Lfz>().ToList() : []) ?? [];
        public static List<Sheets.Target> GetTargetTable() => (Active ? rdb?.Table<Sheets.Target>().ToList() : []) ?? [];
        public static List<Sheets.PriceCat> GetPriceTable() => (Active ? rdb?.Table<Sheets.PriceCat>().ToList() : []) ?? [];

        public static T? Get<T>(object pk) where T : class, new()
        {
            try
            {
                return rdb?.Get<T>(pk) ?? null;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Get Process failed: {e.Message}",2);
                return null;
            }
        }
        public static void SyncPriceTable()
        {
            GetPriceTable().ForEach(x =>
            {
                if (!USettings.PriceCategories.ContainsKey(x.Id))
                {
                    USettings.PriceCategories.Add(x.Id, (x.Name ?? "", x.Price));
                }
                else
                {
                    USettings.PriceCategories[x.Id] = (x.Name ?? "", x.Price);
                }
            });
        }

    }
}

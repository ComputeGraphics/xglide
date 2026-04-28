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
        public static bool Active => rdb != null;

        private static SQLiteConnection? rdb;
        private static readonly string DatabaseFilename = "RData.db3";
        private static readonly string DatabasePath = GSettings.Paths["Database"];

        internal static void Init()
        {
            try
            {
                rdb = new SQLiteConnection(Path.Combine(DatabasePath,DatabaseFilename));
                rdb.CreateTable<Sheets.Flt>();
                rdb.CreateTable<Sheets.Lfz>();
                rdb.CreateTable<Sheets.Slot>();
                rdb.CreateTable<Sheets.Target>();
                rdb.CreateTable<Sheets.PriceCat>();
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Initialization failed: {ex.Message}",2);
            }
        }

        internal static void Reset()
        {
            try
            {
                if (Active)
                {
                    rdb?.DropTable<Sheets.Flt>();
                    rdb?.DropTable<Sheets.Lfz>();
                    rdb?.DropTable<Sheets.Slot>();
                    rdb?.DropTable<Sheets.Target>();
                    rdb?.DropTable<Sheets.PriceCat>();
                    rdb?.CreateTable<Sheets.Flt>();
                    rdb?.CreateTable<Sheets.Lfz>();
                    rdb?.CreateTable<Sheets.Slot>();
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

        internal static void Backup(string name)
        {
            try
            {
                if (Active)
                {
                    rdb?.Backup(Path.Combine(DatabasePath,name));
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Backup failed: {ex.Message}",2);
            }
        }

        internal static void Close()
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

        internal static List<Sheets.Flt> GetFlightTable() => (Active ? rdb?.Table<Sheets.Flt>().ToList() : []) ?? [];
        internal static List<Sheets.Slot> GetSlotsTable() => (Active ? rdb?.Table<Sheets.Slot>().ToList() : []) ?? [];
        internal static List<Sheets.Lfz> GetAircraftTable() => (Active ? rdb?.Table<Sheets.Lfz>().ToList() : []) ?? [];
        internal static List<Sheets.Target> GetTargetTable() => (Active ? rdb?.Table<Sheets.Target>().ToList() : []) ?? [];
        internal static List<Sheets.PriceCat> GetPriceTable() => (Active ? rdb?.Table<Sheets.PriceCat>().ToList() : []) ?? [];

        internal static T? Get<T>(object pk) where T : class, new()
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

        //FUNKTIONIERT NICHT!!!
        internal static List<T>? GetWhere<T>(string Predicate) where T : class, new()
        {
            try
            {
                return rdb?.Query<T>($"SELECT * FROM {typeof(T).Name} WHERE {Predicate}") ?? null;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Get-Where failed: {e.Message}",2);
                return null;
            }
        }

        internal static bool InsertRange<T>(List<T> value) where T : class, new()
        {
            try
            {
                rdb?.InsertAll(value,true);
                return true;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Insert Transaction failed: {e.Message}",2);
                return false;
            }
        }

        internal static int Insert<T>(T value) where T : class, new()
        {
            try
            {
                rdb?.Insert(value);
                return rdb!.ExecuteScalar<int>("SELECT last_insert_rowid()");
            }
            catch (Exception e)
            {   
                ConProc.Log($"[RDATA] Insert Process failed: {e.Message}",2);
                return -1;
            }
        }
    }
}

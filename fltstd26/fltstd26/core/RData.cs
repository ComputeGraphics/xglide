using fltstd26.etc;
using fltstd26.system;
using SQLite;
using System;
using System.Dynamic;
using System.Reflection;

namespace fltstd26.core
{
    //Runtime Database Access
    internal static class RData
    {
        internal static Stack<string> BackupStack = new();
        public static Func<bool> Active = () => rdb != null;

        private static SQLiteConnection? rdb;
        private static readonly string DatabaseFilename = "RData.sqlite";
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
                ConProc.Log($"[RDATA] The database has initialized",0);
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
                if (Active())
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
                    ConProc.Log($"[RDATA] The database has been cleared",1);
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
                if (Active())
                {
                    rdb?.Backup(Path.Combine(GSettings.Paths["Backup"],name));
                    BackupStack.Push(name);
                    ConProc.Log($"[RDATA] The database has been backed up",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Backup failed: {ex.Message}",2);
            }
        }

        internal static void Restore(string name, bool backup = false)
        {
            try
            {
                if (Active()) Close();
                string TargetBackup = Path.Combine(GSettings.Paths["Backup"], backup ? BackupStack.Peek() : name);
                if (File.Exists(Path.Combine(DatabasePath,DatabaseFilename)) && File.Exists(TargetBackup))
                {
                    string PrevFile = "BeforeRestore-" + DateTime.Now.ToString("dd-MM-yy-HH-mm")+".sqlite";
                    File.Move(Path.Combine(DatabasePath,DatabaseFilename),Path.Combine(GSettings.Paths["Temp"],PrevFile));
                    File.Copy(TargetBackup,Path.Combine(DatabasePath,DatabaseFilename));
                    BackupStack.Pop();
                    Init();
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Restoration failed: {ex.Message}",2);
            }
        }

        internal static void Close()
        {
            try
            {
                if (Active())
                {
                    rdb?.Close();
                    rdb = null;
                    ConProc.Log($"[RDATA] The database system was terminated",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Close failed: {ex.Message}",2);
            }
        }

        //////////////////////////////////////////////LANG REDIRECTION//////////////////////////////////////////////

        internal static List<Sheets.Flt> GetFlightTable() => (Active() ? rdb?.Table<Sheets.Flt>().ToList() : []) ?? [];
        internal static List<Sheets.Slot> GetSlotsTable() => (Active() ? rdb?.Table<Sheets.Slot>().ToList() : []) ?? [];
        internal static List<Sheets.Lfz> GetAircraftTable() => (Active() ? rdb?.Table<Sheets.Lfz>().ToList() : []) ?? [];
        internal static List<Sheets.Target> GetTargetTable() => (Active() ? rdb?.Table<Sheets.Target>().ToList() : []) ?? [];
        internal static List<Sheets.PriceCat> GetPriceTable() => (Active() ? rdb?.Table<Sheets.PriceCat>().ToList() : []) ?? [];

        internal static T? Get<T>(object pk) where T : class, new()
        {
            try
            {
                return rdb?.Get<T>(pk);
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Get Process failed: {e.Message}",2);
                return null;
            }
        }

        internal static List<T?>? GetWhere<T>(string Predicate) where T : class, new()
        {
            try
            {
                return rdb?.Query<T?>($"SELECT * FROM {typeof(T).Name} WHERE {Predicate}");
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
                ConProc.Log($"[RDATA] A range of items has been added to the database",0);
                return true;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Insert Transaction failed: {e.Message}",2);
                return false;
            }
        }

        internal static int Insert(object value, Type type)
        {
            try
            {
                rdb?.Insert(value,"",type);
                ConProc.Log($"[RDATA] An item has been added to the " + type.Name + " table",0);
                return rdb!.ExecuteScalar<int>("SELECT last_insert_rowid()");
            }
            catch (Exception e)
            {   
                ConProc.Log($"[RDATA] Insert Process failed: {e.Message}",2);
                return -1;
            }   
        }

        internal static bool UpdateProperty<T,X>(object pk, X val, string prop) where T : class, new() where X : struct
        {
            try
            {
                ConProc.Log($"[RDATA] A range of items has been modified in the " + typeof(T).Name + " table",0);
                PropertyInfo? match = typeof(T).GetProperties().Where(p => p.Name == prop).FirstOrDefault();
                if (match != null)
                {
                    T? prev = Get<T>(pk);
                    match.SetValue(prev,val);
                    rdb?.Update(prev);
                    return true;
                }
                throw new Exception("Property not found");
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entity Property update failed: {e.Message}",2);
                return false;
            }
        }

        internal static int? Update(object obj, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] An item has been modified in the " + type?.Name+ " table",0);
                return rdb?.Update(obj, type);
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entity Update failed: {e.Message}",2);
                return null;
            }
        }

        internal static int? Delete(object pk, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] An item has been deleted out of the " + type?.Name + " table",0);
                return rdb?.Delete(pk,new(type));
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Delete Process failed: {e.Message}",2);
                return null;
            }
        }

    }
}

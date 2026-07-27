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
        internal static Stack<string> RestoreStack = new();

        public static Func<bool> Active = () => rdb != null;
        internal static bool Locked = false;

        private static SQLiteConnection? rdb;
        internal static string DatabaseFilename = "RData.sqlite";
        private static readonly string DatabasePath = GSettings.Paths["Database"];

        internal static void Init(string? DB = null)
        {
            try
            {
                Locked = DB != null;
                rdb = new SQLiteConnection(DB ?? Path.Combine(DatabasePath,DatabaseFilename));
                rdb.CreateTable<Sheets.Flt>();
                rdb.CreateTable<Sheets.Lfz>();
                rdb.CreateTable<Sheets.Slot>();
                rdb.CreateTable<Sheets.Target>();
                rdb.CreateTable<Sheets.PriceCat>();
                ConProc.Log($"[RDATA] Database initialized successfully",0);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Initialization failed: {ex.Message}",2);
                Locked = false;
            }
        }

        internal static bool Reset()
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
                    ConProc.Log($"[RDATA] The database was reset successfully",1);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Database could not be reset: {ex.Message}",2);
            }
            return false;
        }

        internal static void ApplyDatabase(List<Sheets.Flt> flts, List<Sheets.Lfz> acs, List<Sheets.Slot> slots, List<Sheets.Target> tgts, List<Sheets.PriceCat> pcs)
        {
            if(Reset())
            {
                InsertRange(flts);
                InsertRange(acs);
                InsertRange(slots);
                InsertRange(tgts);
                InsertRange(pcs);
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
                    ConProc.Log($"[RDATA] The database was backed up",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Database-Backup failed: {ex.Message}",2);
            }
        }

        internal static void Restore(string name, bool backup = false, bool undo = false)
        {
            try
            {
                if (Active()) Close();
                string TargetBackup = Path.Combine(GSettings.Paths["Backup"], backup ? (undo ? BackupStack.Peek() : RestoreStack.Peek()) : name);
                if (File.Exists(Path.Combine(DatabasePath,DatabaseFilename)) && File.Exists(TargetBackup))
                {
                    string PrevFile = "BeforeRestore-" + DateTime.Now.ToString("dd-MM-yy-HH-mm")+".sqlite";
                    File.Move(Path.Combine(DatabasePath,DatabaseFilename),Path.Combine(GSettings.Paths["Temp"],PrevFile));
                    File.Copy(TargetBackup,Path.Combine(DatabasePath,DatabaseFilename));
                    if (backup)
                    {
                        if (undo)
                        {
                            RestoreStack.Push(TargetBackup);
                            BackupStack.Pop();
                        }
                        else
                        {
                            BackupStack.Push(TargetBackup);
                            RestoreStack.Pop();
                        }
                    }
                    Init();
                }
                ConProc.Log($"[RDATA] Database " + TargetBackup + " was loaded",1);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Database could not be restored: {ex.Message}",2);
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
                    Locked = false;
                    ConProc.Log($"[RDATA] Database System closed",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Database could not be closed: {ex.Message}",2);
            }
        }

        //////////////////////////////////////////////LANG REDIRECTION//////////////////////////////////////////////

        internal static List<Sheets.Flt> GetFlightTable() => (Active() ? rdb?.Table<Sheets.Flt>().ToList() : []) ?? [];
        internal static List<Sheets.Slot> GetSlotsTable() => (Active() ? rdb?.Table<Sheets.Slot>().ToList() : []) ?? [];
        internal static List<Sheets.Lfz> GetAircraftTable() => (Active() ? rdb?.Table<Sheets.Lfz>().ToList() : []) ?? [];
        internal static List<Sheets.Target> GetTargetTable() => (Active() ? rdb?.Table<Sheets.Target>().ToList() : []) ?? [];
        internal static List<Sheets.PriceCat> GetPriceTable() => (Active() ? rdb?.Table<Sheets.PriceCat>().ToList() : []) ?? [];

        internal static T? Get<T>(object? pk) where T : class, new()
        {
            try
            {
                if (pk == null) throw new Exception("Primary Key was null");
                return rdb?.Get<T>(pk);
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Selection Failed: {e.Message}",2);
                return null;
            }
        }

        internal static object? Get(object pk, Type type)
        {
            try
            {
                return rdb?.Get(pk, new TableMapping(type));
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] General Selection failed: {e.Message}",2);
                return null;
            }
        }

        internal static List<T?> GetWhere<T>(string Predicate) where T : class, new()
        {
            try
            {
                return rdb?.Query<T?>($"SELECT * FROM {typeof(T).Name} WHERE {Predicate}") ?? [null];
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Selection after Predicate failed: {e.Message}",2);
                return [null];
            }
        }

        internal static bool InsertRange<T>(List<T> value) where T : class, new()
        {
            try
            {
                rdb?.InsertAll(value,true);
                ConProc.Log($"[RDATA] Multiple Entities were added to the database in a transaction",0);
                return true;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entity transaction failed: {e.Message}",2);
                return false;
            }
        }

        internal static int Insert(object value, Type type)
        {
            try
            {
                rdb?.Insert(value,"",type);
                ConProc.Log($"[RDATA] An Entity was added to the " + type.Name + " table",0);
                return rdb!.ExecuteScalar<int>("SELECT last_insert_rowid()");
            }
            catch (Exception e)
            {   
                ConProc.Log($"[RDATA] Failed to add Entity to the database: {e.Message}",2);
                return -1;
            }   
        }

        internal static bool UpdateProperty<X>(object pk, X? val, string prop, Type type, bool cannull = false)
        {
            try
            {
                PropertyInfo? match = type.GetProperties().Where(p => p.Name == prop).FirstOrDefault();
                if (match != null)
                {
                    if (val == null && !cannull) throw new Exception("Value can not be null");
                    object? prev = Get(pk, type);
                    match.SetValue(prev,val);
                    rdb?.Update(prev);
                    ConProc.Log($"[RDATA] A Property of an entity was updated in the " + type.Name + " table",0);
                    return true;
                }
                throw new Exception("Property not found");
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Property of Entity could not be modified: {e.Message}",2);
                return false;
            }
        }

        internal static int? Update(object obj, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] An Entity in the " + type?.Name+ " was updated",0);
                return rdb?.Update(obj, type);
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Failed to update Entity: {e.Message}",2);
                return null;
            }
        }

        internal static int? Delete(object pk, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] An Entity was removed from the " + type?.Name + " table",0);
                return rdb?.Delete(pk,new(type));
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entity could not be deleted: {e.Message}",2);
                return null;
            }
        }

    }
}

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
                ConProc.Log($"[RDATA] Datenbank initialisierung abgeschlossen",0);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Initialisierung fehlgeschlagen: {ex.Message}",2);
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
                    ConProc.Log($"[RDATA] Die Datenbank wurde zurückgesetzt",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Zurücksetzen der Datenbank fehlgeschlagen: {ex.Message}",2);
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
                    ConProc.Log($"[RDATA] Die Datenbank wurde gesichert",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Datenbanksicherung fehlgeschlagen: {ex.Message}",2);
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
                ConProc.Log($"[RDATA] Datenbank " + TargetBackup + " wurde geladen",1);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Datenbank konnte nicht wiederhergestellt werden: {ex.Message}",2);
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
                    ConProc.Log($"[RDATA] Das Datenbanksystem wude beendet",1);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[RDATA] Datenbank konnte nicht geschlossen werden: {ex.Message}",2);
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
                ConProc.Log($"[RDATA] Selektion fehlgeschlagen: {e.Message}",2);
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
                ConProc.Log($"[RDATA] Generelle Selektion fehlgeschlagen: {e.Message}",2);
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
                ConProc.Log($"[RDATA] Selektion nach Eigenschaft fehlgeschlagen: {e.Message}",2);
                return [null];
            }
        }

        internal static bool InsertRange<T>(List<T> value) where T : class, new()
        {
            try
            {
                rdb?.InsertAll(value,true);
                ConProc.Log($"[RDATA] Mehrere Entitäten wurden einer Tabelle hinzugefügt",0);
                return true;
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Transaktion von Entitäten fehlgeschlagen: {e.Message}",2);
                return false;
            }
        }

        internal static int Insert(object value, Type type)
        {
            try
            {
                rdb?.Insert(value,"",type);
                ConProc.Log($"[RDATA] Eine Entität wurde der " + type.Name + " Tabelle hinzugefügt",0);
                return rdb!.ExecuteScalar<int>("SELECT last_insert_rowid()");
            }
            catch (Exception e)
            {   
                ConProc.Log($"[RDATA] Einfügen in die Datenbank fehlgeschlagen: {e.Message}",2);
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
                    if (val == null && !cannull) throw new Exception("Unzulässiger Nullwert");
                    object? prev = Get(pk, type);
                    match.SetValue(prev,val);
                    rdb?.Update(prev);
                    ConProc.Log($"[RDATA] Eine Eigenschaft einer Entität der " + type.Name + " Tabelle wurde verändert",0);
                    return true;
                }
                throw new Exception("Property not found");
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Modifikation der Eigenschaften einer Entität fehlgeschlagen: {e.Message}",2);
                return false;
            }
        }

        internal static int? Update(object obj, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] Eine Entität in der " + type?.Name+ " Tabelle wurde bearbeitet",0);
                return rdb?.Update(obj, type);
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entität konnte nicht bearbeitet werden: {e.Message}",2);
                return null;
            }
        }

        internal static int? Delete(object pk, Type type)
        {
            try
            {
                ConProc.Log($"[RDATA] Eine Entität wurde aus der " + type?.Name + " Tabelle gelöscht",0);
                return rdb?.Delete(pk,new(type));
            }
            catch (Exception e)
            {
                ConProc.Log($"[RDATA] Entität konnte nicht gelöscht werden: {e.Message}",2);
                return null;
            }
        }

    }
}

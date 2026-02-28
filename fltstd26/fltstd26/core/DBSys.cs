using CommunityToolkit.Maui.Converters;
using fltstd26.etc;
using fltstd26.system;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.core
{
    internal class DBSys
    {
        public class Handler(SQLiteConnection dbin)
        {
            public SQLiteConnection db = dbin;
            private readonly string err = "[DBMNGR] An error occurred while ";
            #region Realtime Systems
            #region Request Systems
            /// <summary>
            /// Get flight information for a Flight ID.
            /// </summary>
            /// <param name="Id">FLT Identifier</param>
            /// <returns>FLT otherwise Id = -1.</returns>
            public Types.FLT GetFlight(int Id)
            {
                try
                {
                    Sheets.Flt f = db!.Find<Sheets.Flt>(Id);
                    if (f is null) return new() { Id = -1 };
                    List<Types.TGT> tgts = [.. db!.Table<Sheets.Target>().Where(x => x.LId == f.Id).ToList().Select(Converter.Convert)];
                    Types.LFZ aircraft = Converter.Convert(db!.Find<Sheets.Lfz>(f.Lfz));
                    Types.FTS timeslot = Converter.Convert(db!.Find<Sheets.Slots>(f.Slot));
                    Types.FLT flt = Converter.Convert(f);
                    flt.Aircraft = aircraft;
                    flt.TimeSlot = timeslot;
                    flt.Target = tgts;
                    return flt;
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the flight: {ex.Message}",2);
                    return new() { Id = -1 };
                }
            }

            /// <summary>
            /// Get all flight information for FTS.
            /// </summary>
            /// <param name="fts">FTS</param>
            /// <returns>List of FLT otherwise empty</returns>
            public List<Types.FLT> GetFlightsByFTS(Types.FTS fts)
            {
                try
                {
                    TableQuery<Sheets.Flt> q = db!.Table<Sheets.Flt>().Where(x => x.Slot == fts.Id);
                    if (q is null) return [];
                    List<Types.FLT> flights = [];
                    foreach (Sheets.Flt f in q)
                    {
                        List<Types.TGT> tgts = [.. db!.Table<Sheets.Target>().Where(x => x.LId == f.Id).ToList().Select(Converter.Convert)];
                        Types.LFZ aircraft = Converter.Convert(db!.Find<Sheets.Lfz>(f.Lfz));
                        Types.FTS timeslot = Converter.Convert(db!.Find<Sheets.Slots>(f.Slot));
                        Types.FLT flt = Converter.Convert(f);
                        flt.Aircraft = aircraft;
                        flt.TimeSlot = timeslot;
                        flt.Target = tgts;
                        flights.Add(flt);
                    }
                    return flights;
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the flights: {ex.Message}",2);
                    return [];
                }
            }

            /// <summary>
            /// Get slot information for a Slot ID.
            /// </summary>
            /// <param name="Id">FTS Identifier</param>
            /// <returns>FTS otherwise Id = -1.</returns>
            public Types.FTS GetSlot(int Id)
            {
                try
                {
                    Sheets.Slots f = db!.Find<Sheets.Slots>(Id);
                    return f is null ? new() { Id = -1 } : Converter.Convert(f);
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the slot: {ex.Message}",2);
                    return new() { Id = -1 };
                }
            }

            /// <summary>
            /// Get matching slots for a slot starting time/ending time.
            /// </summary>
            /// <param name="t">DateTime Starttime/Endtime</param>
            /// <param name="a">Bool - True if Endtime else Starttime</param>
            /// <returns>List of FTS otherwise empty</returns>
            public List<Types.FTS> GetSlotsByTime(DateTime t,Boolean a)
            {
                try
                {
                    List<Types.FTS> s = [.. db!.Table<Sheets.Slots>().Where(x => (a ? x.FTime : x.STime) == t).ToList().Select(Converter.Convert)];
                    return s ?? [];
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the slots: {ex.Message}",2);
                    return [];
                }
            }

            /// <summary>
            /// Get aircraft information for an Aircraft ID.
            /// </summary>
            /// <param name="Id">LFZ Identifier</param>
            /// <returns>LFZ otherwise Id = -1.</returns>
            public Types.LFZ GetAircraft(int Id)
            {
                try
                {
                    Sheets.Lfz f = db!.Find<Sheets.Lfz>(Id);
                    return f is null ? new() { Id = -1 } : Converter.Convert(f);
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the aircraft: {ex.Message}",2);
                    return new() { Id = -1 };
                }
            }

            /// <summary>
            /// Get target information for a Target ID.
            /// </summary>
            /// <param name="Id">TGT Identifier</param>
            /// <returns>TGT otherwise Id = -1.</returns>
            public Types.TGT GetTarget(int Id)
            {
                try
                {
                    Sheets.Target f = db!.Find<Sheets.Target>(Id);
                    return f is null ? new() { Id = -1 } : Converter.Convert(f);
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the target: {ex.Message}",2);
                    return new() { Id = -1 };
                }
            }

            /// <summary>
            /// Get matching targets for a target link.
            /// </summary>
            /// <param name="lid">Linked ID</param>
            /// <returns>List of TGT otherwise empty</returns>
            public List<Types.TGT> GetTargetsByLink(int lid)
            {
                try
                {
                    List<Types.TGT> s = [.. db!.Table<Sheets.Target>().Where(x => x.LId == lid).ToList().Select(Converter.Convert)];
                    return s ?? [];
                }
                catch (Exception ex)
                {
                    ConProc.Log($"[DBMNGR] An error occurred while fetching the targets: {ex.Message}",2);
                    return [];
                }
            }
            #endregion
            #region Modification Systems
            /// <summary>
            /// Insert new flight into database.
            /// WITHOUT TARGETS
            /// </summary>
            /// <param name="flt">FLT</param>
            /// <returns>Identifier or -1</returns>
            public int InsertFlight(Types.FLT flt,bool auto = false)
            {
                try
                {
                    Sheets.Flt f = Converter.Convert(flt);
                    if (auto) _ = flt.Target.Select(x => InsertTarget(x,flt.Id));
                    db!.Insert(f);
                    return f.Id;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the flight: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Update flight in the Database.
            /// </summary>
            /// <param name="flt">FLT</param>
            /// <returns>Rows Updated or -1</returns>
            public int UpdateFlight(Types.FLT flt)
            {
                try { return db!.Update(Converter.Convert(flt)); }
                catch (Exception ex)
                {
                    ConProc.Log(err + "updating the flight: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Insert new target into database.
            /// </summary>
            /// <param name="tgt">TGT</param>
            /// <returns>Identifier</returns>
            public int InsertTarget(Types.TGT tgt,int lid = 0)
            {
                try
                {
                    Sheets.Target s = Converter.Convert(tgt);
                    s.LId = lid;
                    db!.Insert(s);
                    return s.Id;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the target: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Update target in the Database.
            /// </summary>
            /// <param name="tgt">TGT</param>
            /// <returns>Rows Updated or -1</returns>
            public int UpdateTarget(Types.TGT tgt)
            {
                try { return db!.Update(Converter.Convert(tgt)); }
                catch (Exception ex)
                {
                    ConProc.Log(err + "updating the target: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Insert new slot into database.
            /// </summary>
            /// <param name="fts">FTS</param>
            /// <returns>Identifier</returns>
            public int InsertSlot(Types.FTS fts)
            {
                try
                {
                    Sheets.Slots s = Converter.Convert(fts);
                    db!.Insert(s);
                    return s.Id;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the slot: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Update slot in the Database.
            /// </summary>
            /// <param name="fts">FTS</param>
            /// <returns>Rows Updated</returns>
            public int UpdateSlot(Types.FTS fts)
            {
                try { return db!.Update(Converter.Convert(fts)); }
                catch (Exception ex)
                {
                    ConProc.Log(err + "updating the slot: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Insert new aircraft into database.
            /// </summary>
            /// <param name="lfz">LFZ</param>
            /// <returns>Identifier</returns>
            public int InsertAircraft(Types.LFZ lfz)
            {
                try
                {
                    Sheets.Lfz s = Converter.Convert(lfz);
                    db!.Insert(s);
                    return s.Id;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the aircraft: " + ex.Message,2);
                    return -1; // Indicate failure
                }
            }

            /// <summary>
            /// Update aircraft in the Database.
            /// </summary>
            /// <param name="lfz">LFZ</param>
            /// <returns>Rows Updated</returns>
            public int UpdateAircraft(Types.LFZ lfz)
            {
                try { return db!.Update(Converter.Convert(lfz)); }
                catch (Exception ex)
                {
                    ConProc.Log(err + "updating the aircraft: " + ex.Message,2);
                    return -1;
                }
            }

            /// <summary>
            /// Delete something out of the Database.
            /// </summary>
            /// <param name="Id">Identifier</param>
            /// <returns>Number of objects removed</returns>
            public int Delete<T>(int Id) where T : struct
            {
                try { return db!.Delete<T>(Id); }
                catch (Exception ex)
                {
                    ConProc.Log(err + "deleting an object: " + ex.Message,2);
                    return -1;
                }
            }
            #endregion
            #endregion
            #region Transaction Systems

            /// <summary>
            /// Updates multiple flights in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="flts">List of Flights that shall be modified</param>
            /// <returns>Success</returns>
            public bool UpdateFlightT(List<Types.FLT> flts)
            {
                try
                {
                    db!.RunInTransaction(() => flts.ForEach(x => db!.Update(Converter.Convert(x))));
                    return true;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting flights: "+ex.Message,2);
                    return false;
                }
            }

            /// <summary>
            /// Updates multiple targets in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="tgts">List of Targets that shall be modified</param>
            /// <returns>Success</returns>
            public bool UpdateTargetT(List<Types.TGT> tgts)
            {
                try
                {
                    db!.RunInTransaction(() => tgts.ForEach(x => db!.Update(Converter.Convert(x))));
                    return true;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting targets: " + ex.Message,2);
                    return false;
                }
            }

            /// <summary>
            /// Updates multiple slots in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="ftss">List of Slots that shall be modified</param>
            /// <returns>Success</returns>
            public bool UpdateSlotT(List<Types.FTS> ftss)
            {
                try
                {
                    db!.RunInTransaction(() => ftss.ForEach(x => db!.Update(Converter.Convert(x))));
                    return true;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting slots: " + ex.Message,2);
                    return false;
                }
            }

            /// <summary>
            /// Updates multiple aircrafts in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="lfzs">List of Aircraft that shall be modified</param>
            /// <returns>Success</returns>
            public bool UpdateAircraftT(List<Types.LFZ> lfzs)
            {
                try
                {
                    db!.RunInTransaction(() => lfzs.ForEach(x => db!.Update(Converter.Convert(x))));
                    return true;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting aircraft: " + ex.Message,2);
                    return false;
                }
            }

            /// <summary>
            /// Creates multiple flights in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="flts">List of Flights that shall be created</param>
            /// <returns>Created IDs or empty</returns>
            public List<int> InsertFlightT(List<Types.FLT> flts, bool auto = false)
            {
                try
                {
                    List<int> ids = [];
                    db!.RunInTransaction(() =>
                    {
                        flts.ForEach(x =>
                        {
                            Sheets.Flt f = Converter.Convert(x);
                            if (auto) _ = x.Target.Select(a => InsertTarget(a,x.Id));
                            db!.Insert(f);
                            ids.Add(f.Id);
                        });
                    });
                    return ids;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting flights: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Creates multiple targets in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="s">List of Targets that shall be created</param>
            /// <returns>Created IDs or empty</returns>
            public List<int> InsertTargetT(List<Types.TGT> s)
            {
                try
                {
                    List<int> ids = [];
                    db!.RunInTransaction(() =>
                    {
                        s.ForEach(x =>
                        {
                            Sheets.Target f = Converter.Convert(x);
                            db!.Insert(f);
                            ids.Add(f.Id);
                        });
                    });
                    return ids;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting targets: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Creates multiple slots in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="s">List of Slots that shall be created</param>
            /// <returns>Created IDs or empty</returns>
            public List<int> InsertSlotT(List<Types.FTS> s)
            {
                try
                {
                    List<int> ids = [];
                    db!.RunInTransaction(() =>
                    {
                        s.ForEach(x =>
                        {
                            Sheets.Slots f = Converter.Convert(x);
                            db!.Insert(f);
                            ids.Add(f.Id);
                        });
                    });
                    return ids;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting slots: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Creates multiple aircraft in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="s">List of Aircraft that shall be created</param>
            /// <returns>Created IDs or empty</returns>
            public List<int> InsertAircraftT(List<Types.LFZ> s)
            {
                try
                {
                    List<int> ids = [];
                    db!.RunInTransaction(() =>
                    {
                        s.ForEach(x =>
                        {
                            Sheets.Lfz f = Converter.Convert(x);
                            db!.Insert(f);
                            ids.Add(f.Id);
                        });
                    });
                    return ids;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting aircraft: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Delete multiple items in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <param name="Id">Identifiers</param>
            /// <returns>Succes</returns>
            public bool DeleteT<T>(List<int> Id) where T : struct
            {
                try
                {
                    db!.RunInTransaction(() => Id.ForEach(x => db!.Delete<T>(Id)));
                    return true;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "deleting objects: " + ex.Message,2);
                    return false;
                }
            }
            #endregion
        }

        public class Converter
        {
            //////////////////////CONVERT TO PROCESSING TYPES//////////////////////

            /// <summary>
            /// Full Conversion. Converts Database Sheet into Record
            /// </summary>
            /// <param name="Con">Sheet Slot</param>
            /// <returns>FTS</returns>
            public static Types.FTS Convert(Sheets.Slots Con)
            {
                return new()
                {
                    Id = Con.Id,
                    End = Con.FTime,
                    Start = Con.FTime,
                    Length = Con.Length,
                };
            }

            /// <summary>
            /// Full Conversion. Converts Database Sheet into Record
            /// </summary>
            /// <param name="Con">Sheet Aircraft</param>
            /// <returns>LFZ</returns>
            public static Types.LFZ Convert(Sheets.Lfz Con)
            {
                return new()
                {
                    Id = Con.Id,
                    Reg = Con.Reg ?? "",
                    Type = Con.Type ?? "",
                    AutoAssign = Con.AutoAssign,
                    Interval = Con.Interval,
                    PriceCat = Con.PriceCat,
                    Seats = Con.Seats,
                };
            }

            /// <summary>
            /// Partial Conversion [Link is not carried]. Converts Database Sheet into Record
            /// </summary>
            /// <param name="Con">Sheet Aircraft</param>
            /// <returns>TGT</returns>
            public static Types.TGT Convert(Sheets.Target Con)
            {
                return new()
                {
                    Id = Con.Id,
                    Name = Con.Name ?? "",
                    Persistent = Con.Persistent,
                    Price = Con.Price,
                    QuickTicket = Con.QuickTicket,
                    Weight = Con.Weight,
                };
            }

            /// <summary>
            /// Partial Conversion [Aircraft, TimeSlot, Target not carried]. Converts Database Sheet into Record
            /// </summary>
            /// <param name="Con">Sheet Aircraft</param>
            /// <returns>TGT</returns>
            public static Types.FLT Convert(Sheets.Flt Con)
            {
                return new()
                {
                    Id = Con.Id,
                    eId = Con.EId,
                    Status = Con.Status,
                    Add = Con.Add ?? "",
                };
            }

            ///////////////////////CONVERT TO DATABASE TYPES///////////////////////

            /// <summary>
            /// Full Conversion. Converts Record into Database Sheet
            /// </summary>
            /// <param name="Con">FTS</param>
            /// <returns>Sheet Slot</returns>
            public static Sheets.Slots Convert(Types.FTS Con)
            {
                return new()
                {
                    Id = Con.Id,
                    STime = Con.Start,
                    FTime = Con.End,
                    Length = Con.Length,
                };
            }

            /// <summary>
            /// Full Conversion. Converts Record into Database Sheet
            /// </summary>
            /// <param name="Con">LFZ</param>
            /// <returns>Sheet Aircraft</returns>
            public static Sheets.Lfz Convert(Types.LFZ Con)
            {
                return new()
                {
                    Id = Con.Id,
                    Interval = Con.Interval,
                    AutoAssign = Con.AutoAssign,
                    PriceCat = Con.PriceCat,
                    Type = Con.Type,
                    Reg = Con.Type,
                    Seats = Con.Seats,
                };
            }

            /// <summary>
            /// Partial Conversion [Link not carried]. Converts Record into Database Sheet
            /// </summary>
            /// <param name="Con">TGT</param>
            /// <returns>Sheet Target</returns>
            public static Sheets.Target Convert(Types.TGT Con)
            {
                return new()
                {
                    Id = Con.Id,
                    Name = Con.Name,
                    Persistent = Con.Persistent,
                    QuickTicket = Con.QuickTicket,
                    Price = Con.Price,
                    Weight = Con.Weight,
                };
            }

            /// <summary>
            /// Full Conversion [Given Links are carried]. Converts Record into Database Sheet
            /// </summary>
            /// <param name="Con">FLT</param>
            /// <returns>Sheet Flight</returns>
            public static Sheets.Flt Convert(Types.FLT Con)
            {
                return new()
                {
                    Id = Con.Id,
                    EId = Con.eId,
                    Add = Con.Add,
                    Lfz = Con.Aircraft.Id,
                    Slot = Con.TimeSlot.Id,
                    Status = Con.Status,
                };
            }
        }
    }
}

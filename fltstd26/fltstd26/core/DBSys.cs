/*using CommunityToolkit.Maui.Converters;
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
            private readonly SQLiteConnection db = dbin;
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
                    int aircraft = db!.Find<Sheets.Lfz>(f.Lfz).Id;
                    int timeslot = db!.Find<Sheets.Slots>(f.Slot).Id;
                    Types.FLT flt = Converter.Convert(f);
                    flt.Aircraft = aircraft;
                    flt.TimeSlot = timeslot;
                    flt.Target = tgts;
                    return flt;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "fetching the flight: " + ex.Message ,2);
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
                        int aircraft = db!.Find<Sheets.Lfz>(f.Lfz).Id;
                        int timeslot = db!.Find<Sheets.Slots>(f.Slot).Id;
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
                    ConProc.Log(err + "fetching the flights: " + ex.Message,2);
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
                    ConProc.Log(err + "fetching the slot: " + ex.Message,2);
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
                    ConProc.Log(err + "fetching the slots: " + ex.Message,2);
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
                    ConProc.Log(err + "fetching the aircraft: " + ex.Message,2);
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
                    ConProc.Log(err + "fetching the target: " + ex.Message,2);
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
                    ConProc.Log(err + "fetching the targets: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Request all converted objects from a specified table
            /// </summary>
            /// <returns>List of T otherwise empty</returns>
            public List<T> GetAll<T>() where T : struct
            {
                try
                {
                    if (typeof(T) == typeof(Types.FLT))
                    {
                        List<Types.FLT> flights = [];
                        foreach (Sheets.Flt f in db!.Table<Sheets.Flt>())
                        {
                            List<Types.TGT> tgts = [.. db!.Table<Sheets.Target>().Where(x => x.LId == f.Id).ToList().Select(Converter.Convert)];
                            int aircraft = db!.Find<Sheets.Lfz>(f.Lfz).Id;
                            int timeslot = db!.Find<Sheets.Slots>(f.Slot).Id;
                            Types.FLT flt = Converter.Convert(f);
                            flt.Aircraft = aircraft;
                            flt.TimeSlot = timeslot;
                            flt.Target = tgts;
                            flights.Add(flt);
                        }
                        return [.. flights.Cast<T>()];
                    }
                    else if (typeof(T) == typeof(Types.TGT))
                    {
                        return [.. db!.Table<Sheets.Target>().ToList().Select(Converter.Convert).Cast<T>()];
                    }
                    else if (typeof(T) == typeof(Types.FTS))
                    {
                        return [.. db!.Table<Sheets.Slots>().ToList().Select(Converter.Convert).Cast<T>()];
                    }
                    else if (typeof(T) == typeof(Types.LFZ))
                    {
                        return [.. db!.Table<Sheets.Lfz>().ToList().Select(Converter.Convert).Cast<T>()];
                    }
                    else return [];
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "fetching the table: " + ex.Message,2);
                    return [];
                }
            }

            #endregion
            #region Modification Systems
            /// <summary>
            /// Insert new flight into database.
            /// </summary>
            /// <param name="flt">FLT</param>
            /// <param name="auto">Auto Insert Targets</param>
            /// <returns>Identifier or -1 in Value 1 - Identifiers of Targets or also -1</returns>
            public (int, int[]) InsertFlight(Types.FLT flt,bool auto = false)
            {
                try
                {
                    Sheets.Flt f = Converter.Convert(flt);
                    int[] tgtids = auto ? [.. InsertTargetT(flt.Target)] : [-1];
                    db!.Insert(f);
                    return (db!.Table<Sheets.Flt>().Last().Id, tgtids);
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the flight: " + ex.Message,2);
                    return (-1, [-1]);
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
                    return db!.Table<Sheets.Target>().Last().Id;
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
                    return db!.Table<Sheets.Slots>().Last().Id;
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
                    return db!.Table<Sheets.Lfz>().Last().Id;
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
            /// Insert a new price category into database.
            /// </summary>
            /// <returns>Identifier</returns>
            public int InsertPrice(string name, int price)
            {
                try
                {
                    Sheets.PriceCat s = new()
                    {
                        Name = name,
                        Price = price
                    };
                    db!.Insert(s);
                    return db!.Table<Sheets.PriceCat>().Last().Id;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "inserting the price category: " + ex.Message,2);
                    return -1; // Indicate failure
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
            /// <param name="auto">Auto add Targets of Flight to Target DB</param>
            /// <returns>Tuple of Created Flight ID and Target IDs or empty</returns>
            public List<(int, int[])> InsertFlightT(List<Types.FLT> flts, bool auto = false)
            {
                try
                {
                    List<(int, int[]) > ids = [];

                    int insertedRows = 0;
                    db!.RunInTransaction(() =>
                    {    
                        flts.ForEach(x =>
                        {
                            int[] tgtIDs = [];
                            if (auto) tgtIDs = [.. x.Target.Select(a => InsertTarget(a,x.Id))];
                            insertedRows += db!.Insert(Converter.Convert(x));
                            ids.Add((db!.Table<Sheets.Flt>().Last().Id, tgtIDs));
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
                            db!.Insert(Converter.Convert(x));
                            ids.Add(db!.Table<Sheets.Target>().Last().Id);
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
                    if (db!.Table<Sheets.Slots>().Count() < 256)
                    {
                        List<int> ids = [];
                        db!.RunInTransaction(() =>
                        {
                            s.ForEach(x =>
                            {
                                db!.Insert(Converter.Convert(x));
                                ids.Add(db!.Table<Sheets.Slots>().Last().Id);
                            });
                        });
                        return ids;
                    }
                    else
                    {
                        ConProc.Log(err + "transacting slots: Too many slots.",2);
                        return [];
                    }
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
                    if (db!.Table<Sheets.Lfz>().Count() < 256)
                    {
                        List<int> ids = [];
                        db!.RunInTransaction(() =>
                        {
                            s.ForEach(x =>
                            {
                                db!.Insert(Converter.Convert(x));
                                ids.Add(db!.Table<Sheets.Lfz>().Last().Id);
                            });
                        });
                        return ids;
                    }
                    else
                    {
                        ConProc.Log(err + "transacting aircraft: Too many aircraft.",2);
                        return [];
                    }
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting aircraft: " + ex.Message,2);
                    return [];
                }
            }

            /// <summary>
            /// Creates multiple price cats in a transaction. If one update fails, all updates will be rolled back.
            /// </summary>
            /// <returns>Created IDs or empty</returns>
            public List<int> InsertPriceT(List<string> name, List<int> price)
            {
                try
                {
                    if(name.Count != price.Count) throw new Exception("Name and Price count do not match.");
                    List<int> ids = [];
                    db!.RunInTransaction(() =>
                    {
                        for(int i = 0; i < name.Count; i++)
                        {
                            Sheets.PriceCat s = new()
                            {
                                Name = name[i],
                                Price = price[i]
                            };
                            db!.Insert(s);
                            ids.Add(db!.Table<Sheets.PriceCat>().Last().Id);
                        }
                    });
                    return ids;
                }
                catch (Exception ex)
                {
                    ConProc.Log(err + "transacting price categories: " + ex.Message,2);
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
    }
}
*/
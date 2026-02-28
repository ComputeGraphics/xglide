/*sing fltstd26.etc;
using SQLite;
using System.Collections.Generic;

#pragma warning disable IDE1006 // Disable naming style warnings for this file

namespace fltstd26.core
{
    //THIS DOCUMENT IS DEPRECATED
    internal class DBToolsDeprecated
    {

        #region Request Systems
        #region Flight Data Requests
        //////////////////////REQUEST FLIGHT DATA//////////////////////

        /// <summary>
        /// Get flight information for a Flight ID.
        /// </summary>
        /// <param name="Id">FLT Identifier</param>
        /// <returns>FLT otherwise Id = -1.</returns>
        public static Types.FLT getFlight(SQLiteConnection db,int Id)
        {
            try
            {
                Sheets.Flt f = db!.Find<Sheets.Flt>(Id);
                if (f is null)
                {
                    return new Types.FLT
                    {
                        Id = -1,
                    };
                }
                else
                {
                    Sheets.Lfz l = db!.Find<Sheets.Lfz>(f.Lfz);
                    Sheets.Slots s = db!.Find<Sheets.Slots>(f.Slot);
                    List<Sheets.Target> t = [.. db!.Table<Sheets.Target>().Where(x => x.LId == f.Id)];

                    Types.LFZ aircraft = new()
                    {
                        Id = l.Id,
                        Reg = l.Reg!,
                        Type = l.Type!,
                        Seats = l.Seats,
                        AutoAssign = l.AutoAssign
                    };

                    Types.FTS timeslot = new()
                    {
                        Id = s.Id,
                        Start = s.STime,
                        End = s.FTime,
                        Length = s.Length
                    };

                    List<Types.TGT> targets = [];
                    foreach (var tg in t)
                    {
                        targets.Add(new()
                        {
                            Id = tg.Id,
                            LId = tg.LId,
                            Name = tg.Name!,
                            Weight = tg.Weight,
                            Price = tg.Price,
                            QuickTicket = tg.QuickTicket,
                            Persistent = tg.Persistent
                        });
                    }

                    return new Types.FLT
                    {
                        Id = f.Id,
                        eId = f.EId,
                        Aircraft = aircraft,
                        Target = [.. targets],
                        TimeSlot = timeslot,
                        Status = f.Status,
                        Add = f.Add ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the flight: {ex.Message}", 2);
                return new Types.FLT
                {
                    Id = -1,
                };
            }
        }

        /// <summary>
        /// Get All flight information for a Flight Time Slot.
        /// </summary>
        /// <param name="fts">Searched FTS</param>
        /// <returns>List of FLT otherwise empty</returns>
        public static List<Types.FLT> getFlightsByFTS(SQLiteConnection db,Types.FTS fts)
        {
            try
            {
                var query = db!.Table<Sheets.Flt>().Where(x => x.Slot == fts.Id);
                if (query is null)
                {
                    return [];
                }
                else
                {
                    List<Types.FLT> flights = [];
                    foreach (var f in query)
                    {
                        Sheets.Lfz l = db!.Find<Sheets.Lfz>(f.Lfz);
                        Sheets.Slots s = db!.Find<Sheets.Slots>(f.Slot);
                        List<Sheets.Target> t = [.. db!.Table<Sheets.Target>().Where(x => x.LId == f.Id)];
                        Types.LFZ aircraft = new()
                        {
                            Id = l.Id,
                            Reg = l.Reg!,
                            Type = l.Type!,
                            Seats = l.Seats,
                            AutoAssign = l.AutoAssign
                        };
                        Types.FTS timeslot = new()
                        {
                            Id = s.Id,
                            Start = s.STime,
                            End = s.FTime,
                            Length = s.Length
                        };
                        List<Types.TGT> targets = [];
                        foreach (var tg in t)
                        {
                            targets.Add(new Types.TGT
                            {
                                Id = tg.Id,
                                LId = tg.LId,
                                Name = tg.Name!,
                                Weight = tg.Weight,
                                QuickTicket = tg.QuickTicket,
                                Price = tg.Price,
                                Persistent = tg.Persistent
                            });
                        }

                        flights.Add(new Types.FLT
                        {
                            Id = f.Id,
                            eId = f.EId,
                            Aircraft = aircraft,
                            Target = [.. targets],
                            TimeSlot = timeslot,
                            Departure = f.STime,
                            Arrival = f.FTime,
                            Length = f.Length,
                            Status = f.Status,
                            Add = f.Add ?? ""
                        });
                    }

                    return flights;
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the flights: {ex.Message}",2);
                return [];
            }
        }
        #endregion

        #region Slot Data Requests
        ///////////////////////REQUEST SLOT DATA///////////////////////

        /// <summary>
        /// Get Flight Time Slot information for a Slot ID.
        /// </summary>
        /// <param name="Id">Searched Identifier</param>
        /// <returns>FTS otherwise Id = -1</returns>
        public static Types.FTS getSlot(SQLiteConnection db,int Id)
        {
            try
            {
                Sheets.Slots s = db!.Find<Sheets.Slots>(Id);
                if (s is null) { return new Types.FTS { Id = -1 }; }
                else
                {
                    return new Types.FTS
                    {
                        Id = s.Id,
                        Start = s.STime,
                        End = s.FTime,
                        Length = s.Length
                    };
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the slot: {ex.Message}", 2);
                return new Types.FTS { Id = -1 };
            }
        }

        /// <summary>
        /// Get Flight Time Slot information for a Slot Start Time.
        /// </summary>
        /// <param name="starttime">Searched Start Time</param>
        /// <returns>FTS otherwise Id = -1</returns>
        public static Types.FTS getSlotByStartTime(SQLiteConnection db,DateTime starttime)
        {
            try
            {
                Sheets.Slots s = db!.Table<Sheets.Slots>().Where(x => x.STime == starttime).FirstOrDefault();
                if (s is null) { return new Types.FTS { Id = -1 }; }
                else
                {
                    return new Types.FTS
                    {
                        Id = s.Id,
                        Start = s.STime,
                        End = s.FTime,
                        Length = s.Length
                    };
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the slot: {ex.Message}", 2);
                return new Types.FTS { Id = -1 };
            }
        }

        #endregion

        #region Aircraft Data Requests

        /////////////////////REQUEST AIRCRAFT DATA/////////////////////

        /// <summary>
        /// Get Aircraft information for a Aircraft ID.
        /// </summary>
        /// <param name="Id">Searched Identifier</param>
        /// <returns>LFZ otherwise Id = -1</returns>
        public static Types.LFZ getAircraft(SQLiteConnection db,int Id)
        {
            try
            {
                Sheets.Lfz l = db!.Find<Sheets.Lfz>(Id);
                if (l is null) { return new Types.LFZ { Id = -1 }; }
                else
                {
                    return new Types.LFZ
                    {
                        Id = l.Id,
                        Reg = l.Reg!,
                        Type = l.Type!,
                        Seats = l.Seats,
                        AutoAssign = l.AutoAssign
                    };
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the aircraft: {ex.Message}", 2);
                return new Types.LFZ { Id = -1 };
            }
        }

        #endregion

        #region Target Data Requests
        //////////////////////REQUEST TARGET DATA//////////////////////

        /// <summary>
        /// Get Target information for a Target ID.
        /// </summary>
        /// <param name="Id">Searched Identifier</param>
        /// <returns>TGT otherwise Id = -1</returns>
        public static Types.TGT getTarget(SQLiteConnection db,int Id)
        {
            try
            {
                Sheets.Target t = db!.Find<Sheets.Target>(Id);
                if (t is null) { return new Types.TGT { Id = -1 }; }
                else
                {
                    return new Types.TGT
                    {
                        Id = t.Id,
                        LId = t.LId,
                        Name = t.Name!,
                        Weight = t.Weight,
                        Persistent = t.Persistent
                    };
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the target: {ex.Message}", 2);
                return new Types.TGT { Id = -1 };
            }
        }

        /// <summary>
        /// Get Targets for a Target Link ID.
        /// </summary>
        /// <param name="LId">Searched Identifier</param>
        /// <returns>List of TGT otherwise empty</returns>
        public static List<Types.TGT> getTargetsbyLink(SQLiteConnection db,int LId)
        {
            try
            {
                Sheets.Target[] t = [.. db!.Table<Sheets.Target>().Where(x => x.LId == LId)];
                if (t is null) { return []; }
                else
                {
                    List<Types.TGT> targets = [];
                    targets.AddRange(t.Select(tg => new Types.TGT
                    {
                        Id = tg.Id,
                        LId = tg.LId,
                        Name = tg.Name!,
                        Weight = tg.Weight,
                        Persistent = tg.Persistent
                    }));
                    return targets;
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while fetching the target: {ex.Message}", 2);
                return [];
            }
        }
        #endregion

        #endregion

        #region Modification Systems
        #region Flight Data Modifications

        /////////////////////CREATE AIRCRAFT DATA//////////////////////

        /// <summary>
        /// Insert new flight into database.
        /// WITHOUT TARGETS, TARGETS MUST BE INSERTED SEPARATELY. (Due to the fact that the Target table has a foreign key constraint to the Flight table)
        /// </summary>
        /// <param name="flt">FLT</param>
        /// <returns>Identifier</returns>
        public static int insertFlight(SQLiteConnection db,Types.FLT flt)
        {
            try
            {
                Sheets.Flt f = new()
                {
                    EId = flt.eId,
                    Lfz = flt.Aircraft.Id,
                    //Target = flt.Target,
                    STime = flt.Departure,
                    FTime = flt.Arrival,
                    Length = flt.Length,
                    Slot = flt.TimeSlot.Id,
                    Status = flt.Status,
                    Add = flt.Add
                };

                db!.Insert(f);
                return f.Id;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while inserting the flight: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Update flight in the Database.
        /// </summary>
        /// <param name="flt">FLT</param>
        /// <returns>Rows Updated</returns>
        public static int updateFlight(SQLiteConnection db,Types.FLT flt)
        {
            try
            {
                return db!.Update(new Sheets.Flt
                {
                    //Id = flt.Id,
                    EId = flt.eId,
                    Lfz = flt.Aircraft.Id,
                    //Target = flt.Target,
                    STime = flt.Departure,
                    FTime = flt.Arrival,
                    Length = flt.Length,
                    Slot = flt.TimeSlot.Id,
                    Status = flt.Status,
                    Add = flt.Add
                });
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while updating the flight: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Delete flight out of the Database.
        /// </summary>
        /// <param name="Id">FLT Identifier</param>
        /// <returns>Number of objects removed</returns>
        public static int deleteFlight(SQLiteConnection db,int Id)
        {
            try
            {
                return db!.Delete<Sheets.Flt>(Id);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while deleting the flight: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        #endregion

        #region Slot Data Modifications

        ///////////////////////CREATE SLOT DATA////////////////////////

        /// <summary>
        /// Insert new slot into database.
        /// </summary>
        /// <param name="fts">FTS</param>
        /// <returns>Identifier</returns>
        public static int insertSlot(SQLiteConnection db,Types.FTS fts)
        {
            try
            {
                Sheets.Slots s = new()
                {
                    STime = fts.Start,
                    FTime = fts.End,
                    Length = fts.Length,
                };

                db!.Insert(s);
                return s.Id;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while inserting the slot: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Update slot in the Database.
        /// </summary>
        /// <param name="fts">FTS</param>
        /// <returns>Rows Updated</returns>
        public static int updateSlot(SQLiteConnection db,Types.FTS fts)
        {
            try
            {
                return db!.Update(new Sheets.Slots
                {
                    //Id = flt.Id,
                    STime = fts.Start,
                    FTime = fts.End,
                    Length = fts.Length,
                });
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while updating the slot: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Delete slots out of the Database.
        /// </summary>
        /// <param name="Id">FTS Identifier</param>
        /// <returns>Number of objects removed</returns>
        public static int deleteSlot(SQLiteConnection db,int Id)
        {
            try
            {
                return db!.Delete<Sheets.Slots>(Id);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while deleting the slot: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        #endregion

        #region Aircraft Data Modifications

        /////////////////////CREATE AIRCRAFT DATA//////////////////////

        /// <summary>
        /// Insert new aircraft into database.
        /// </summary>
        /// <param name="lfz">LFZ</param>
        /// <returns>Identifier</returns>
        public static int insertAircraft(SQLiteConnection db,Types.LFZ lfz)
        {
            try
            {
                Sheets.Lfz s = new()
                {
                    Reg = lfz.Reg,
                    Type = lfz.Type,
                    Seats = lfz.Seats,
                    AutoAssign = lfz.AutoAssign
                };
                db!.Insert(s);
                return s.Id;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while inserting the aircraft: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Update aircraft in the Database.
        /// </summary>
        /// <param name="lfz">LFZ</param>
        /// <returns>Rows Updated</returns>
        public static int updateAircraft(SQLiteConnection db,Types.LFZ lfz)
        {
            try
            {
                return db!.Update(new Sheets.Lfz
                {
                    //Id = flt.Id,
                    Reg = lfz.Reg,
                    Type = lfz.Type,
                    Seats = lfz.Seats,
                    AutoAssign = lfz.AutoAssign
                });
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while updating the aircraft: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Delete aircraft out of the Database.
        /// </summary>
        /// <param name="Id">LFZ Identifier</param>
        /// <returns>Number of objects removed</returns>
        public static int deleteAircraft(SQLiteConnection db,int Id)
        {
            try
            {
                return db!.Delete<Sheets.Lfz>(Id);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while deleting the aircraft: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        #endregion

        #region Target Data Modifications
        ///////////////////////CREATE TARGET DATA///////////////////////

        /// <summary>
        /// Insert new target into database.
        /// </summary>
        /// <param name="tgt">TGT</param>
        /// <returns>Identifier</returns>
        public static int insertTarget(SQLiteConnection db,Types.TGT tgt)
        {
            try
            {
                Sheets.Target s = new()
                {
                    LId = tgt.LId,
                    Name = tgt.Name,
                    Weight = tgt.Weight,
                    Persistent = tgt.Persistent
                };
                db!.Insert(s);
                return s.Id;
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while inserting the target: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Update target in the Database.
        /// </summary>
        /// <param name="tgt">TGT</param>
        /// <returns>Rows Updated</returns>
        public static int updateTarget(SQLiteConnection db,Types.TGT tgt)
        {
            try
            {
                return db!.Update(new Sheets.Target
                {
                    //Id = flt.Id,
                    LId = tgt.LId,
                    Name = tgt.Name,
                    Weight = tgt.Weight,
                    Persistent = tgt.Persistent
                });
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while updating the target: {ex.Message}", 2);
                return -1; // Indicate failure
            }
        }

        /// <summary>
        /// Delte target from the database.
        /// </summary>
        /// <param name="Id">TGT Identifier</param>
        /// <returns>Number of objects removed</returns>
        public static int deleteTarget(SQLiteConnection db,int Id)
        {
            try
            {
                return db!.Delete<Sheets.Target>(Id);
            }
            catch (Exception ex)
            {
                ConProc.Log($"[DBMNGR] An error occurred while deleting the target: {ex.Message}",2);
                return -1; // Indicate failure
            }
        }
        #endregion
        #endregion

        #region Transaction Systems
        ///////////////////UPDATE TRANSACTION SYSTEM///////////////////

        #region Flight Data Transactions
        /// <summary>
        /// Updates multiple flights in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="flts">List of Flights that shall be modified</param>
        /// <returns>Success</returns>
        public static bool updateTransactionFLT(SQLiteConnection db,List<Types.FLT> flts)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var flt in flts)
                    {
                        db.Update(new Sheets.Flt
                        {
                            //Id = flt.Id,
                            EId = flt.eId,
                            Lfz = flt.Aircraft.Id,
                            //Target = flt.Target,
                            STime = flt.Departure,
                            FTime = flt.Arrival,
                            Length = flt.Length,
                            Slot = flt.TimeSlot.Id,
                            Status = flt.Status,
                            Add = flt.Add
                        });
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Slot Data Transactions
        /// <summary>
        /// Updates multiple slots in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="slots">List of Slots that shall be modified</param>
        /// <returns>Success</returns>
        public static bool updateTransactionSlot(SQLiteConnection db,List<Types.FTS> slots)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var slot in slots)
                    {
                        db.Update(new Sheets.Slots
                        {
                            //Id = slot.Id,
                            STime = slot.Start,
                            FTime = slot.End,
                            Length = slot.Length,
                        });
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Aircraft Data Transactions
        /// <summary>
        /// Updates multiple aircraft in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="aircraft">List of Aircraft that shall be modified</param>
        /// <returns>Success</returns>
        public static bool updateTransactionAircraft(SQLiteConnection db,List<Types.LFZ> aircraft)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var lfz in aircraft)
                    {
                        db.Update(new Sheets.Lfz
                        {
                            //Id = lfz.Id,
                            Reg = lfz.Reg,
                            Type = lfz.Type,
                            Seats = lfz.Seats,
                            AutoAssign = lfz.AutoAssign
                        });
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Target Data Transactions
        /// <summary>
        /// Updates multiple targets in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="tgt">List of Targets that shall be modified</param>
        /// <returns>Success</returns>
        public static bool updateTransactionTarget(SQLiteConnection db,List<Types.TGT> tgt)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var t in tgt)
                    {
                        db.Update(new Sheets.Target
                        {
                            //Id = t.Id,
                            LId = t.LId,
                            Name = t.Name,
                            Weight = t.Weight,
                            Persistent = t.Persistent
                        });
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion

        ///////////////////CREATE TRANSACTION SYSTEM///////////////////

        #region Flight Data Creation Transactions
        /// <summary>
        /// Creates multiple flights in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="flts">List of Flights that shall be created</param>
        /// <returns>Created IDs</returns>
        public static List<int> createTransactionFLT(SQLiteConnection db,List<Types.FLT> flts)
        {
            try
            {
                List<int> createdIds = [];
                db!.RunInTransaction(() =>
                {
                    foreach (var flt in flts)
                    {
                        Sheets.Flt f = new()
                        {
                            EId = flt.eId,
                            Lfz = flt.Aircraft.Id,
                            //Target = flt.Target,
                            STime = flt.Departure,
                            FTime = flt.Arrival,
                            Length = flt.Length,
                            Slot = flt.TimeSlot.Id,
                            Status = flt.Status,
                            Add = flt.Add
                        };
                        db.Insert(f);
                        createdIds.Add(f.Id);
                    }
                });
                return createdIds; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return []; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Slot Data Creation Transactions
        /// <summary>
        /// Creates multiple slots in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="slots">List of Slots that shall be created</param>
        /// <returns>Created IDs</returns>
        public static List<int> createTransactionSlot(SQLiteConnection db,List<Types.FTS> slots)
        {
            try
            {
                List<int> createdIds = [];
                db!.RunInTransaction(() =>
                {
                    foreach (var slot in slots)
                    {
                        Sheets.Slots s = new()
                        {
                            STime = slot.Start,
                            FTime = slot.End,
                            Length = slot.Length,
                        };
                        db.Insert(s);
                        createdIds.Add(s.Id);
                    }
                });
                return createdIds; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return []; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Aircraft Data Creation Transactions
        /// <summary>
        /// Creates multiple aircraft in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="aircraft">List of Aircraft that shall be created</param>
        /// <returns>Created IDs</returns>
        public static List<int> createTransactionAircraft(SQLiteConnection db,List<Types.LFZ> aircraft)
        {
            try
            {
                List<int> createdIds = [];
                db!.RunInTransaction(() =>
                {
                    foreach (var lfz in aircraft)
                    {
                        Sheets.Lfz s = new()
                        {
                            Reg = lfz.Reg,
                            Type = lfz.Type,
                            Seats = lfz.Seats,
                            AutoAssign = lfz.AutoAssign
                        };
                        db.Insert(s);
                        createdIds.Add(s.Id);
                    }
                });
                return createdIds; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return []; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Target Data Creation Transactions
        /// <summary>
        /// Creates multiple targets in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="targets">List of Targets that shall be created</param>
        /// <returns>Created IDs</returns>
        public static List<int> createTransactionTarget(SQLiteConnection db,List<Types.TGT> targets)
        {
            try
            {
                List<int> createdIds = [];
                db!.RunInTransaction(() =>
                {
                    foreach (var t in targets)
                    {
                        Sheets.Target s = new()
                        {
                            LId = t.LId,
                            Name = t.Name,
                            Weight = t.Weight,
                            Persistent = t.Persistent
                        };
                        db.Insert(s);
                        createdIds.Add(s.Id);
                    }
                });
                return createdIds; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return []; // Transaction failed and was rolled back
            }
        }
        #endregion

        ///////////////////REMOVAL TRANSACTION SYSTEM///////////////////

        #region Flight Data Removal Transactions
        /// <summary>
        /// Deletes multiple flights in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="flts">List of Flights that shall be removed</param>
        /// <returns>Success</returns>
        public static bool deleteTransactionFLT(SQLiteConnection db,List<int> flts)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var id in flts)
                    {
                        db.Delete<Sheets.Flt>(id);
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Slot Data Removal Transactions
        /// <summary>
        /// Deletes multiple slots in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="slots">List of Slots that shall be removed</param>
        /// <returns>Success</returns>
        public static bool deleteTransactionSlot(SQLiteConnection db,List<int> slots)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var id in slots)
                    {
                        db.Delete<Sheets.Slots>(id);
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Aircraft Data Removal Transactions
        /// <summary>
        /// Deletes multiple aircraft in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="aircraft">List of Aircraft that shall be removed</param>
        /// <returns>Success</returns>
        public static bool deleteTransactionAircraft(SQLiteConnection db,List<int> aircraft)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var id in aircraft)
                    {
                        db.Delete<Sheets.Lfz>(id);
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion
        #region Target Data Removal Transactions
        /// <summary>
        /// Deletes multiple target in a transaction. If one update fails, all updates will be rolled back.
        /// </summary>
        /// <param name="target">List of Target that shall be removed</param>
        /// <returns>Success</returns>
        public static bool deleteTransactionTarget(SQLiteConnection db,List<int> target)
        {
            try
            {
                db!.RunInTransaction(() =>
                {
                    foreach (var id in target)
                    {
                        db.Delete<Sheets.Target>(id);
                    }
                });
                return true; // Transaction succeeded
            }
            catch (Exception ex)
            {
                ConProc.Log($"An error occurred during the transaction: {ex.Message}",2);
                return false; // Transaction failed and was rolled back
            }
        }
        #endregion


        ////////////////////////////////////////////////////////////////
        #endregion
    }
}

#pragma warning restore IDE1006 Restore naming style warnings for this file*/
using CommunityToolkit.Maui.Converters;
using fltstd26.board;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.etc.online;
using fltstd26.Resources.Texts;
using fltstd26.system;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace fltstd26.XFly
{
    internal class Manager
    {

        #region Checks

        /// <summary>
        /// Find all existing flights, that can take the required weight
        /// </summary>
        public static List<Sheets.Flt> FindFitWeightLength(int Length,int Weight,bool Now,bool Quick)
        {
            List<Sheets.Flt> flts = [];
            foreach (Sheets.Flt flt in RData.GetFlightTable())
            {
                Sheets.Slot? slot = RData.Get<Sheets.Slot>(flt.Slot);
                if (slot is null || slot.Length != Length) continue;
                if (!Now || slot.STime > (Quick ? DateTime.Now.AddMinutes(Quick ? -USettings.Instance.QuickTolerance : 0) : DateTime.Now))
                {
                    if (FlightFitsWeight(flt.Id,flt.Lfz,Weight)) flts.Add(flt);
                }
            }
            return flts;
        }

        /// <summary>
        /// Find all Slots with specified length or also at specific time if specified
        /// </summary>
        public static List<int> FindCompatibleSlots(int Length,DateTime? times = null)
        {
            List<int> CompatibleSlots = [];
            foreach (var slot in RData.GetSlotsTable())
            {
                if (slot.Length == Length)
                {
                    if (times is null || slot.STime >= times) CompatibleSlots.Add(slot.Id);
                }
            }
            return CompatibleSlots;
        }

        public static List<int> FindAvailableAircraft(int SlotID,bool Auto,int? PriceFilter)
        {
            List<int> AvailableAircraft = [];
            foreach (Sheets.Lfz lfz in RData.GetAircraftTable())
            {
                if ((!Auto || lfz.AutoAssign == true) && lfz.AvailTimes is not null && lfz.AvailTimes.Where(x => x == SlotID).Any() && (PriceFilter == null || lfz.PriceCat == PriceFilter))
                {
                    AvailableAircraft.Add(lfz.Id);
                }
            }
            return AvailableAircraft;
        }

        public static bool AvailableIn(int LFZID,int FTSID)
        {
            try
            {
                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(LFZID);
                return lfz is null ? throw new Exception("Lfz not found in database") : lfz.AvailTimes!.Any(x => x == FTSID);
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-GET] Can't test for aircraft availability: " + e,2);
                return false;
            }
        }
        public static bool AircraftFitsWeight(int LFZID,int Weight)
        {
            try
            {
                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(LFZID);
                return lfz is null ? throw new Exception("Lfz not found in database") : lfz.Seats >= Weight;
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-GET] Can't test for aircraft weight: " + e,2);
                return false;
            }
        }

        public static bool FlightFitsWeight(int LID,int LFZID,int Weight)
        {
            try
            {
                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(LFZID);
                List<Sheets.Target?>? tgt = RData.GetWhere<Sheets.Target>($"lid = {LID}");
                if (lfz is not null && tgt is not null)
                {
                    int curWeight = 0;
                    tgt.ForEach(x => curWeight += x?.Weight ?? 0);
                    if (curWeight + Weight <= lfz.Seats) return true;
                }
                return false;
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-GET] Can't test for remaining flight weight: " + e,2);
                return false;
            }
        }
        #endregion
        internal static async Task<(Func<Task<(int, DatabaseAction?)>>, (Sheets.Target, Sheets.Target))?> DatabaseNodeMove(XBlock Node,TargetStack Stack,int FreeingWeight,bool SuppressPopup)
        {
            //Persistency Check
            try
            {
                Sheets.Target? TargetNode = RData.Get<Sheets.Target>(Node.TargetID);

                if (!(TargetNode?.Persistent ?? false) && Node.Parent is TargetStack sourceContainer && !Stack.Id.Equals(sourceContainer.Id))
                {
                    bool transact = true;
                    int lid = -1;
                    Sheets.Flt? TargetFlt = RData.Get<Sheets.Flt>(TargetNode?.LId);
                    Sheets.Lfz? TargetLFZ = RData.Get<Sheets.Lfz>(Stack.LFZID);
                    Sheets.Slot? TargetSlot = RData.Get<Sheets.Slot>(Stack.SLTID);
                    //Avail Check
                    if (TargetSlot != null && TargetLFZ?.AvailTimes != null && TargetLFZ.AvailTimes.Contains(Stack.SLTID))
                    {
                        if (USettings.Instance.IgnoreTransactionLength || TargetSlot.Length == Node.Length)
                        {
                            Sheets.Flt? flt = RData.GetWhere<Sheets.Flt>($"slot = {Stack.SLTID}")?.Where(x => x?.Lfz == Stack.LFZID).FirstOrDefault();
                            if (flt != null)
                            {
                                //Flug vorhanden

                                if (USettings.Instance.IgnoreTransactionWeight || Manager.FlightFitsWeight(flt.Id,flt.Lfz,TargetNode?.Weight - FreeingWeight ?? USettings.Instance.DefaultTgtWeight)) lid = flt.Id;
                                else
                                {
                                    system.modals.ModalPush.Message(Lang.warning,Lang.message_too_much_weight);
                                    transact = false;
                                }
                            }
                            else
                            {
                                //Kein Flug vorhanden
                                if (!(USettings.Instance.IgnoreTransactionWeight || Manager.AircraftFitsWeight(Stack.LFZID,TargetNode?.Weight ?? USettings.Instance.DefaultTgtWeight)))
                                {
                                    system.modals.ModalPush.Message(Lang.warning,Lang.message_too_much_weight);
                                    transact = false;
                                }
                            }

                            if (!SuppressPopup && USettings.Instance.AskForNodeMove)
                            {
                                await system.modals.ModalPush.Question(Lang.security,Lang.nodemove_question_sub).ContinueWith(x =>
                                {
                                    if (!x.Result) transact = false;
                                });
                            }
                        }
                        else
                        {
                            system.modals.ModalPush.Message(Lang.warning,Lang.message_length_mismatch);
                            transact = false;
                        }
                    }

                    if (transact && TargetNode != null && GSettings.nav != null)
                    {
                        int newpc = await Drawer.AskForPriceUpdate(TargetNode.Price,TargetLFZ?.PriceCat,TargetNode.Name ?? "N/A");
                        if (newpc == 0) return null;

                        if (lid != -1)
                        {
                            //Datenbankaktion
                            ConProc.Log($"[XFLY-MANAGER] Target {Node.TargetID} will be transacted from flight {TargetNode.LId} to {lid}");

                            Tuple<int,DatabaseAction?> t = new(lid,null);

                            return (async () =>
                            {
                                await Task.Run(() =>
                                {
                                    if (RData.UpdateProperty<int>(Node.TargetID,lid,"LId",typeof(Sheets.Target)))
                                        RData.UpdateProperty<int>(Node.TargetID,newpc,"Price",typeof(Sheets.Target));
                                });
                                return new(lid,null);
                            }, (TargetNode, new Sheets.Target() { Id = Node.TargetID,LId = lid,Name = TargetNode.Name,Persistent = TargetNode.Persistent,Price = newpc,QuickTicket = TargetNode.QuickTicket,Weight = TargetNode.Weight }));
                        }
                        else
                        {
                            bool result = false;
                            await system.modals.ModalPush.Question(Lang.warning,Lang.newflt_warning).ContinueWith(t => result = t.Result);
                            if (result)
                            {
                                string? Adds = "";
                                byte Status = (byte)(GSettings.Status.Length - 1);
                                TargetCustomizer tc = new(null,new() { Status = TargetFlt?.Status ?? 0,Add = TargetFlt?.Add },true);
                                await GSettings.nav.PushModalAsync(tc);
                                await tc.ShowAndSelect().ContinueWith(r =>
                                {
                                    if (r.Result.Item2 == null)
                                    {
                                        result = false;
                                        return;
                                    }
                                    Adds = r.Result.Item2.Add;
                                    Status = r.Result.Item2.Status;
                                });
                                if (result)
                                {

                                    //Datenbankaktion awaitable machen. Xplan refresh und cleanup passieren vor oder gleichzeitig mit aktion
                                    return (async () =>
                                    {
                                        (Sheets.Flt, int) r = await Builder.CreateFlight(TargetNode.Weight,Adds,Status,TargetSlot?.Length,TargetNode.QuickTicket,0,Stack.LFZID,[Stack.SLTID]);
                                        if (r.Item1.Id != -1 && RData.UpdateProperty<int>(Node.TargetID,r.Item1.Id,"LId",typeof(Sheets.Target)))
                                        {
                                            RData.UpdateProperty<int>(Node.TargetID,newpc,"Price",typeof(Sheets.Target));
                                            ConProc.Log($"[XFLY-MANAGER] Flight {r.Item1.Id} was created and Target {Node.TargetID} will be transacted there from flight {TargetNode.LId}");
                                        }
                                        return (r.Item1.Id, new() { ActionID = 1,PreviousValue = r.Item1,DataType = typeof(Sheets.Flt),ObjectID = r.Item1.Id,ForeignKeyName = "LId" });
                                        //FLIGHT CLEANUP DB ACTION 
                                    }, (TargetNode, new Sheets.Target() { Id = Node.TargetID,LId = lid,Name = TargetNode.Name,Persistent = TargetNode.Persistent,Price = newpc,QuickTicket = TargetNode.QuickTicket,Weight = TargetNode.Weight }));
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-MANAGER] Node transaction could not be processed: " + e.Message,2);
                return null;
            }   
        }

        /// <summary>
        /// Initializes a new Linked Target. Returns Id=-1 on failure.
        /// </summary>
        internal static Sheets.Target CreateLinkedTarget(int lid,string name,int weight,int price,bool quick = false,bool persistent = false)
        {
            Sheets.Target t = new()
            {
                Name = name,
                Weight = weight,
                Price = FormatPrice(price),
                LId = lid,
                QuickTicket = quick,
                Persistent = persistent,
            };
            int resp = RData.Insert(t,typeof(Sheets.Target));
            if (resp != -1)
            {
                t.Id = resp;
                ConProc.Log($"[XFLY-MANAGER] Target {name} was created and linked to flight {lid}");
                return t;
            }
            return new() { Id = -1 };
        }

        /// <summary>
        /// Initializes a new Flight. Returns Id=-1 on failure.
        /// </summary>
        internal static Sheets.Flt CreateFlight(string? eId,int lfz,int slot,byte status,string? Add)
        {
            Sheets.Flt f = new()
            {
                EId = eId,
                Status = status,
                Slot = slot,
                Lfz = lfz,
                Add = Add,
            };
            int resp = RData.Insert(f,typeof(Sheets.Flt));
            if (resp != -1)
            {
                f.Id = resp;
                ConProc.Log($"[XFLY-MANAGER] Flight {eId ?? resp.ToString()} was created");
                return f;
            }        
            return new() { Id = -1 };
        }

        public static string? CreateEID()
        {
            return null;
        }

        //Determine für einen ganzen Slot statt einen Flug!!!
        public static void DetermineStatus(Sheets.Slot slt,List<Sheets.Flt?>? optionalflt,bool UseOGN)
        {
            try
            {
                List<Sheets.Flt?>? flts = optionalflt ?? RData.GetWhere<Sheets.Flt>($"slot={slt.Id}");
                if (flts == null)
                {
                    ConProc.Log("[XFLY-MANAGER] No flights to update");
                    return;
                }
                List<double> necessaryResyncs = [];
                foreach (Sheets.Flt? flt in flts)
                {
                    if (flt == null || flt.Status != 13) continue;
                    byte status = (byte)(slt.Delay ? 3 : 0);
                    DateTime now = DateTime.Now;
                    if (now >= slt.STime)
                    {
                        status = 2;
                        if (UseOGN)
                        {
                            (byte, int) online = OnlineManager.DetermineOnline(flt,slt);
                            necessaryResyncs.Add(online.Item2);
                            status = online.Item1;
                            //Neuen OGN Check ansetzen
                        }
                        else
                        {
                            if (now >= slt.FTime) status = 7;
                            else if (now >= slt.STime.Add(new TimeSpan(0,(int)double.Ceiling(((slt.FTime - slt.STime).TotalMinutes - slt.Length) / 2),0)))
                            {
                                necessaryResyncs.Add((slt.FTime - now).Add(TimeSpan.FromSeconds(5)).TotalMinutes);
                                status = 5;
                            }
                        }
                        //dpt/airborne/app/finished - OGN
                    }
                    //StatusChange(flt.Id,status,flt.Status,false);

                    if (GSettings.StatusLink.ContainsKey(flt.Id)) GSettings.StatusLink[flt.Id] = status;
                    else GSettings.StatusLink.TryAdd(flt.Id,status);
                }
                StatusRefresh();
                foreach (double resync in necessaryResyncs.Distinct().Where(x => x > 0))
                {
                    TimeServ.Schedule(DateTime.Now.AddMinutes(resync),() => DetermineStatus(slt,null,USettings.Instance.OGNStatus));
                }
            }
            catch (Exception ex)
            {
                ConProc.Log($"[XFLY-MANAGER] Determination of Slot {slt.Id} failed: {ex.Message}",2);
            }
        }

        internal static void StatusChange(int FltID,int prev,int status,bool dbupdate = true)
        {
            if (prev == 13 && status != 13) GSettings.StatusLink.Remove(FltID);
            else if (status == 13 && !GSettings.StatusLink.TryAdd(FltID,prev)) GSettings.StatusLink[FltID] = status;
            if (dbupdate) RData.UpdateProperty(FltID,(byte)status,"Status",typeof(Sheets.Flt));
            StatusRefresh();
            //Invoke Plan Update
            //Invoke Board Update
        }

        internal static void StatusRefresh()
        {
            List<Sheets.Flt> allFlt = RData.GetFlightTable();
            foreach (var fltcollector in XMain.FlightCollectors)
            {
                fltcollector.UpdateStatus(allFlt.Find(x => x.Id == fltcollector.FlightID)?.Status ?? 11);
            }
            //Xboard updaten
            BoardController.SynchronizeWithStatus(allFlt);
        }

        private static readonly List<(Action, DatabaseAction)> DelayActions = [];
        internal static void InitDelay(int slot,int minutes)
        {
            try
            {
                DateTime now = DateTime.Now;

                List<Sheets.Slot> slots = RData.GetSlotsTable();
                List<Sheets.Flt> flts = RData.GetFlightTable();
                //Aktuelle betroffene Flüge
                IEnumerable<Sheets.Flt> currentFLT = flts.Where(x => x.Slot == slot);
                //Aktuell betroffene Flugzeuge
                List<Sheets.Lfz> affectedAC = [.. RData.GetAircraftTable().Where(x => currentFLT.Select(x => x.Lfz).Contains(x.Id))];
                //Verzögerter Slot
                Sheets.Slot? delayed = slots.Find(x => x.Id == slot);
                //Copy für ActionStack
                Sheets.Slot? newdelay = Sheets.Clone<Sheets.Slot>(delayed);
                //Folgende Slots in Zeitlicher Reihenfolge
                if (delayed != null)
                {
                    List<Sheets.Slot> orderedSlots = [.. slots.Where(x => x.STime >= delayed.FTime).OrderBy(x => x.STime)];
                    DelaySlotBy(delayed,newdelay,orderedSlots,affectedAC,minutes,true);
                    foreach ((Action, DatabaseAction) da in DelayActions)
                    {
                        da.Item1.Invoke();
                    }
                    if (DelayActions.Count > 0) AutoAct.PushAction(null,[.. DelayActions.Select(x => x.Item2)]);
                }
            }
            catch (Exception ex)
            {
                ConProc.Log("[XFLY-MANAGER] Slot could not be delayed: " + ex.Message,2);
            }
        }

        internal static void DelaySlotBy(Sheets.Slot delayed,Sheets.Slot? copy,List<Sheets.Slot> orderedSlots,List<Sheets.Lfz> affectedAC,double minutes,bool init)
        {

            //double affected = minutes / USettings.Instance.DelayTolerance;

            if (copy != null)
            {
                if (orderedSlots.Count > 0 && minutes < USettings.Instance.MaxDelay)
                {
                    int tol = delayed.Delay ? 0 : USettings.Instance.DelayTolerance;
                    TimeSpan dlyspn = orderedSlots[0].STime - delayed.FTime;
                    double buff = dlyspn.TotalMinutes + tol;
                    copy.Delay = true;
                    copy.STime = copy.STime.AddMinutes(minutes);
                    System.Diagnostics.Debug.WriteLine($"Buffer {buff} - Delay {minutes}");
                    System.Diagnostics.Debug.WriteLine($"Buffer Slot ID {orderedSlots[0].Id} - Delayed Slot ID {delayed.Id}");
                    DelayActions.Add((() =>
                    {
                        RData.UpdateProperty<bool>(delayed.Id,true,"Delay",typeof(Sheets.Slot));
                        RData.UpdateProperty<DateTime>(delayed.Id,copy.STime,"STime",typeof(Sheets.Slot));
                    },
                    new() { ActionID = 3,DataType = typeof(Sheets.Slot),ObjectID = delayed.Id,PreviousValue = delayed,CurrentValue = copy }));
                    if (buff < minutes)
                    {
                        double newdly = minutes - buff;
                        copy.FTime = copy.FTime.AddMinutes(newdly);
                        DelayActions.Add((() =>
                        {
                            RData.UpdateProperty<DateTime>(delayed.Id,copy.FTime,"FTime",typeof(Sheets.Slot));
                        },
                            new() { ActionID = 3,DataType = typeof(Sheets.Slot),ObjectID = delayed.Id,PreviousValue = delayed,CurrentValue = copy }));
                        ConProc.Log($"[XFLY-MANAGER] Slot {delayed.Id} has been delayed by {minutes}");
                        if (orderedSlots.Count > 0)
                        {
                            DelaySlotBy(orderedSlots[0],Sheets.Clone(orderedSlots[0]),orderedSlots[1..],affectedAC,newdly,false);
                        }
                    }
                    else
                    {
                        //Kann in diesem Slot aufgefangen werden
                        if (init)
                        {
                            system.modals.ModalPush.Message(Lang.notification,Lang.delay_compensation);
                            ConProc.Log($"[XFLY-MANAGER] Slot {delayed.Id} has been delayed by {minutes}. No other slots were affected");
                        }

                    }
                }
                else
                {
                    //Keine Slots zu verschieben oder Delay zu groß
                    if (init)
                    {
                        system.modals.ModalPush.Message(Lang.notification,Lang.delay_error);
                        ConProc.Log($"[XFLY-MANAGER] Slot {delayed.Id} could not be delayed. The delay was outside of the boundaries or no slots were found",1);
                    }
                }
            }

        }
        public static int FormatPrice(int price) => price < 0 ? RData.Get<Sheets.PriceCat>(price * -1)?.Price ?? RData.Get<Sheets.PriceCat>(USettings.Instance.FallbackPriceCat)?.Price ?? 0 : price;
    }
}

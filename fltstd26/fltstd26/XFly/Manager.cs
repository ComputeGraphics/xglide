using CommunityToolkit.Maui.Converters;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.etc.online;
using fltstd26.Resources.Texts;
using fltstd26.system;
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
                if (!Now || slot.STime.TimeOfDay > (Quick ? DateTime.Now.AddMinutes(Quick ? -USettings.QuickTolerance : 0).TimeOfDay : DateTime.Now.TimeOfDay))
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
            Sheets.Target? TargetNode = RData.Get<Sheets.Target>(Node.TargetID);
            if (((!TargetNode?.Persistent) ?? false) && Node.Parent is TargetStack sourceContainer && !Stack.Id.Equals(sourceContainer.Id))
            {
                bool transact = true;
                int lid = -1;
                Sheets.Lfz? TargetLFZ = RData.Get<Sheets.Lfz>(Stack.LFZID);
                Sheets.Slot? TargetSlot = RData.Get<Sheets.Slot>(Stack.SLTID);
                //Avail Check
                if (TargetSlot != null && TargetLFZ?.AvailTimes != null && TargetLFZ.AvailTimes.Contains(Stack.SLTID))
                {
                    if (USettings.IgnoreTransactionLength || TargetSlot.Length == Node.Length)
                    {
                        Sheets.Flt? flt = RData.GetWhere<Sheets.Flt>($"slot = {Stack.SLTID}")?.Where(x => x?.Lfz == Stack.LFZID).FirstOrDefault();
                        if (flt != null)
                        {
                            //Flug vorhanden

                            if (USettings.IgnoreTransactionWeight || Manager.FlightFitsWeight(flt.Id,flt.Lfz,TargetNode?.Weight - FreeingWeight ?? USettings.DefaultTgtWeight)) lid = flt.Id;
                            else
                            {
                                if (!SuppressPopup) system.modals.ModalPush.Message(Lang.warning,Lang.message_too_much_weight);
                                transact = false;
                            }
                        }
                        else
                        {
                            //Kein Flug vorhanden
                            if (!(USettings.IgnoreTransactionWeight || Manager.AircraftFitsWeight(Stack.LFZID,TargetNode?.Weight ?? USettings.DefaultTgtWeight)))
                            {
                                if (!SuppressPopup) system.modals.ModalPush.Message(Lang.warning,Lang.message_too_much_weight);
                                transact = false;
                            }
                        }

                        if (!SuppressPopup && USettings.AskForNodeMove)
                        {
                            await system.modals.ModalPush.Question(Lang.security,Lang.nodemove_question_sub).ContinueWith(x =>
                            {
                                if (!x.Result) transact = false;
                            });
                        }
                    }
                    else
                    {
                        if (!SuppressPopup) system.modals.ModalPush.Message(Lang.warning,Lang.message_length_mismatch);
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
                        return (() =>
                        {
                            if (RData.UpdateProperty<int>(Node.TargetID,lid,"LId",typeof(Sheets.Target)))
                                RData.UpdateProperty<int>(Node.TargetID,newpc,"Price",typeof(Sheets.Target));
                            return Task.FromResult<(int, DatabaseAction?)>(new(lid,null));
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
                            TargetCustomizer tc = new(null,new(),true);
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
                return f;
            }
            return new() { Id = -1 };
        }

        public static string? CreateEID()
        {
            return null;
        }

        //Determine für einen ganzen Slot statt einen Flug!!!
        public static void DetermineStatus(Sheets.Slot slt,bool UseOGN)
        {
            List<byte> fltstatus = [];
            List<Sheets.Flt?>? flts = RData.GetWhere<Sheets.Flt>($"slot={slt.Id}");
            if (flts == null) return;
            foreach (Sheets.Flt? flt in flts)
            {
                if (flt == null) continue;
                byte status =(byte)(slt.Delay ? 3 : 0);
                if (DateTime.Now.TimeOfDay >= slt.STime.TimeOfDay)
                {
                    status = 2;

                    if (UseOGN)
                    {
                        (byte, int) online = OnlineManager.DetermineOnline(flt,slt);
                        status = online.Item1;
                    }
                    else
                    {
                        if (DateTime.Now.TimeOfDay >= slt.FTime.TimeOfDay) status = 7;
                        else if (DateTime.Now.TimeOfDay >= slt.STime.TimeOfDay.Add(new TimeSpan(0,(int)Double.Ceiling(((slt.FTime.TimeOfDay - slt.STime.TimeOfDay).TotalMinutes - slt.Length) / 2),0))) status = 5;
                    }
                    //dpt/airborne/app/finished - OGN
                }
                fltstatus.Add(status);

            }
        }


        public static int FormatPrice(int price) => price < 0 ? RData.Get<Sheets.PriceCat>(price * -1)?.Price ?? RData.Get<Sheets.PriceCat>(USettings.FallbackPriceCat)?.Price ?? 0 : price;
    }
}

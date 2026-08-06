using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using static SQLite.SQLite3;

namespace fltstd26.XFly
{
    internal static class Builder
    {
        public async static Task<bool> CreateTarget(string Definition,int TgtWeight,int? Length,int TgtPrice,bool Quick,bool Persistency,byte Status,string? Adds,int? LfzOverride = null,TimeSpan? TimeOverride = null,Sheets.Target? Relink = null)
        {
            try
            {
                if (!RData.Active()) throw new Exception("No Database");
                bool FlightCreate = false;
                IEnumerable<int>? OverrideSlotID = null;
                if (TimeOverride != null)
                {
                    DateTime? OverrideFormat = DateTime.Today + TimeOverride;
                    //DateTime? OverrideDateTime = OverrideFormat < DateTime.Now ? OverrideFormat + new TimeSpan(1,0,0,0) : OverrideFormat;
                    OverrideSlotID = RData.GetSlotsTable().Where(x => x.STime == OverrideFormat).Select(x => x.Id);
                }
                int FltLength = Length is null ? USettings.Instance.DefaultFltLength : Length.Value;
                List<Sheets.Flt> FitFlightWeight = Manager.FindFitWeightLength(FltLength,TgtWeight,USettings.Instance.AutoTimeCheck,Quick);
                if (FitFlightWeight.Count != 0)
                {
                    // -> Genug Flüge. Nutzer muss sich einen aussuchen
                    List<int> PriceList = [];
                    List<(string, string, string)> content = [("add.png", Lang.newflt, "")];
                    for(int i = 0; i < FitFlightWeight.Count; i++)
                    {
                        if (LfzOverride == null || LfzOverride == FitFlightWeight[i].Lfz)
                        {
                            if (OverrideSlotID == null || OverrideSlotID.Contains(FitFlightWeight[i].Slot))
                            {
                                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(FitFlightWeight[i].Lfz);
                                if (lfz != null)
                                {
                                    Sheets.PriceCat? pc = RData.Get<Sheets.PriceCat>(lfz.PriceCat);
                                    if (TgtPrice < 0 && pc?.Id != -TgtPrice)
                                    {
                                        FitFlightWeight.RemoveAt(i--);
                                        continue;
                                    }
                                    PriceList.Add(pc?.Id ?? -1);
                                    content.Add(("plane.png", $"{FitFlightWeight[i].EId ?? FitFlightWeight[i].Id.ToString()} ({RData.Get<Sheets.Slot>(FitFlightWeight[i].Slot)?.STime.ToShortTimeString() ?? "N/A"})", $"REG: {lfz?.Reg ?? "N/A"} PC: {pc?.Price} STATUS: {GSettings.Status[FitFlightWeight[i].Status]}"));
                                }
                            }
                        }
                    }

                    int TResult = -1;
                    await system.modals.ModalPush.Selector(Lang.select_flight,content).ContinueWith(t => TResult = t.Result);
                    if (TResult > 0)
                    {
                        int Price = TgtPrice == 0 ? (PriceList[TResult - 1] == -1 ? -USettings.Instance.FallbackPriceCat : -PriceList[TResult - 1]) : TgtPrice;
                        if (Relink != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Relinking directly");
                            DatabaseAction dba = new() { ActionID = 3,CurrentValue = new Sheets.Target() { Id = Relink.Id,LId = FitFlightWeight[TResult - 1].Id,Name = Relink.Name,Persistent = Relink.Persistent,Price = Relink.Price,QuickTicket = Relink.QuickTicket,Weight = Relink.Weight },PreviousValue = Relink,DataType = typeof(Sheets.Target),ObjectID = Relink.Id };
                            RData.UpdateProperty<int>(Relink.Id,FitFlightWeight[TResult - 1].Id,"LId", typeof(Sheets.Target));
                            //Nexus.Pass([dba]);
                            AutoAct.PushAction(dba);
                        }
                        else
                        {
                            Sheets.Target newtgt = Manager.CreateLinkedTarget(FitFlightWeight[TResult - 1].Id,Definition,TgtWeight,Price,Quick,Persistency);
                            /*Sheets.Target newtgt = new()
                            {
                                LId = FitFlightWeight[TResult - 1].Id,
                                Name = Definition,
                                Weight = TgtWeight,
                                Price = Price,
                                QuickTicket = Quick,
                                Persistent = Persistency
                            };*/
                            
                            DatabaseAction dba = new() { ActionID = 1,CurrentValue = null,PreviousValue = newtgt,DataType = typeof(Sheets.Target),ObjectID = newtgt.Id };
                            //int dbas = Nexus.PushSingle(dba);
                            AutoAct.PushAction(dba);
                        }
                    }
                    else if (TResult == -1)
                    {
                        //VOLLSTÄNDIGER ABBRUCH
                        ConProc.Log("[XFLY-BUILDER] The Target Creation System was cancelled by the user",1);
                        return false;
                    }
                    else if (TResult == 0)
                    {
                        FlightCreate = true;
                    }
                }
                else
                {
                    // -> Shit. Wir brauchen noch einen.
                    FlightCreate = true;
                }
                if (FlightCreate)
                {
                    bool result = false;
                    await system.modals.ModalPush.Question(Lang.warning,Lang.newflt_warning).ContinueWith(t => result = t.Result);
                    if (result)
                    {
                        await CreateFlight(TgtWeight,Adds,Status,FltLength,Quick,TgtPrice,LfzOverride,OverrideSlotID).ContinueWith(x =>
                        {
                            int Price = TgtPrice == 0 ? (x.Result.Item2 == -1 ? -USettings.Instance.FallbackPriceCat : -RData.Get<Sheets.Lfz>(x.Result.Item2)?.PriceCat ?? -USettings.Instance.FallbackPriceCat) : TgtPrice;
                            if (x.Result.Item1.Id != -1)
                            {
                                DatabaseAction da;
                                List<DatabaseAction> a = [];
                                if (Relink != null)
                                {
                                    System.Diagnostics.Debug.WriteLine("Relinking after new flight");
                                    //RData.UpdateProperty(Relink.Id,x.Result.Item1.Id,"LId",typeof(Sheets.Target));
                                    da = new()
                                    { 
                                        ActionID = 3,
                                        CurrentValue = new Sheets.Target() 
                                        { 
                                            Id = Relink.Id,
                                            LId = x.Result.Item1.Id,
                                            Name = Relink.Name,
                                            Persistent = Relink.Persistent,
                                            Price = Relink.Price,
                                            QuickTicket = Relink.QuickTicket,
                                            Weight = Relink.Weight 
                                        },
                                        PreviousValue = Relink,
                                        DataType = typeof(Sheets.Target),
                                        ObjectID = Relink.Id
                                    };
                                }
                                else
                                {
                                    Sheets.Target newtgt = Manager.CreateLinkedTarget(x.Result.Item1.Id,Definition,TgtWeight,Price,Quick,Persistency);
                                    /*Sheets.Target newtgt = new()
                                    {
                                        LId = x.Result.Item1.Id,
                                        Name = Definition,
                                        Weight = TgtWeight,
                                        Price = Price,
                                        QuickTicket = Quick,
                                        Persistent = Persistency
                                    };
                                    id = Nexus.PushSingle(da);
                                           */
                                    da = new()
                                    { 
                                        ActionID = 1,
                                        CurrentValue = null,
                                        PreviousValue = newtgt,
                                        DataType = typeof(Sheets.Target),
                                        ObjectID = newtgt.Id,
                                    };
                                    
                                }
                                a.Add(da);
                                a.Add(new() { ActionID = 1,CurrentValue = null,PreviousValue = x.Result.Item1,DataType = typeof(Sheets.Flt),ObjectID = x.Result.Item1.Id,LinkAction=da.ID, ForeignKeyName="LId" });
                                //Nexus.Pass(a);
                                AutoAct.PushAction(null,a);
                            }
                            //else throw new Exception("Flug konnte nicht erstellt werden");
                        });
                    }
                    else
                    {
                        ConProc.Log("[XFLY-BUILDER] The Flight Creation System was rejected by the user",1);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ConProc.Log("[XFLY-BUILDER] Error creating target: " + ex.Message,2);
                return false;
            }
        }

        // Gibt Tupel aus der Flight ID und der Luftfahrzeug ID zurück
        public async static Task<(Sheets.Flt, int)> CreateFlight(int Weight,string? InAdd,byte InStatus,int? Length,bool Quick,int PriceFilter,int? LfzOverride,IEnumerable<int>? OverrideSlots)
        {
            try
            {

                System.Diagnostics.Debug.WriteLine("Creating new flight. Length: " + Length.ToString() + ", Price: " + PriceFilter.ToString());
                int FltLength = Length is null ? USettings.Instance.DefaultFltLength : Length.Value;
                List<int> FitSlots = Manager.FindCompatibleSlots(FltLength,USettings.Instance.AutoTimeCheck ? DateTime.Now.AddMinutes(Quick ? -USettings.Instance.QuickTolerance : 0) : null);
                if (OverrideSlots != null) FitSlots = [.. FitSlots.Intersect(OverrideSlots)];
                if (FitSlots.Count != 0)
                {
                    System.Diagnostics.Debug.WriteLine("Checks for new flight passed");
                    //Alle Slots in denen Länge und Zeit passen
                    if (USettings.Instance.AutoASAP)
                    {
                        for (int i = 0; i < FitSlots.Count; i++)
                        {
                            HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt?.Lfz ?? -1)];
                            IEnumerable<int> FitAircraft = Manager.FindAvailableAircraft(FitSlots[i],!LfzOverride.HasValue,PriceFilter < 0 ? -PriceFilter : null,LfzOverride.HasValue).Where(x => Manager.AircraftFitsWeight(x,Weight)).Except(SlotFlights);
                            if (LfzOverride.HasValue) FitAircraft = [.. FitAircraft.Where(x => x == LfzOverride)];
                            //Kontingent geprüft
                            foreach (int Aircraft in FitAircraft)
                            {
                                Sheets.Flt res = Manager.CreateFlight(Manager.CreateEID(),Aircraft,FitSlots[i],InStatus,InAdd);
                                if (res.Id == -1) continue;
                                return (res, Aircraft);
                            }
                        }
                        return (new() { Id = -1 }, -1);
                    }
                    else
                    {
                        (Sheets.Flt, int) ProcResult = (new() { Id = -1 }, -1);
                        List<(string, string, string)> content = [("add.png", Lang.newobj, "")];
                        List<int> SelSlots = [];

                        //Testen, welche Slots für die Auswahl möglich wären
                        for (int i = 0; i < FitSlots.Count; i++)
                        {
                            List<int> SlotFlightsT = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt?.Lfz ?? -1)];
                            //SlotFlightsT.ForEach(x => System.Diagnostics.Debug.WriteLine("Slot FLT: " + x));
                            IEnumerable<int> FitAircraftT = Manager.FindAvailableAircraft(FitSlots[i],!LfzOverride.HasValue,PriceFilter < 0 ? -PriceFilter : null,LfzOverride.HasValue).Where(x => Manager.AircraftFitsWeight(x,Weight)).Except(SlotFlightsT);
                            //FitAircraftT.ForEach(x => System.Diagnostics.Debug.WriteLine("Fit AC: " + x));
                            if (FitAircraftT.Any())
                            {
                                Sheets.Slot SelSlot = RData.Get<Sheets.Slot>(FitSlots[i]) ?? new() { Id = -1 };
                                SelSlots.Add(FitSlots[i]);
                                content.Add(("slot.png", $"{SelSlot.STime.ToShortTimeString() ?? "N/A"} - {SelSlot.FTime.ToShortTimeString() ?? "N/A"}", $"({SelSlot.Length.ToString() ?? "N/A"}min)"));
                            }
                            System.Diagnostics.Debug.WriteLine($"{i}/{FitSlots.Count} processed");
                        }
                        content.ForEach(x => System.Diagnostics.Debug.WriteLine(x.Item1 + " " + x.Item2 + " " + x.Item3));

                        await system.modals.ModalPush.Selector(Lang.builder_selectSlot,content).ContinueWith(t =>
                        {
                            //System.Diagnostics.Debug.WriteLine($"Selected Index: {t.Result}");
                            if (t.Result > 0)
                            {
                                ProcResult.Item1.Id = t.Result;
                                //Flugzeug Auswählen
                            }
                            else if (t.Result == -1)
                            {
                                //ProcResult = (new() { Id = -1 }, -1);
                                //VOLLSTÄNDIGER ABBRUCH
                            }
                            else if (t.Result == 0) ProcResult = (new() { Id = -1 }, -1);
                        });
                        if (ProcResult.Item1.Id == -1) return ProcResult;
                        //System.Diagnostics.Debug.WriteLine("STOP 1 " + ProcResult);
                        int SelectedSlot = SelSlots[ProcResult.Item1.Id - 1];
                        List<(string, string, string)> accontent = [];
                        List<int> SelLfzs = [];
                        //System.Diagnostics.Debug.WriteLine("STOP 2");
                        HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={SelSlots[ProcResult.Item1.Id - 1]}") ?? []).Select(flt => flt?.Lfz ?? -1)];
                        IEnumerable<int> FitAircraft = Manager.FindAvailableAircraft(SelSlots[ProcResult.Item1.Id - 1],!LfzOverride.HasValue,PriceFilter < 0 ? -PriceFilter : null,LfzOverride.HasValue).Where(x => Manager.AircraftFitsWeight(x,Weight)).Except(SlotFlights);
                        if (LfzOverride.HasValue) FitAircraft = [.. FitAircraft.Where(x => x == LfzOverride)];
                        foreach (int ac in FitAircraft)
                        {
                            Sheets.Lfz SelLfz = RData.Get<Sheets.Lfz>(ac) ?? new() { Id = -1 };
                            SelLfzs.Add(ac);
                            accontent.Add(("plane.png", $"{SelLfz.Reg ?? "N/A"}", $"PC: {SelLfz.PriceCat} TYPE: {SelLfz.Type ?? "N/A"} SEATS: {SelLfz.Seats}"));
                        }
                        //System.Diagnostics.Debug.WriteLine("STOP 3");
                        await system.modals.ModalPush.Selector(Lang.builder_selectLfz,accontent).ContinueWith(t =>
                        {
                            if (t.Result >= 0)
                            {
                                Sheets.Flt f = Manager.CreateFlight(Manager.CreateEID(),SelLfzs[t.Result],SelectedSlot,InStatus,InAdd);
                                if(f.Id != -1) ProcResult = (f, SelLfzs[t.Result]);
                                
                            }
                            //else ProcResult = (new() { Id = -1 }, -1);
                        });

                        return ProcResult;
                    }
                }
                else
                {
                    //Keine passenden Slots. yikes. würd mir stinken
                    //Methode für Slot generierung todo
                    ConProc.Log("[XFLY-BUILDER] Flight could not be created due to missing slots",1);
                    system.modals.ModalPush.Message(Lang.warning,Lang.missing_slots_warning);
                    return (new() { Id = -1 }, -1);
                }
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-BUILDER] Flight could not be created: " + e.Message,1);
                return (new() { Id = -1 }, -1);
            }
        }
    }
}

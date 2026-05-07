using CommunityToolkit.Maui.Converters;
using fltstd26.core;
using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Text;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static SQLite.SQLite3;

namespace fltstd26.XFly
{
    internal static class Builder
    {
        public async static void CreateTarget(string Definition,int TgtWeight,int FltLength,int TgtPrice,bool Quick,bool Persistency,byte Status,string? Adds,int? LfzOverride = null,TimeSpan? TimeOverride = null)
        {
            try
            {
                if (!RData.Active) throw new Exception("Database not found");
                bool FlightCreate = false;
                IEnumerable<int>? OverrideSlotID = null;
                if (TimeOverride != null)
                {
                    DateTime? OverrideFormat = DateTime.Today + TimeOverride;
                    //DateTime? OverrideDateTime = OverrideFormat < DateTime.Now ? OverrideFormat + new TimeSpan(1,0,0,0) : OverrideFormat;
                    OverrideSlotID = RData.GetSlotsTable().Where(x => x.STime == OverrideFormat).Select(x => x.Id);
                }
                List<Sheets.Flt> FitFlightWeight = Manager.FindFitWeightLength(FltLength,TgtWeight,USettings.AutoTimeCheck,Quick);
                if (FitFlightWeight.Count != 0)
                {
                    // -> Genug Flüge. Nutzer muss sich einen aussuchen
                    List<int> PriceList = [];
                    List<(string, string, string)> content = [("add.png", Lang.newflt, "")];
                    foreach (Sheets.Flt i in FitFlightWeight)
                    {
                        if (LfzOverride == null || LfzOverride == i.Lfz)
                        {
                            if (OverrideSlotID == null || OverrideSlotID.Contains(i.Slot))
                            {
                                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(i.Lfz);
                                if (lfz != null)
                                {
                                    Sheets.PriceCat? pc = RData.Get<Sheets.PriceCat>(lfz.PriceCat);
                                    PriceList.Add(pc?.Id ?? -1);
                                    content.Add(("plane.png", $"{i.EId ?? i.Id.ToString()} ({RData.Get<Sheets.Slot>(i.Slot)?.STime.ToShortTimeString() ?? "N/A"})", $"REG: {lfz?.Reg ?? "N/A"} PC: {pc?.Price} STATUS: {GSettings.Status[i.Status]}"));
                                }
                            }
                        }
                    }

                    int TResult = -1;
                    await system.modals.ModalPush.Selector(Lang.select_flight,content).ContinueWith(t => TResult = t.Result);
                    if (TResult > 0)
                    {
                        int Price = TgtPrice == 0 ? (PriceList[TResult - 1] == -1 ? -USettings.FallbackPriceCat : -PriceList[TResult - 1]) : TgtPrice;
                        Manager.CreateLinkedTarget(FitFlightWeight[TResult - 1].Id,Definition,TgtWeight,Price,Quick,Persistency);
                        //TRIGGER XPLAN REFRESH
                    }
                    else if (TResult == -1)
                    {
                        //VOLLSTÄNDIGER ABBRUCH
                        throw new Exception("Target matching cancelled by user");
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
                        await CreateFlight(TgtWeight, Adds,Status,FltLength,Quick,LfzOverride,OverrideSlotID).ContinueWith(x =>
                        {
                            int Price = TgtPrice == 0 ? (x.Result.Item2 == -1 ? -USettings.FallbackPriceCat : -x.Result.Item2) : TgtPrice;
                            if (x.Result.Item1 != -1) Manager.CreateLinkedTarget(x.Result.Item1,Definition,TgtWeight,Price,Quick,Persistency);
                        });
                        //TRIGGER XPLAN REFRESH
                    }
                }
            }
            catch (Exception ex)
            {
                ConProc.Log("[BUILDER] Error building target: " + ex.Message,2);
            }
        }

        // Gibt Tupel aus der Flight ID und der Luftfahrzeug ID zurück --- WEIGHT CHECK MUSS IMPLEMENTIERT WERDEN!!!!!
        public async static Task<(int, int)> CreateFlight(int Weight, string? InAdd,byte InStatus,int FltLength,bool Quick,int? LfzOverride, IEnumerable<int>? OverrideSlots)
        {
            List<int> FitSlots = Manager.FindCompatibleSlots(FltLength,USettings.AutoTimeCheck ? DateTime.Now.AddMinutes(Quick ? -USettings.QuickTolerance : 0) : null);
            if(OverrideSlots != null) FitSlots = [.. FitSlots.Intersect(OverrideSlots)];
            if (FitSlots.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine("Creating new flight");
                //Alle Slots in denen Länge und Zeit passen
                if (USettings.AutoASAP)
                {
                    for (int i = 0; i < FitSlots.Count; i++)
                    {
                        HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt.Lfz)];
                        IEnumerable<int> FitAircraft = Manager.FindAvailableAircraft(FitSlots[i],!LfzOverride.HasValue).Where(x => Manager.FitsWeight(x, Weight)).Except(SlotFlights);
                        if (LfzOverride.HasValue) FitAircraft = [.. FitAircraft.Where(x => x == LfzOverride)];
                        //Kontingent geprüft
                        foreach (int Aircraft in FitAircraft)
                        {
                            int res = Manager.CreateFlight(Manager.CreateEID(),Aircraft,FitSlots[i],InStatus,InAdd);
                            if (res == -1) continue;
                            return (res, Aircraft);
                        }
                    }
                    return (-1,-1);
                }
                else
                {
                    (int, int) ProcResult = (-1,-1);
                    List<(string, string, string)> content = [("add.png", Lang.newobj, "")];
                    List<int> SelSlots = [];

                    //Testen, welche Slots für die Auswahl möglich wären
                    for (int i = 0; i < FitSlots.Count; i++)
                    {
                        List<int> SlotFlightsT = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt.Lfz)];
                        //SlotFlightsT.ForEach(x => System.Diagnostics.Debug.WriteLine("Slot FLT: " + x));
                        IEnumerable<int> FitAircraftT = Manager.FindAvailableAircraft(FitSlots[i],!LfzOverride.HasValue).Where(x => Manager.FitsWeight(x,Weight)).Except(SlotFlightsT);
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
                            ProcResult.Item1 = t.Result;
                            //Flugzeug Auswählen
                        }
                        else if (t.Result == -1)
                        {
                            ProcResult = (-1, -1);
                            //VOLLSTÄNDIGER ABBRUCH
                        }
                        else if (t.Result == 0) ProcResult = (-1, -1);
                    });

                    //System.Diagnostics.Debug.WriteLine("STOP 1 " + ProcResult);
                    int SelectedSlot = SelSlots[ProcResult.Item1 - 1];
                    List<(string, string, string)> accontent = [];
                    List<int> SelLfzs = [];
                    //System.Diagnostics.Debug.WriteLine("STOP 2");
                    HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={SelSlots[ProcResult.Item1 - 1]}") ?? []).Select(flt => flt.Lfz)];
                    IEnumerable<int> FitAircraft = Manager.FindAvailableAircraft(SelSlots[ProcResult.Item1 - 1],!LfzOverride.HasValue).Where(x => Manager.FitsWeight(x,Weight)).Except(SlotFlights);
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
                            ProcResult = (Manager.CreateFlight(Manager.CreateEID(),SelLfzs[t.Result],SelectedSlot,InStatus,InAdd), SelLfzs[t.Result]);
                        }
                        else ProcResult = (-1, -1);
                    });

                    return ProcResult;
                }
            }
            else
            {
                //Keine passenden Slots. yikes. würd mir stinken
                //Methode für Slot generierung todo
                return (-1, -1);
            }
        }


    }
}

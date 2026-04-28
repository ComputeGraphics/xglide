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
using System.Text;

namespace fltstd26.XFly
{
    internal static class Builder
    {
        public static void CreateTarget(string Definition,string Weight,string Length,string Price,bool Quick,bool Persistency,byte Status,string Adds)
        {
            try
            {
                if (!RData.Active) throw new Exception("Database not found");
                if (Definition == "") Definition = "N/A";
                int FltLength = Int32.TryParse(Length,out int ParseLength) ? ParseLength : USettings.DefaultFltLength;
                int TgtWeight = Int32.TryParse(Weight,out int ParseWeight) ? ParseWeight : USettings.DefaultTgtWeight;
                int TgtPrice = Int32.TryParse(Price,out int ParsePrice) ? ParseWeight : 0;

                List<Sheets.Flt> FitFlightWeight = Manager.FindFitWeightLength(FltLength,TgtWeight,true,Quick);
                if (FitFlightWeight.Count != 0)
                {
                    // -> Genug Flüge. Nutzer muss sich einen aussuchen
                    List<(string, string, string)> content = [("add.png", Lang.newflt, "")];
                    foreach (Sheets.Flt i in FitFlightWeight)
                    {
                        Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(i.Lfz);
                        content.Add(("plane.png", $"{i.EId ?? ""} ({RData.Get<Sheets.Slot>(i.Slot)?.STime.ToShortTimeString() ?? "N/A"})", $"REG: {lfz?.Reg ?? "N/A"} PC: {lfz?.PriceCat} STATUS: {GSettings.Status[i.Status]}"));
                    }
                    system.modals.ModalPush.Selector(Lang.select_flight,content).ContinueWith(t =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Selected Index: {t.Result}");
                        if (t.Result > 0)
                        {
                            Manager.CreateLinkedTarget(FitFlightWeight[t.Result - 1].Id,Definition,TgtWeight,TgtPrice,Quick,Persistency);
                            //TRIGGER XPLAN REFRESH
                        }
                        else if (t.Result == -1)
                        {
                            //VOLLSTÄNDIGER ABBRUCH
                            return;
                        }
                        else if (t.Result == 0)
                        {
                            int fltno = CreateFlight(Adds,Status,FltLength,Quick);
                            if(fltno != -1) Manager.CreateLinkedTarget(fltno,Definition,TgtWeight,TgtPrice,Quick,Persistency);
                            //TRIGGER XPLAN REFRESH
                        }
                    });
                }
                else
                {
                    // -> Shit. Wir brauchen noch einen. Erstmal Nutzer fragen (<--- EINFÜGEN)
                    system.modals.ModalPush.Question(Lang.warning,Lang.newflt_warning).ContinueWith(t =>
                    {
                        System.Diagnostics.Debug.WriteLine("Push finish");
                        if (t.Result)
                        {
                            int fltno = CreateFlight(Adds,Status,FltLength,Quick);
                            if (fltno != -1) Manager.CreateLinkedTarget(fltno,Definition,TgtWeight,TgtPrice,Quick,Persistency);
                            //TRIGGER XPLAN REFRESH
                        }
                    });
                    
                }
            }
            catch (Exception ex)
            {
                ConProc.Log("[BUILDER] Error building target: " + ex.Message,2);
            }
        }

        public static int CreateFlight(string InAdd,byte InStatus,int FltLength,bool Quick)
        {
            List<int> FitSlots = Manager.FindCompatibleSlots(FltLength,GSettings.AutoTimeCheck ? DateTime.Now.AddMinutes(Quick ? -USettings.QuickTolerance : 0) : null);
            if (FitSlots.Count != 0)
            {
                System.Diagnostics.Debug.WriteLine("Creating new flight");
                //Alle Slots in denen Länge und Zeit passen
                if (GSettings.AutoASAP)
                {
                    for (int i = 0; i < FitSlots.Count; i++)
                    {
                        HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt.Lfz)];
                        List<int> FitAircraft = Manager.FindAvailableAircraft(FitSlots[i],true);
                        if (FitAircraft.Count == 0) continue;

                        IEnumerable<int> FreeAircraft = FitAircraft.Except(SlotFlights);
                        //Kontingent geprüft
                        foreach (int Aircraft in FreeAircraft)
                        {
                            int res = Manager.CreateFlight(CreateEID(),Aircraft, FitSlots[i],InStatus,InAdd);
                            if (res == -1) continue;
                            return res;
                        }
                    }
                    return -1;
                }
                else
                {
                    int ProcResult = -1;
                    List<(string, string, string)> content = [("add.png", Lang.newobj, "")];
                    List<int> SelSlots = [];

                    //Testen, welche Slots für die Auswahl möglich wären
                    for (int i = 0; i < FitSlots.Count; i++)
                    {
                        HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={FitSlots[i]}") ?? []).Select(flt => flt.Lfz)];
                        List<int> FitAircraft = Manager.FindAvailableAircraft(FitSlots[i],true);
                        if(FitAircraft.Except(SlotFlights).Any())
                        {
                            Sheets.Slot SelSlot = RData.Get<Sheets.Slot>(FitSlots[i]) ?? new() { Id = -1 };
                            SelSlots.Add(FitSlots[i]);
                            content.Add(("slot.png", $"{SelSlot.STime.ToShortTimeString() ?? "N/A"} - {SelSlot.FTime.ToShortTimeString() ?? "N/A"}", $"({SelSlot.Length.ToString() ?? "N/A"}min)   {SelSlot.Id.ToString() ?? ""}"));
                        }
                        System.Diagnostics.Debug.WriteLine($"{i}/{FitSlots.Count} processed");
                    }
                    system.modals.ModalPush.Selector("TO FILL IN",content).ContinueWith(t =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Selected Index: {t.Result}");
                        if (t.Result > 0)
                        {
                            int SelectedSlot = SelSlots[t.Result-1];
                            List<(string, string, string)> accontent = [];
                            List<int> SelLfzs = [];

                            HashSet<int> SlotFlights = [.. (RData.GetWhere<Sheets.Flt>($"slot={SelSlots[t.Result-1]}") ?? []).Select(flt => flt.Lfz)];
                            List<int> FitAircraft = Manager.FindAvailableAircraft(SelSlots[t.Result-1],true);
                            IEnumerable<int> AvailableAircraft = FitAircraft.Except(SlotFlights);
                            foreach(int ac in AvailableAircraft)
                            {
                                Sheets.Lfz SelLfz = RData.Get<Sheets.Lfz>(ac) ?? new() { Id = -1 };
                                SelLfzs.Add(ac);
                                accontent.Add(("plane.png", $"{SelLfz.Reg ?? "N/A"}", $"PC: {SelLfz.PriceCat} TYPE: {SelLfz.Type} SEATS: {SelLfz.Seats} AUTO: {SelLfz.AutoAssign}"));
                            }
                            system.modals.ModalPush.Selector("TO FILL IN",accontent).ContinueWith(t =>
                            {
                                if (t.Result >= 0)
                                {
                                    ProcResult = Manager.CreateFlight(CreateEID(),SelLfzs[t.Result],SelectedSlot,InStatus,InAdd);
                                }
                                else ProcResult = - 1;
                            });
                                //Flugzeug Auswählen
                        }
                        else if (t.Result == -1)
                        {
                            ProcResult = -1;
                            //VOLLSTÄNDIGER ABBRUCH
                        }
                        else if (t.Result == 0) ProcResult = -1;
                    });
                    return ProcResult;
                }
            }
            else
            {
                //Keine passenden Slots. yikes
                //Methode für Slot generierung todo
                return -1;
            }
        }

        public static string CreateEID()
        {
            return "N/A";
        }
    }
}

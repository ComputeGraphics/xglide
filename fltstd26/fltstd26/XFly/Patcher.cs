using fltstd26.core;
using fltstd26.etc;
using fltstd26.etc.online;
using fltstd26.Resources.Texts;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.XFly
{
    internal class Patcher
    {
        internal async static Task<bool> GeneralInspection()
        {
            await RelinkTargets();
            TestOverload();
            CleanupFlights();
            TestNight();
            TestOverlap();
            return true;
        }

        internal static List<Sheets.Flt> CleanupFlights(List<int>? ExcludeTickets = null)
        {
            IEnumerable<int> UnusedFlight = RData.GetFlightTable().Select(x => x.Id).Except(RData.GetTargetTable().Where(x => !ExcludeTickets?.Contains(x.Id) ?? true).Select(x => x.LId).Distinct());
            List<Sheets.Flt> flts = [];
            foreach (int flight in UnusedFlight)
            {
                Sheets.Flt? flt = RData.Get<Sheets.Flt>(flight);
                if (flt != null)
                {
                    flts.Add(flt);
                    RData.Delete(flight,typeof(Sheets.Flt));
                }

            }
            return flts;
        }

        //Überschneidende Flüge?
        //Flüge nach oder vor dämmerung?
        internal async static Task<bool> RelinkTargets()
        {
            if (GSettings.nav == null) return false;
            IEnumerable<Sheets.Target>? Unlinks = RData.GetTargetTable().Where(x => !RData.GetFlightTable().Select(x => x.Id).Contains(x.LId));
            if (Unlinks != null && Unlinks.Any())
            {
                bool question = false;
                await system.modals.ModalPush.Question(Lang.warning,Lang.unlinked_tgts).ContinueWith(x => question = x.Result);
                if (question)
                {
                    foreach (Sheets.Target target in Unlinks)
                    {
                        //Ask for new length
                        ConProc.Log("[PATCHER] Ziel " + target.Id + " wird ein neuer Flug zugeordnet",1);
                        int Length = USettings.Instance.DefaultFltLength;
                        byte Status = 13;
                        string? Adds = null;
                        bool success = false;
                        TargetCustomizer tc = new(null,new() { Status = Status,EId = Length.ToString(),Slot = Length},false,true,target.Name ?? "N/A");
                        await GSettings.nav.PushModalAsync(tc);
                        await tc.ShowAndSelect().ContinueWith(r =>
                        {
                            if (r.Result.Item2 == null) return;
                            Adds = r.Result.Item2.Add;
                            Status = r.Result.Item2.Status;
                            Length = r.Result.Item2.Slot;
                            success = true;
                        });

                        if(success) await Builder.CreateTarget(target.Name ?? "N/A",target.Weight,Length,target.Price,target.QuickTicket,target.Persistent,Status,Adds,null,null,target);
                    }
                }
            }
            return true;
        }

        internal static void TestOverload()
        {
            foreach(Sheets.Flt flight in RData.GetFlightTable())
            {
                int? LfzCapacity = RData.Get<Sheets.Lfz>(flight.Lfz)?.Seats;
                if (LfzCapacity == null) continue;
                List<Sheets.Target?>? LinkedTargets = RData.GetWhere<Sheets.Target>($"lid={flight.Id}");
                if (LinkedTargets == null) continue;
                if(LinkedTargets.Count > LfzCapacity)
                {
                    ConProc.Log("[PATCHER] Flug " + flight.Id + " überfüllt",1);
                    if (GSettings.nav == null) return;
                    system.modals.ModalPush.Message(flight.EId ?? flight.Id.ToString(),Lang.flt_overflow);
                }
            }
        }

        internal static void TestNight()
        {
            TimeSpan? bgl_dawn = OGN.FormatTime(OGN.CurrentOGN.airfield?.time_info?.dawn);
            TimeSpan? bgl_dusk = OGN.FormatTime(OGN.CurrentOGN.airfield?.time_info?.twilight);
            System.Diagnostics.Debug.WriteLine("Current OGN Dawn: " + bgl_dawn ?? "N/A");
            System.Diagnostics.Debug.WriteLine("Current OGN Dusk: " + bgl_dusk ?? "N/A");
            if (bgl_dusk == null || bgl_dawn == null) return;
            foreach(Sheets.Slot slot in RData.GetSlotsTable())
            {
                if (slot.STime.TimeOfDay > bgl_dusk || slot.FTime.TimeOfDay < bgl_dawn) system.modals.ModalPush.Message(Lang.warning,$"Slot {slot.Id} " + Lang.too_late);
                //if(slot.STime > )
            }
        }
        internal static void TestOverlap()
        {
            List<Sheets.Slot> slots = [..RData.GetSlotsTable().OrderBy(x => x.STime)];
            string overlaps = "";
            for(int i = 0; i < slots.Count; i++)
            {
                if (i+1 < slots.Count && slots[i].FTime > slots[i+1].STime) overlaps += $"Slot {slots[i].Id} ({slots[i].STime.ToShortTimeString()}-{slots[i].FTime.ToShortTimeString()}) -!- {slots[i+1].Id} ({slots[i].STime.ToShortTimeString()}-{slots[i].FTime.ToShortTimeString()}) \r\n";
            }

            system.modals.ModalPush.Message(Lang.warning,overlaps + Lang.overlap_warning);
        }
    }
}

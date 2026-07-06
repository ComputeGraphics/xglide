using fltstd26.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.etc.online
{
    internal static class OnlineManager
    {
        // (Status, Sync-Code) -> Sync Code: -1 - kein sync, > 0 - Sync in Sync-Code * Standard Intervall
        internal static (byte, int) DetermineOnline(Sheets.Flt flt, Sheets.Slot slt)
        {
            byte status = 11;
            int nextCheck = -1;
            Sheets.Lfz? ac = RData.Get<Sheets.Lfz>(flt.Lfz);
            if (ac != null)
            {
                List<OGN.Device>? devices = OGN.CurrentOGN.devices;
                if (devices != null)
                {
                    int dev = devices.FindIndex(x => x.address == ac.OGN);
                    if (dev != -1 && OGN.CurrentOGN.flights != null)
                    {
                        OGN.Flight? ognflt = OGN.CurrentOGN.flights.Where(x => x.device == dev).Where(x => MatchesSlot(x.start,slt.STime,slt.FTime)).OrderBy(x => OGN.FormatTime(x.start)).FirstOrDefault();
                        if(ognflt == null)
                        {
                            nextCheck = 1;
                            //Erneut in einer minute prüfen
                        }
                        else
                        {
                            //Erneuter scan in 2 min
                            TimeSpan? to = OGN.FormatTime(ognflt.start);
                            if(to.HasValue)
                            {
                                if (ognflt.stop == null)
                                {
                                    //Flug noch im Gange
                                    status = (byte)(DateTime.Now.TimeOfDay < to.Value.Add(TimeSpan.FromMinutes(USettings.TakeoffDuration)) ? 4 : 5);
                                    nextCheck = (int)Double.Ceiling(slt.Length - (DateTime.Now.TimeOfDay - to.Value).TotalMinutes - 1);
                                    if(nextCheck < 1) nextCheck = 1;
                                }
                                else
                                {
                                    status = (byte)(DateTime.Now.TimeOfDay < slt.FTime.TimeOfDay ? 6 : 7);
                                    nextCheck = -1;
                                }
                            }

                        }
                    }
                    else
                    {
                        //Keine Zuweisung!!
                        nextCheck = 1;
                    }
                    //OGN Zuweisung
                    //Wenn Flugzeug nicht vorhanden LinkAddress(false) ausführen!


                    //Mit OGN:
                    //Slot anfang nicht enthalten -> erneuter OGN Sync in 1 min sonst nichts
                    //Wenn alle Startzeit vorhanden -> erneuter OGN Sync in 2 min -> 4
                    //Nach 2 min -> 5
                    //5 Minuten vor geplanter Landezeit -> jede Minute OGN Sync
                    //Wenn alle landezeit -> Keine OGN Checks mehr -> 6
                    //Wenn Slot Vorbei -> 7

                    //bis 2 Minuten nach erscheinen -> 4
                    //bis Landung -> 5
                    //ab Landung -> 6
                    //ab Slot ende -> 7


                    //devices.FindIndex(x => x.registration == ac.Reg)
                }
            }
            return (status, nextCheck);
            
        }

        private static bool MatchesSlot(string? ogn_start,DateTime slot_start,DateTime? slot_end)
        {
            TimeSpan? ogn = OGN.FormatTime(ogn_start);
            TimeSpan tol = TimeSpan.FromMinutes(USettings.OGNTolerance);
            return ogn != null && (ogn.Value > slot_start.TimeOfDay.Subtract(tol) && (slot_end == null || ogn.Value < slot_end.Value.TimeOfDay.Add(tol)));
        }

    }
}

using fltstd26.core;
using fltstd26.etc;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.XFly
{
    internal class Manager
    {

        #region Checks

        /// <summary>
        /// Find all existing flights, that can take the required weight
        /// </summary>
        public static List<int> FindFitWeight(int Weight, bool Now, bool Quick)
        {
            List<int> flts = [];
            foreach (Sheets.Flt flt in RData.GetFlightTable())
            {
                Sheets.Slots? slot = RData.Get<Sheets.Slots>(flt.Slot);
                if (!Now || (slot is not null && slot.STime > (Quick ? DateTime.Now.Subtract(new TimeSpan(0,GSettings.QuickVolume,0)) : DateTime.Now)))
                {
                    Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(flt.Lfz);
                    if (lfz is not null)
                    {
                        int curWeight = 0;
                        RData.Handler!.GetTargetsByLink(flt.Id).ForEach(x => curWeight += x.Weight);
                        if (curWeight + Weight <= lfz.Seats) flts.Add(flt.Id);
                    }
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
                    if (times is not null)
                    {
                        if(slot.STime >= times) CompatibleSlots.Add(slot.Id);
                    }
                    else
                    {
                        CompatibleSlots.Add(slot.Id);
                    }
                }
            }
            return CompatibleSlots;
        }

        public static List<int> FindAvailableAircraft(int SlotID,bool auto)
        {
            List<int> AvailableAircraft = [];
            foreach (Sheets.Lfz lfz in RData.GetAircraftTable())
            {
                if (lfz.AvailTimes is not null && lfz.AvailTimes.Select(x => x == SlotID).FirstOrDefault())
                {
                    AvailableAircraft.Add(lfz.Id);
                }
            }
            return AvailableAircraft;
        }



        #endregion


        /// <summary>
        /// Initializes a new Target
        /// </summary>
        public static Types.TGT CreateTarget(string name,int weight,int price,bool quick = false,bool persistent = false)
        {
            return new()
            {
                Name = name,
                Weight = weight,
                Price = price < 0 ? USettings.PriceCategories[price * -1].Item2 : price,
                QuickTicket = quick,
                Persistent = persistent,
            };
        }

        /// <summary>
        /// Initializes a new Slot
        /// </summary>


    }
}

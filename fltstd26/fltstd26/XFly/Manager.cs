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
        public static List<Sheets.Flt> FindFitWeightLength(int Length, int Weight,bool Now,bool Quick)
        {
            List<Sheets.Flt> flts = [];
            foreach (Sheets.Flt flt in RData.GetFlightTable())
            {
                Sheets.Slot? slot = RData.Get<Sheets.Slot>(flt.Slot);
                if (slot is null || slot.Length != Length) continue;
                if (!Now || slot.STime > (Quick ? DateTime.Now.AddMinutes(Quick ? -USettings.QuickTolerance : 0) : DateTime.Now))
                {
                    Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(flt.Lfz);
                    List<Sheets.Target>? tgt = RData.GetWhere<Sheets.Target>($"lid = {flt.Id}");
                    if (lfz is not null && tgt is not null)
                    {
                        int curWeight = 0;
                        tgt.ForEach(x => curWeight += x.Weight);
                        if (curWeight + Weight <= lfz.Seats) flts.Add(flt);
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
                    if (times is null || slot.STime >= times) CompatibleSlots.Add(slot.Id);
                }
            }
            return CompatibleSlots;
        }

        public static List<int> FindAvailableAircraft(int SlotID, bool Auto)
        {
            List<int> AvailableAircraft = [];
            foreach (Sheets.Lfz lfz in RData.GetAircraftTable())
            {
                if ((!Auto || lfz.AutoAssign == true) && lfz.AvailTimes is not null && lfz.AvailTimes.Where(x => x == SlotID).Any())
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
                ConProc.Log("[XFLY-GET] Can't test for Aircraft availability: " + e,2);
                return false;
            }
        }

        public static bool FitsWeight(int LFZID, int Weight)
        {
            try
            {
                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(LFZID);
                return lfz is null ? throw new Exception("Lfz not found in database") : lfz.Seats >= Weight;
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY-GET] Can't test for Aircraft weight: " + e,2);
                return false;
            }
        }
        #endregion


        /// <summary>
        /// Initializes a new Target (Not saved to database)
        /// </summary>
        public static Sheets.Target CreateTarget(string name,int weight,int price,bool quick = false,bool persistent = false)
        {
            Sheets.Target t = new()
            {
                Name = name,
                Weight = weight,
                Price = price < 0 ? RData.Get<Sheets.PriceCat>(price * -1)?.Price ?? RData.Get<Sheets.PriceCat>(USettings.FallbackPriceCat)?.Price ?? 0 : price,
                QuickTicket = quick,
                Persistent = persistent,
            };
            return t;
        }

        /// <summary>
        /// Initializes a new Linked Target. Returns Id=-1 on failure.
        /// </summary>
        public static int CreateLinkedTarget(int lid,string name,int weight,int price,bool quick = false,bool persistent = false)
        {
            Sheets.Target t = new()
            {
                Name = name,
                Weight = weight,
                Price = price < 0 ? RData.Get<Sheets.PriceCat>(price * -1)?.Price ?? RData.Get<Sheets.PriceCat>(USettings.FallbackPriceCat)?.Price ?? 0: price,
                LId = lid,
                QuickTicket = quick,
                Persistent = persistent,
            };
            int resp = RData.Insert(t);
            if (resp != -1) return resp;
            return -1;
        }

        /// <summary>
        /// Initializes a new Flight. Returns Id=-1 on failure.
        /// </summary>
        public static int CreateFlight(string? eId,int lfz,int slot,byte status,string? Add)
        {
            Sheets.Flt f = new()
            {
                EId = eId,
                Status = status,
                Slot = slot,
                Lfz = lfz,
                Add = Add,
            };
            int resp = RData.Insert(f);
            if (resp != -1) return resp;
            return -1;
        }

        public static string? CreateEID()
        {
            return null;
        }
    }
}

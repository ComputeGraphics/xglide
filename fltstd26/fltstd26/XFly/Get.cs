using fltstd26.core;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.XFly
{
    public static class Get
    {
        public static bool AvailableIn(int LFZID, int FTSID)
        {
            try
            {
                Sheets.Lfz? lfz = RData.Get<Sheets.Lfz>(LFZID);
                return lfz is null ? throw new Exception("Lfz not found in database") : lfz.AvailTimes!.Any(x => x == FTSID);
            }
            catch (Exception e)
            {
                ConProc.Log("[XFLY.GET] Can't test for Aircraft availability: " + e,2);
                return false;
            }
        }


    }
}

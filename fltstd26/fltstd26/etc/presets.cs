using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fltstd26.core;
using fltstd26.etc;
using static fltstd26.etc.Types;

namespace fltstd26.etc
{
    class Presets
    {
        public static void WriteSample()
        {
            USettings.allLFZ.Add(new LFZ { Reg = "D-ABCD",Type = "Cessna 172",Seats = 4,AutoAssign = true,Interval = 15,PriceCat = 0 });
            USettings.allLFZ.Add(new LFZ { Reg = "D-EFGH",Type = "Piper PA-28",Seats = 4,AutoAssign = true,Interval = 15,PriceCat = 0 });
            USettings.allLFZ.Add(new LFZ { Reg = "D-IJKL",Type = "P-17 Stearman",Seats = 2,AutoAssign = false,Interval = 15,PriceCat = 0 });
            USettings.allLFZ.Add(new LFZ { Reg = "D-MNOP",Type = "WT9 Dynamic",Seats = 2,AutoAssign = true,Interval = 15,PriceCat = 0 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(8),End = DateTime.Today.AddHours(9),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(9),End = DateTime.Today.AddHours(10),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(10),End = DateTime.Today.AddHours(11),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(11),End = DateTime.Today.AddHours(12),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(12),End = DateTime.Today.AddHours(13),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(13),End = DateTime.Today.AddHours(14),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(14),End = DateTime.Today.AddHours(15),Length = 15 });
            USettings.allFTS.Add(new FTS { Start = DateTime.Today.AddHours(15),End = DateTime.Today.AddHours(16),Length = 15 });


            List<int> FTSIDs = RData.rdbsys!.InsertSlotT(USettings.allFTS);
            List<int> LFZIDs = RData.rdbsys!.InsertAircraftT(USettings.allLFZ);
            USettings.allFTS.ForEach(x => x.Id = FTSIDs[USettings.allFTS.IndexOf(x)]);
            USettings.allLFZ.ForEach(x => x.Id = LFZIDs[USettings.allLFZ.IndexOf(x)]);
            for (int i = 0; i < USettings.allFTS.Count; i++)
            {
                
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fltstd26.core;
using fltstd26.etc;

namespace fltstd26.etc
{
    class Presets
    {
        public static void WriteSample()
        {
            List<Types.LFZ> allLFZ = [];
            List<Types.FTS> allFTS = [];
            List<Sheets.PriceCat> allPriceCat = [];

            //Price Cats
            allPriceCat.Add(new Sheets.PriceCat { Name = "Standard", Price = 3500 });
            allPriceCat.Add(new Sheets.PriceCat { Name = "Stearman",Price = 7500 });
            allPriceCat.Add(new Sheets.PriceCat { Name = "VIP",Price = 45000 });

            //LFZ
            allLFZ.Add(new Types.LFZ { Reg = "D-ABCD",Type = "Cessna 172",Seats = 4,AutoAssign = true,Interval = 15,PriceCat = 0,AvailTimes = [1,2,3,4,5,6] });
            allLFZ.Add(new Types.LFZ { Reg = "D-EFGH",Type = "Piper PA-28",Seats = 4,AutoAssign = true,Interval = 15,PriceCat = 0,AvailTimes = [2,4,6] });
            allLFZ.Add(new Types.LFZ { Reg = "D-IJKL",Type = "P-17 Stearman",Seats = 2,AutoAssign = false,Interval = 15,PriceCat = 1,AvailTimes = [1,3,5] });
            allLFZ.Add(new Types.LFZ { Reg = "D-MNOP",Type = "WT9 Dynamic",Seats = 2,AutoAssign = true,Interval = 15,PriceCat = 0,AvailTimes = [1,2,3,4,5,6] });

            //FTS
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(8),End = DateTime.Today.AddHours(9),Length = 15 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(9),End = DateTime.Today.AddHours(10),Length = 30 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(10),End = DateTime.Today.AddHours(11),Length = 15 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(11),End = DateTime.Today.AddHours(12),Length = 30 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(12),End = DateTime.Today.AddHours(13),Length = 15 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(13),End = DateTime.Today.AddHours(14),Length = 30 });
            allFTS.Add(new Types.FTS { Start = DateTime.Today.AddHours(14),End = DateTime.Today.AddHours(15),Length = 15 });

            //Upload
            RData.Handler?.db.InsertAll(allPriceCat,true);
            RData.Handler?.InsertAircraftT(allLFZ);
            RData.Handler?.InsertSlotT(allFTS);

        }
    }
}

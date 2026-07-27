using fltstd26.etc;
using fltstd26.Resources.Texts;
using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace fltstd26.core
{
    internal static class Ark
    {
        private readonly static List<DataColumn> TargetColumns = [
            new DataColumn(Lang.xplan_name),
            new DataColumn(Lang.xplan_quickticket.Replace(".",String.Empty)),
            new DataColumn(Lang.fltno),
            new DataColumn(Lang.status),
            new DataColumn(Lang.lfzreg),
            new DataColumn(Lang.type),
            new DataColumn(Lang.time),
            new DataColumn(Lang.xplan_length),
            new DataColumn(Lang.delay),
            //Hier einfügen ^3
            new DataColumn(Lang.xplan_weight),
            new DataColumn(Lang.xplan_price),
            new DataColumn(Lang.price_sum),
        ];

        private readonly static List<DataColumn> AircraftColumns = [
            new DataColumn(Lang.lfzreg),
            new DataColumn(Lang.type),
            new DataColumn(Lang.xplan_weight),
            new DataColumn(Lang.interval),
            new DataColumn(Lang.autoassign),
            new DataColumn(Lang.ogn_address), 
            new DataColumn(Lang.flight_sum),
            new DataColumn(Lang.targets_transported),
            new DataColumn(Lang.weight_transported),
            new DataColumn(Lang.xplan_price),
            new DataColumn(Lang.turnover),
            new DataColumn(Lang.activity_range),
            new DataColumn(Lang.length_sum),
        ];
        public static async void SumTargets(string file)
        {
            TargetColumns.InsertRange(9,[.. USettings.Instance.Additionals.Select(x => new DataColumn(x))]);
            //Target: Name,Quickticket,Flugnummer,Letzter Status,Flugzeug,Type,STime-FTime,Length,Delay,[Adds],Weight,Price,Weight*Price
            //Price * Weight = All
            //DataSet ticketsum = new(DateTime.Now.ToString("R"));
            DataTable table = new("Tickets");
            table.Columns.AddRange([.. TargetColumns]);
            //ticketsum.Tables.Add(table);

            List<Sheets.Flt> flights = RData.GetFlightTable();
            List<Sheets.Lfz> aircrafts = RData.GetAircraftTable();
            List<Sheets.Target> targets = RData.GetTargetTable();
            List<Sheets.Slot> slots = RData.GetSlotsTable();

            DataRow row;
            int[] summaries = new int[3];

            foreach (Sheets.Target tgt in targets)
            {
                System.Diagnostics.Debug.WriteLine(table.Columns.Count.ToString() + " Columns in the summary");
                row = table.NewRow();
                row[0] = tgt.Name;
                row[1] = tgt.QuickTicket ? "P" : "N";

                Sheets.Flt? flt = flights.Find(x => x.Id == tgt.LId);
                row[2] = flt?.EId ?? flt?.Id.ToString() ?? "N/A";
                row[3] = flt?.Status != 13 ? GSettings.Status[flt?.Status ?? 11] ?? "N/A" : (GSettings.StatusLink.TryGetValue(flt.Id,out int status) ? GSettings.Status[status] : "N/A");

                Sheets.Lfz? lfz = aircrafts.Find(x => x.Id == flt?.Lfz);
                row[4] = lfz?.Reg ?? "N/A";
                row[5] = lfz?.Type ?? "N/A";

                Sheets.Slot? slt = slots.Find(x => x.Id == flt?.Slot);
                row[6] = $"{slt?.STime:G} - {slt?.FTime:G}";
                row[7] = slt?.Length.ToString() ?? "N/A";
                row[8] = slt?.Delay ?? false ? "P" : "N";

                if (flt?.Add != null)
                {
                    string[] adds = flt.Add.Split(';');
                    for (int i = 0; i < USettings.Instance.Additionals.Count; i++)
                    {
                        row[i + 9] = adds.Length > i ? adds[i] : "N/A";
                    }
                }
                int price = tgt.Price < 0 ? RData.Get<Sheets.PriceCat>(-tgt.Price)?.Price ?? 0 : tgt.Price;
                row[TargetColumns.Count - 3] = tgt.Weight;
                summaries[0] += tgt.Weight;
                row[TargetColumns.Count - 2] = GSettings.UnformatPrice(price);
                summaries[1] += price;
                row[TargetColumns.Count - 1] = GSettings.UnformatPrice(tgt.Weight * price);
                summaries[2] += price * tgt.Weight;
                table.Rows.Add(row);
            }

            row = table.NewRow();
            row[0] = Lang.full;
            row[2] = $"{Lang.targets_sold}: {targets.Sum(x => x.Weight)}";
            row[3] = $"{Lang.total_transactions}: {targets.Count}";
            row[TargetColumns.Count - 3] = summaries[0];
            row[TargetColumns.Count - 2] = GSettings.UnformatPrice(summaries[1]);
            row[TargetColumns.Count - 1] = GSettings.UnformatPrice(summaries[2]);
            table.Rows.Add(row);
            //XmlSerializer xml = new(typeof(DataSet));

            //MemoryStream writer = new(Encoding.Default.GetBytes(ticketsum.GetXml()));
            MemoryStream writer = new();
            table.WriteXml(writer,XmlWriteMode.IgnoreSchema);
            writer.Position = 0;
            //xml.Serialize(writer,ticketsum);
            await DskMan.AutoPermSaveFile(writer,file);
            writer.Close();

        }

        public static async void SumAircraft(string file)
        {
            DataTable table = new("Aircraft");
            table.Columns.AddRange([.. AircraftColumns]);

            List<Sheets.Flt> flights = RData.GetFlightTable();
            List<Sheets.Lfz> aircrafts = RData.GetAircraftTable();
            List<Sheets.Target> targets = RData.GetTargetTable();
            List<Sheets.PriceCat> prices = RData.GetPriceTable();
            List<Sheets.Slot> slots = RData.GetSlotsTable();

            DataRow row;
            int[] summaries = new int[3];

            foreach (Sheets.Lfz ac in aircrafts)
            {
                row = table.NewRow();

                IEnumerable<Sheets.Flt> acflts = flights.Where(x => x.Lfz == ac.Id);
                IEnumerable<Sheets.Target> actgts = targets.Where(x => acflts.Any(f => f.Id == x.LId));
                IEnumerable<Sheets.Slot> acslots = slots.Where(x => acflts.Any(f => f.Slot == x.Id));

                row[0] = ac.Reg ?? "N/A";
                row[1] = ac.Type ?? "N/A";
                row[2] = ac.Seats;
                row[3] = ac.Interval; 
                row[4] = ac.AutoAssign ? "P" : "N";
                row[5] = ac.OGN ?? "-";
                row[6] = acflts.Count();
                row[7] = actgts.Count();
                row[8] = actgts.Sum(x => x.Weight);

                row[9] = GSettings.UnformatPrice(prices.Find(x => x.Id == ac.PriceCat)?.Price ?? 0);
                row[10] = GSettings.UnformatPrice(actgts.Sum(tgt => tgt.Price < 0 ? prices.Find(x => x.Id == -tgt.Price)?.Price ?? 0 : tgt.Price));

                row[11] = $"{acslots.Min(x => x.STime)} - {acslots.Max(x => x.FTime)}";
                row[12] = acslots.Sum(x => x.Length);

                table.Rows.Add(row);
            }
            //XmlSerializer xml = new(typeof(DataSet));

            //MemoryStream writer = new(Encoding.Default.GetBytes(ticketsum.GetXml()));
            MemoryStream writer = new();
            table.WriteXml(writer,XmlWriteMode.IgnoreSchema);
            writer.Position = 0;
            //xml.Serialize(writer,ticketsum);
            await DskMan.AutoPermSaveFile(writer,file);
            writer.Close();
        }
    }
}

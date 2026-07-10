using fltstd26.core;
using fltstd26.Resources.Texts;
using fltstd26.system;
using fltstd26.system.modals;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace fltstd26.etc.online
{
    public static class OGN
    {
        internal static OGNLogbook CurrentOGN = new();
        internal static List<string> IgnoredAircraft = [];

        public static TimeSpan? FormatTime(string? time)
        {
            if (time == null) return null;
            string[] split = time.Split('h');
            if (Int32.TryParse(split[0],out int hour) && Int32.TryParse(split[1],out int minute)) return new(hour,minute,0);
            return null;
        }

        public static DateTime? FormatTimeToday(string? time)
        {
            if(time == null) return null;
            string[] split = time.Split('h');
            DateTime today = DateTime.Now;
            if (Int32.TryParse(split[0],out int hour) && Int32.TryParse(split[1],out int minute)) return new(today.Year,today.Month,today.Day,hour,minute,today.Second);
            return null;
        }

        internal async static void Sync()
        {
            try
            {
                ConProc.Log("[OGN] Synchronisieren...",0);
                OGNLogbook log = await Get(USettings.Homebase) ?? throw new Exception("Keine Daten empfangen");
                CurrentOGN = log;
            }
            catch (Exception ex)
            {
                ConProc.Log("[OGN] Fehler: " + ex.Message,2);
                
            }
        }

        internal static void QeueSync(int min)
        {

        }

        internal static void LinkAddress(bool Overwrite)
        {
            if (CurrentOGN.devices == null) return;
            string nonASCII = @"[^\u0000-\u007F]+";
            List<Sheets.Lfz> acs = RData.GetAircraftTable();
            acs.ForEach(ac =>
            {
                if (Overwrite || (ac.OGN == null || ac.OGN == ""))
                {
                    RData.UpdateProperty<string>(ac.Id,
                        CurrentOGN.devices.Find(x => x.registration != null && ac.Reg != null && 
                        Regex.Replace(x.registration,nonASCII,string.Empty).Replace("-",string.Empty).Trim() == Regex.Replace(ac.Reg,nonASCII,string.Empty).Replace("-",string.Empty).Trim())?.address,
                        "OGN",typeof(Sheets.Lfz),true);
                }
                
            });
        }
        internal static async void RelinkAddress(bool allLfz, bool allAdr)
        {
            if (CurrentOGN.devices == null) return;
            List<Sheets.Lfz> a = RData.GetAircraftTable();
            List<Sheets.Lfz> acs = allLfz ? a : [..a.Where(x => x.OGN == null || x.OGN == "")];
            IEnumerable<Device> dvs = allAdr ? CurrentOGN.devices : CurrentOGN.devices.Where(x => !a.Select(x => x.OGN).Contains(x.address));
            List<(string, string, string)> elements = [("x.png", Lang.dont_care, ""),.. acs.Select(x => ("plane.png", x.Reg ?? x.Id.ToString(), x.Type))];
            foreach(Device dv in dvs)
            {
                if(dv == null || dv.address == null) continue;
                int index = -1;
                await ModalPush.Selector(dv.registration + $"({dv.address})\r\n" + dv.aircraft_type,elements).ContinueWith(t => index = t.Result);
                if (index > 0)
                {
                    RData.UpdateProperty<string>(acs[index - 1],dv.address,"OGN",typeof(Sheets.Lfz),true);
                }
                else if (index == -1) break;
            }      
        }
        public static async Task<string> GetRaw(string ap)
        {
            if (ap != "")
            {
                try
                {
                    HttpResponseMessage rsp = await new HttpClient().GetAsync(new Uri("http://flightbook.glidernet.org/api/logbook/" + ap));
                    if (rsp.IsSuccessStatusCode)
                    {
                        return await rsp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        ConProc.Log("[OGN] Ausnahme: " + rsp.StatusCode,2);
                        return "EX: " + rsp.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    ConProc.Log("[OGN] Ausnahme: " + ex.Message,2);
                    return "EX: " + ex.Message;
                }
            }
            else
            {
                ConProc.Log("[OGN] Kein Flugplatz",1);
                return "EX: INVALID";
            }
        }

        public static async Task<OGNLogbook?> Get(string ap)
        {
            ConProc.Log("[OGN] Das Logbuch für " + ap + " wurde angefordert");
            string raw = await GetRaw(ap);
            if (raw.StartsWith("EX: "))
            {
                ConProc.Log("[OGN] Logbuch kann nicht erfragt werden: " + raw,2);
                return null;
            }
            else
            {
                try
                {
                    OGNLogbook? logbook = JsonSerializer.Deserialize<OGNLogbook>(raw);
                    if (logbook != null)
                    {
                        return logbook;
                    }
                    else
                    {
                        ConProc.Log("[OGN] Logbuch Übersetzungsfehler",2);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    ConProc.Log("[OGN] Fehler bei der Übersetzung des Logbuchs: " + ex.Message,2);
                    return null;
                }
            }
        }

#pragma warning disable IDE1006
        public class OGNLogbook
        {
            public string? a_day { get; set; }
            public Airfield? airfield { get; set; }
            public int? call_tsp { get; set; }
            public string? code { get; set; }
            public DateTime? date { get; set; }
            public List<Device>? devices { get; set; }
            public List<Flight>? flights { get; set; }
            public List<string>? rnames { get; set; }
        }

        public class TimeInfo
        {
            public string? dawn { get; set; }
            public string? noon { get; set; }
            public string? sunrise { get; set; }
            public string? sunset { get; set; }
            public string? twilight { get; set; }
            public string? tz_offset { get; set; }
        }

        public class Airfield
        {
            public string? code { get; set; }
            public string? country { get; set; }
            public string? name { get; set; }
            public int? elevation { get; set; }
            public double[]? latlng { get; set; }
            public TimeInfo? time_info { get; set; }
        }

        public class Device
        {
            public string? address { get; set; }
            public string? aircraft { get; set; }
            public int? aircraft_type { get; set; }
            public string? competition { get; set; }
            public string? db_org { get; set; }
            public string? device_type { get; set; }
            public bool? identified { get; set; }
            public string? registration { get; set; }
            public bool? tracked { get; set; }
        }

        public class Flight
        {
            public int? device { get; set; }
            public int? duration { get; set; }
            public int? max_alt { get; set; }
            public int? max_height { get; set; }
            public string? start { get; set; }
            public int? start_q { get; set; } //Piste Start
            public int? start_tsp { get; set; } //Timestamp
            public string? stop { get; set; }
            public int? stop_q { get; set; } //Piste Landung
            public int? stop_tsp { get; set; } //Timestamp
            public int? tow { get; set; }
            public bool? towing { get; set; }
            public bool? warn { get; set; }
        }
    }
}

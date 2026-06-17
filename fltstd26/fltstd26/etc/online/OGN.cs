using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fltstd26.etc.online
{
    public static class OGN
    {
        public static OGNLogbook CurrentOGN = new();

        public static TimeSpan? FormatTime(string? time)
        {
            if (time == null) return null;
            string[] split = time.Split('h');
            if (Int32.TryParse(split[0],out int hour) && Int32.TryParse(split[1],out int minute)) return new(hour,minute,0);
            return null;
        }

        internal async static void Sync()
        {
            ConProc.Log("[OGN] Synchronisieren...",1);
            OGNLogbook? log = await Get(USettings.Homebase);
            if (log != null) CurrentOGN = log;
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

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
                        ConProc.Log("[OGN] Exception: " + rsp.StatusCode,2);
                        return "EX: " + rsp.StatusCode;
                    }
                }
                catch(Exception ex)
                {
                    ConProc.Log("[OGN] Exception: " + ex.Message, 2);
                    return "EX: " + ex.Message;
                }
            }
            else
            {
                ConProc.Log("[OGN] Invalid Airport Call",1);
                return "EX: INVALID";
            }
        }

        public static async Task<OGNLogbook?> Get(string ap)
        {
            string raw = await GetRaw(ap);
            if(raw.StartsWith("EX: "))
            {
                ConProc.Log("[OGN] Failed to fetch logbook: " + raw, 2);
                return null;
            }
            else
            {
                try
                {
                    OGNLogbook? logbook = JsonSerializer.Deserialize<OGNLogbook>(raw);
                    if(logbook != null)
                    {
                        return logbook;
                    }
                    else
                    {
                        ConProc.Log("[OGN] Failed to parse logbook", 2);
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    ConProc.Log("[OGN] Exception while parsing logbook: " + ex.Message, 2);
                    return null;
                }
            }
        }

        public class OGNLogbook
        {
            public string? a_day { get; set; }
            public required Airfield airfield { get; set; }
            public int call_tsp { get; set; }
            public string? code { get; set; }
            public DateTime date { get; set; }
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
            public int elevation { get; set; }
            public double[]? latlng { get; set; }
            public required TimeInfo time_info { get; set; }
        }

        public class Device
        {
            public string? address { get; set; }
            public string? aircraft { get; set; }
            public int aircraft_type { get; set; }
            public string? competition { get; set; }
            public string? db_org { get; set; }
            public string? device_type { get; set; }
            public bool identified { get; set; }
            public string? registration { get; set; }
            public bool tracked { get; set; }
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
            public int stop_tsp { get; set; } //Timestamp
            public int? tow { get; set; }
            public bool towing { get; set; }
            public bool warn { get; set; }
        }
    }
}

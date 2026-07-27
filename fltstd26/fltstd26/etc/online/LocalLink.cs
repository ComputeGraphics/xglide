using fltstd26.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace fltstd26.etc.online
{
    internal static class LocalLink
    {
        internal static int MachineCount;
        internal static readonly List<LinkedMachine> Listeners = [];

        private static LinkState? _link;
        private static int _distribid = 0;
        private static Dictionary<int,(Type, object)> _resplink = [];
        private readonly static List<int> _awaitresp = [];
        //private static UdpClient? _udp = null;
        //private static TcpClient? _tcp = null;
        internal static bool NetworkAvail => Connectivity.Current.NetworkAccess > NetworkAccess.Local;
        internal static bool IsConnected()
        {
            return false;
        }

        internal static void Init(int port)
        {
            try
            {
                IPEndPoint ep = new(IPAddress.Any,port);
                UdpClient uc = new(ep);
                //TcpClient tc = new(ep);
                TcpListener tl = new(ep);
                LinkState ls = new()
                {
                    Port = port,
                    EndPoint = ep,
                    //TcpClient = tc,
                    UdpClient = uc,
                    TcpListener = tl
                };
                _link = ls;
                uc.BeginReceive(x => Receive(x,false),ls);
            }
            catch (Exception ex)
            {
                ConProc.Log("[LINK] Connection failed: " + ex.Message,2);
            }
        }

        internal static void Reset(LinkState ls,bool tcp)
        {
            if (tcp)
            {
                ls.TcpListener?.BeginAcceptSocket(new AsyncCallback(x => Receive(x,true)),ls);
            }
            else
            {
                ls.UdpClient?.BeginReceive(new AsyncCallback(x => Receive(x,false)),ls);
            }
        }

        internal static void AddConnection(string name,string ip,int port,byte level)
        {
            try
            {
                if (!IPAddress.TryParse(ip,out IPAddress? ipa) || ipa == null)
                {
                    throw new Exception("Invalid IP Address");
                }
                else Listeners.Add(new(name,level,ipa,port));
            }
            catch (Exception ex)
            {
                ConProc.Log("[LINK] Connection failed: " + ex.Message,2);
            }
        }

        private static void Receive(IAsyncResult ar,bool tcp)
        {
            if (ar.AsyncState is LinkState ls && ls.EndPoint != null)
            {
                string txt = "";
                if (tcp)
                {
                    Socket? socket = ls.TcpListener?.AcceptSocket();
                    if ((ls.TcpListener?.Pending() ?? false) && socket != null)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            byte[] recieveBuffer = new byte[512];
                            int recievedBytes = socket.Receive(recieveBuffer);
                            if (recievedBytes == 0) break;
                            txt += Encoding.Unicode.GetString(recieveBuffer);
                        }
                    }
                    Reset(ls,true);
                }
                else
                {
                    byte[]? resp = ls.UdpClient?.EndReceive(ar,ref ls.EndPoint!);
                    if (resp != null)
                    {
                        txt = Encoding.Unicode.GetString(resp);
                    }
                    Reset(ls,false);
                }

                //Receive Subscribers
            }
        }

        /*internal static async Task<int> Distribute(string data, bool tcp)
        {
            if (_link == null) throw new Exception("LocalLink is not initialized");

            ArraySegment<byte> ars = new(Encoding.Unicode.GetBytes(data));
            if (tcp)
            {
                if (_link.TcpListener == null) throw new Exception("TCP Listener is not initialized");
                foreach (LinkedMachine lm in Listeners)
                {
                    await _link.TcpListener.Server.ConnectAsync(lm.EndPoint);
                    await _link.TcpListener.Server.SendAsync(ars);
                    _awaitresp.Add(_distribid++);
                }
                
            }
        }*/


    }

    public class LinkState
    {
        public UdpClient? UdpClient;
        //public TcpClient? TcpClient;
        public TcpListener? TcpListener;
        public int Port { get; init; }
        public required IPEndPoint EndPoint;
    }

    public class LinkedMachine(string name,byte level,IPAddress ip,int port)
    {
        public int Id { get; init; } = LocalLink.MachineCount++;
        public string Name { get; set; } = name;
        public byte Level { get; internal set; } = level < 1 ? (byte)1 : level;
        public IPEndPoint EndPoint { get; internal set; } = new(ip,port);
    }
}

using System;
using System.Linq;
using System.Net;
// --- custom --- //
using ProjectWaterMelon.Log;
// -------------- //

namespace ProjectWaterMelon.Network.Sytem
{
    public static class CHostFinder
    {
        public static IPEndPoint GetServerIPEndPointByHostEntry(string host, ushort port, bool ipV4Flag = true)
        {
            IPEndPoint retIPEndPoint = null;
            try
            {
                var lHostIPList = Dns.GetHostEntry(host).AddressList;
                if (ipV4Flag)
                {
                    var retHostIP = lHostIPList.FirstOrDefault(IHostIP => IHostIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    return new IPEndPoint(retHostIP, port);
                }
                else
                {
                    var retHostIP = lHostIPList.FirstOrDefault(IHostIP => IHostIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
                    return new IPEndPoint(retHostIP, port);
                }
            }
            catch(Exception ex)
            {
                CLog4Net.gLog4Net.ErrorFormat($"Exception in CHostFinder.GetServerIPEndPointByHostEntry - {ex.Message},{ex.StackTrace},{host}-{port}-{ipV4Flag}");
            }
            return retIPEndPoint;
        }

        public static IPEndPoint GetServerIPEndPointByIPAddress(string host, ushort port)
        {
            IPEndPoint retIPEndPoint = null;
            try
            {
                return new IPEndPoint(IPAddress.Parse(host), port);
            }
            catch(Exception ex)
            {
                CLog4Net.gLog4Net.ErrorFormat($"Exception in CHostFinder.GetServerIPEndPointByIPAddress - {ex.Message},{ex.StackTrace},{host}-{port}");
            }
            return retIPEndPoint;
        }

        public static IPEndPoint GetServerIPEndPointByLocal(ushort port, bool ipV4Flag = true)
        {
            return GetServerIPEndPointByHostEntry(Dns.GetHostName(), port, ipV4Flag);
        }
    }
}

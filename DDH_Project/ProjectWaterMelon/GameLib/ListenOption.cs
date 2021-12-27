using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ProjectWaterMelon.Network.Config;

namespace ProjectWaterMelon.GameLib
{
    public static class ListenOption
    {
        public static void DoNotWait(this Task job)
        {

        }
        /// <summary>
        /// Listen 관련 클래스(CTcpListener)에서만 사용. 하지만 여러번 호출하기 때문에 따로 static 메서드로 추출 
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        /// <returns></returns>
        public static IPEndPoint GetListenIPEndPoint(string _ip, ushort _port)
        {
            var ip = _ip;
            var port = _port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                ipAddress = IPAddress.Any;
            else if ("ipv6any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                ipAddress = IPAddress.IPv6Any;
            else
                ipAddress = IPAddress.Parse(ip);

            return new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Listen 관련 클래스(CTcpListener)에서만 사용. 하지만 여러번 호출하기 때문에 따로 static 메서드로 추출 
        /// </summary>
        /// <param name="_ip"></param>
        /// <param name="_port"></param>
        /// <returns></returns>
        public static IPEndPoint GetListenIPEndPoint(IListenConfig config)
        {
            var ip = config.ip;
            var port = config.port;

            IPAddress ipAddress;

            if ("any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                ipAddress = IPAddress.Any;
            else if ("ipv6any".Equals(ip, StringComparison.OrdinalIgnoreCase))
                ipAddress = IPAddress.IPv6Any;
            else
                ipAddress = IPAddress.Parse(ip);

            return new IPEndPoint(ipAddress, port);
        }
    }
}

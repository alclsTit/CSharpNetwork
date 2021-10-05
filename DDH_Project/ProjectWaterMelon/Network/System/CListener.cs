using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Log;
using static ConstModule.ConstDefine;
using static ConstModule.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.Sytem
{
    class CListener
    {
        public Socket mListenSocket { get; set; }
        private CAcceptor m_Acceptor;
        private bool mipv4Flag { get; set; }

        // 클라이언트 접속 시 호출될 델리게이트 
        public delegate void NewClientAccessHandler(Socket _clientSocket, object _tocken);
        public NewClientAccessHandler CallBack_On_NewClient = null;


        public CListener(bool tcpflag = true, bool ipv4Flag = true)
        {
            if (tcpflag)
            {
                // tcp
                if (ipv4Flag)
                    mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                else
                    mListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                // udp
                if (ipv4Flag)
                    mListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                else
                    mListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            }

            mListenSocket.NoDelay = true;   // nagle 알고리즘을 사용하지 않는다. (네트워크 트래픽 부하 감소 보단 성능선택)
            mipv4Flag = ipv4Flag;
        }

        public void Start(ushort port, int backlog = 200)
        {
            IPEndPoint lEndPoint = CHostFinder.GetServerIPEndPointByLocal(port, mipv4Flag);
            try
            {
                // bind/listen 
                mListenSocket.Bind(lEndPoint);
                mListenSocket.Listen(backlog);

                // recv/send pool 생성 
                CSocketAsyncEventManager.Init(MAX_CONNECTION, eSockEvtType.CONCURRENT);
                CSocketAsyncEventManager.InitRecvAndSendArgsMax(mListenSocket, lEndPoint);

                // accept 
                m_Acceptor = new CAcceptor(mListenSocket, lEndPoint);
                Thread lAsyncThread = new Thread(m_Acceptor.Start);
                lAsyncThread.Start();

            }
            catch(Exception ex)
            {
                CLog4Net.LogError($"Exception in CListener.Start!!! - {ex.Message},{ex.StackTrace}, host = {lEndPoint.Address}, port = {lEndPoint.Port}");
            }
        }
    }
}

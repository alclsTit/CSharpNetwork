using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
// --- custom --- //
using ProjectWaterMelon.Network.CustomSocket;
using ProjectWaterMelon.Network.Sytem;
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using ProjectWaterMelon.Game;
using static ProjectWaterMelon.ConstDefine;
using static ProjectWaterMelon.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.Sytem
{
    class CConnector
    {
        public Socket mSocket { get; private set; }
        private bool mipv4Flag { get; set; }

        private IPEndPoint mIPEndPoint;

        private CSessionManager mSessionManager = new CSessionManager();

        public CConnector()
        {
        }
  
        public void Init(ushort port, bool tcpFlag = true, bool ipV4Flag = true)
        {
            mIPEndPoint = CHostFinder.GetServerIPEndPointByLocal(port, ipV4Flag);
            mipv4Flag = ipV4Flag;
            if (tcpFlag)
            {
                //tcp 
                if (mipv4Flag) mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                else mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                // udp
                if (mipv4Flag) mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                else mSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            }

            mSocket.NoDelay = true;
        }

        public void Start()
        {
            try
            {
                CLog4Net.LogDebugSysLog($"1.CConnector.Start", $"Connect Start");

                var lConnectAsyncEventArgs = new SocketAsyncEventArgs();
                lConnectAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectHandler);
                lConnectAsyncEventArgs.RemoteEndPoint = mIPEndPoint;

                var lPending = mSocket.ConnectAsync(lConnectAsyncEventArgs);
                if (!lPending)
                {
                    // 비동기 함수 호출이 즉시완료 되지 않은경우 pending = false. 이 경우 비동기 함수 호출을 직접 진행 
                    OnConnectHandler(this, lConnectAsyncEventArgs);
                    CLog4Net.LogDebugSysLog($"1.CConnector.Start", $"Call OnConnectHandler(No Async Call)");
                }
            }
            catch(Exception ex)
            {
                CLog4Net.LogError($"Exception in CConnector.Start!!! - {ex.Message},{ex.StackTrace}");
            }
        }

        /*
        * 정의: connect 실패 시 해당 소켓과 연결된 대상 종료 및 리소스 초기화 진행      
        * connect - accept에 성공한 대상의 경우에만 session 생성
        * client는 connect 풀링을 하지 않기 때문에 accept와는 다르게 사용한 SocketAsyncEventArgs 객체풀에 push 하지 않는다
        */
        public void OnBadConnectHandler(in SocketAsyncEventArgs e)
        {
            // connect 단계에서 SocketError 발생 대상들은 소켓만 Close 한다
            e.AcceptSocket.Close();     
        }

        private void OnConnectHandler(object send, SocketAsyncEventArgs e)
        {          
            if (e.SocketError == SocketError.Success)
            {
                CLog4Net.LogDebugSysLog($"2.CConnector.OnConnectHandler", $"Call OnConnectHandler(Connect is success)");

                var lUserToken = new CSession();
                if (CSocketAsyncEventManager.SetSocketAsyncEventArgs(mSocket, ref lUserToken, mIPEndPoint))
                {
                    lUserToken.mTcpSocket.SetSocketConnected(true);
                    mSessionManager.Add(ref lUserToken);

                    CLog4Net.LogDebugSysLog($"3.CConnector.OnConnectHandler", $"Receive Start");
                    bool lPending = lUserToken.mTcpSocket.mRawSocket.ReceiveAsync(lUserToken.mTcpSocket.mRecvArgs);
                    if (!lPending)
                    {
                        CLog4Net.LogDebugSysLog($"3.CConnector.OnConnectHandler", $"Call OnReceiveHandler(No Async Call)");
                        lUserToken.mTcpSocket.OnReceiveHandler(this, lUserToken.mTcpSocket.mRecvArgs);
                    }

                    // SEND TEST PACKET
                    var req_msg = new Protocol.msg_test.req_network_sessionid_user2game();
                    req_msg.session_id = lUserToken.mSessionID;
                    req_msg.cur_datetime = DateTime.Now.ToString(DateFormatYMDHMS);
                    lUserToken.mTcpSocket.Relay<Protocol.msg_test.req_network_sessionid_user2game>(req_msg.msg_id, req_msg, true);
                }
                else
                {
                    OnBadConnectHandler(e);
                    CLog4Net.LogError($"Error in CConnector.OnConnectHandler - SEND/RECV SocketAsyncEventArgs Set Error");
                }
            }
            else
            {
                OnBadConnectHandler(e);
                CLog4Net.LogError($"Error in CConnector.OnConnectHandler - {e.SocketError}");
            }
        }

    }
}

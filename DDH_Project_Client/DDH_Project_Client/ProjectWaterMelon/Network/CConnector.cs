using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static ConstModule.ConstDefine;
using static ConstModule.GSocketState;
using ProjectWaterMelon.Game;

namespace ProjectWaterMelon.Network
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

        public void OnBadConnectHandler(ref SocketAsyncEventArgs args)
        {
            var lUserToken = args.UserToken as CSession;
            lUserToken?.mTcpSocket.Disconnect();
            args.AcceptSocket = null;

            if (!mSessionManager.Remove(lUserToken.mSessionId))
                CLog4Net.LogError($"Error in CConnector.OnBadConnectHandler!!! - SessionManager didn't remove session properly");
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
                    var req_msg = new Protocol.msg_test.handler_req_network_sessionid_user2game();
                    req_msg.session_id = lUserToken.mSessionId;
                    req_msg.cur_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    req_msg.BuildPacketClass(ref req_msg,(int)Protocol.PacketId.req_network_sessionid);
                    lUserToken.mTcpSocket.AsyncSend(req_msg);
                }
                else
                {
                    OnBadConnectHandler(ref e);
                }
            }
            else
            {
                OnBadConnectHandler(ref e);
                CLog4Net.LogError($"Error in CConnector.OnConnectHandler!!! - {e.SocketError}");
            }
        }

    }
}

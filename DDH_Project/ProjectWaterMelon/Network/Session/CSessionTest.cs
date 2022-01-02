using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using ProjectWaterMelon.Network.SystemLib;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Network.CustomSocket;

namespace ProjectWaterMelon.Network.Session
{
    public class CSessionTest : ISessionRoot
    {
        public CTcpAsyncSocket mClientSocket { get; private set; }
        public string mSessionId { get; private set; }
        public IPEndPoint mHostLocalEndPoint { get; private set; }
        public IPEndPoint mRemoteClientEndPoint { get; private set; }
        private CServerConfig mServerConfig;


        public String GetSessionId => mSessionId;

        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IServerConfig config, in Socket socket, int queueMaxSize, in SocketAsyncEventArgs recv, in SocketAsyncEventArgs send)
            : this(config, socket, queueMaxSize, recv, send, GSocketState.eSockEvtType.CONCURRENT)
        {

        }

        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IServerConfig config, in Socket socket, int queueMaxSize, SocketAsyncEventArgs recv, SocketAsyncEventArgs send, GSocketState.eSockEvtType poolAsync)
        {
            mServerConfig = config as CServerConfig;

            // Session 당 사용할 커스텀 소켓 객체 할당 및 옵션 설정
            mClientSocket = new CTcpAsyncSocket(socket, mServerConfig.sendingQueueSize, queueMaxSize, ref recv, ref send, this);
            mClientSocket.SetSocketOption(mServerConfig.noDelay, 
                                         mServerConfig.recvBufferSize,
                                         mServerConfig.sendBufferSize,
                                         mServerConfig.socketLingerFlag,
                                         mServerConfig.socketLingerDelayTime);

            mHostLocalEndPoint = (IPEndPoint)socket.LocalEndPoint;

            mRemoteClientEndPoint = (IPEndPoint)socket.RemoteEndPoint;

            mSessionId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Session 객체 생성시 동반되는 초기화부분. recv 용 SocketAsyncEventPool 세팅 
        /// </summary>
        public void Initalize(in SocketAsyncEventArgs recv, in SocketAsyncEventArgs send)
        {
     
      
        }

        /// <summary>
        /// Recv 작업 진행 
        /// </summary>
        public void Start()
        {
            mClientSocket.Receive();
        }

        public void Close()
        {

        }
    }
}


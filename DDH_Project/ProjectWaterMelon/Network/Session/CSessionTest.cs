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
using ProjectWaterMelon.Network.Server;

namespace ProjectWaterMelon.Network.Session
{
    public class CSessionTest : CSessionRoot
    {
        /// <summary>
        /// 세션에서 통신에 사용할 소켓 정보
        /// </summary>
        public CTcpAsyncSocket mClientSocket { get; private set; }

        /// <summary>
        /// 현재 세션 상태
        /// </summary>
        public SessionState mSessionState { get; private set; } = SessionState.NotInitialized;

        /// <summary>
        /// 세션과 연결된 서버 정보 (서버는 여러종류가 있으므로 인터페이스로 보유)
        /// </summary>
        public IAppServer mIAppServer { get; private set; }

        /// <summary>
        /// 세션 아이디 반환
        /// </summary>
        public String GetSessionID => sessionID;

        /// <summary>
        /// 세션을 관리하는 서버의 상태 반환
        /// </summary>
        public ServerState GetServerState => mIAppServer.state;

        /// <summary>
        /// 세션을 관리하는 서버의 타입 반환
        /// </summary>
        public ServerType GetServerType => mIAppServer.type;
        
        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IAppServer server, Socket socket, int queueMaxSize, SocketAsyncEventArgs recv, SocketAsyncEventArgs send)
            : this(server, socket, queueMaxSize, recv, send, GSocketState.eSockEvtType.CONCURRENT)
        {

        }

        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IAppServer server, Socket socket, int queueMaxSize, SocketAsyncEventArgs recv, SocketAsyncEventArgs send, GSocketState.eSockEvtType poolAsync)
        {
            var config = server.config;

            mIAppServer = server;

            // Session 당 사용할 커스텀 소켓 객체 할당 및 옵션 설정
            mClientSocket = new CTcpAsyncSocket(socket, config.sendingQueueSize, queueMaxSize, recv, send, this);
            mClientSocket.SetSocketOption(config.noDelay,
                                         config.recvBufferSize,
                                         config.sendBufferSize,
                                         config.socketLingerFlag,
                                         config.socketLingerDelayTime);

            hostEndPoint = (IPEndPoint)socket.LocalEndPoint;

            remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;

            sessionID = Guid.NewGuid().ToString();

            this.OnClose += OnSessionClose;

            mSessionState = SessionState.Initialized;
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
        public override void Start()
        {
            mClientSocket.Receive();
        }

        /// <summary>
        /// 세션 종료 시 후처리 작업 진행
        /// </summary>
        /// <param name="reason"></param>
        public override void Close(eCloseReason reason)
        {
            mSessionState = SessionState.Stop;

            mClientSocket.Close(reason);
        }

        /// <summary>
        /// Session Close 후처리 작업
        /// </summary>
        /// <param name="session"></param>
        /// <param name="closeReason"></param>
        private void OnSessionClose(ISessionBase session, eCloseReason closeReason)
        {

        }

    }
}


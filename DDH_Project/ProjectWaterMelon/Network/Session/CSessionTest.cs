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
        public CTcpAsyncSocket clientsocket { get; private set; }
        public string mSessionId { get; private set; }
        public IPEndPoint mLocalEP { get; private set; }
        public IPEndPoint remoteEP { get; private set; }
        public IServerConfig mIServerConfig { get; private set; }

        /// <summary>
        /// Recv SocketAsyncEventArgs Pool
        /// </summary>
        private CSocketAsyncEventArgsPool mConcurrentRecvPool;

        /// <summary>
        /// Recv SocketAsyncEventArgs Pool의 SetBuffer진행 시 사용할 버퍼 매니저
        /// </summary>
        private CBufferManager mBufferManager;

        /// <summary>
        /// 세션 아이디 저장
        /// </summary>
        /// <param name="sid"></param>
        public void SetSessionId(string sid) { mSessionId = sid; }

        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IServerConfig config, in Socket socket, int queueMaxSize)
            : this(config, socket, queueMaxSize, GSocketState.eSockEvtType.CONCURRENT)
        {

        }

        /// <summary>
        /// 세션 생성자, 클라이언트와 소켓 연결 후 세션 생성
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CSessionTest(IServerConfig config, in Socket socket, int queueMaxSize, GSocketState.eSockEvtType poolAsync)
        {
            clientsocket = new CTcpAsyncSocket(config, socket, queueMaxSize);
            mLocalEP = clientsocket.localEP;
            remoteEP = (IPEndPoint)socket.RemoteEndPoint;

            mIServerConfig = config;
            mBufferManager = new CBufferManager(config.recvBufferSize, config.recvBufferSize * config.max_connect_count);

            SetSockRecvPool();
        }

        /// <summary>
        /// Recv SocketAsyncEventArgs Pool 생성 
        /// *[주의] clientsocket.OnReceiveHandler은 외부 클래스의 경우 이곳에서만 사용한다 
        /// </summary>
        /// <param name="poolAsync"></param>
        private void SetSockRecvPool()
        {
            mConcurrentRecvPool = new CSocketAsyncEventArgsPool(mIServerConfig.recvBufferSize);

            for(var idx = 0; idx < mIServerConfig.recvBufferSize; ++idx)
            {
                SocketAsyncEventArgs recv = new SocketAsyncEventArgs();
                recv.Completed += new EventHandler<SocketAsyncEventArgs>(clientsocket.OnReceiveHandler);
                mBufferManager.SetBuffer(recv);

                mConcurrentRecvPool.Push(recv);
            }
        }

        public void Initalize()
        {
        }

        /// <summary>
        /// Recv 작업 진행 
        /// </summary>
        public void Start()
        {
            clientsocket.StartReceive();
        }

        public void Close()
        {

        }
    }
}


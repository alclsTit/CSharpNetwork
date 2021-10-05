using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Session;
using static ConstModule.GSocketState;
using static ConstModule.ConstDefine;
// -------------- //

namespace ProjectWaterMelon.Network.Sytem
{
    // 유저 객체당 할당되는 CSession에 대한 관리  
    static class CSocketAsyncEventManager
    {
        private static CSocketAsyncEventArgsPool mRecvAsyncPool, mSendAsyncPool;

        private static CBufferManager mBufferMng = new CBufferManager(MAX_CONNECTION, MAX_BUFFER_SIZE);
        public static CSocketAsyncEventArgsPool GetRecvAsyncPool => mRecvAsyncPool;
        public static CSocketAsyncEventArgsPool GetSendAsyncPool => mSendAsyncPool;

        public delegate void OnSocketAsyncEventArgsInput(object sender, SocketAsyncEventArgs args);
        
        public static void Init(int BufferCount, eSockEvtType type)
        {
            mRecvAsyncPool = new CSocketAsyncEventArgsPool(BufferCount, type);
            mSendAsyncPool = new CSocketAsyncEventArgsPool(BufferCount, type);
        }

        // 미리 사용할 유저 send/recv pool을 생성한다
        public static void InitRecvAndSendArgsMax(in Socket socket, in System.Net.IPEndPoint IPEndPoint)
        {
            for (var i = 0; i < MAX_CONNECTION; ++i)
            {
                // 1. 세션 하나 생성 (세션과 클라는 1:1 매칭) => 세션 설정을 미리하지 않는다. 
                // Listen 소켓으로 세팅
                CSession lUserToken = new CSession();

                // 2. 세션의 비동기 수신객체 설정 
                var lRecvArgs = new SocketAsyncEventArgs();
                if (mBufferMng.SetBuffer(lRecvArgs))
                {
                    lRecvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(lUserToken.mTcpSocket.OnReceiveHandler);
                    lRecvArgs.UserToken = lUserToken;
                    mRecvAsyncPool.Push(lRecvArgs);
                }

                // 3. 세션의 비동기 송신객체 설정 
                var lSendArgs = new SocketAsyncEventArgs();
                if (mBufferMng.SetBuffer(lSendArgs))
                {
                    lSendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(lUserToken.mTcpSocket.OnSendHandler);
                    lSendArgs.UserToken = lUserToken;
                    mSendAsyncPool.Push(lSendArgs);
                }
            }
        }

        // Send, Recv SocketEventArgs 풀링에서 대상이 있을 경우 호출, 풀에서 객체 가져옴 (우선적 실행)
        public static SocketAsyncEventArgs GetRecvSendSocketAsyncEventArgsPools(eSocketType socketType)
        {
            if (socketType == eSocketType.RECV)
                return mRecvAsyncPool.Count > 0 ? mRecvAsyncPool.Pop() : null;
            else
                return mSendAsyncPool.Count > 0 ? mSendAsyncPool.Pop() : null;
        }

        // Send, Recv SocketAsyncEventArgs 단독 생성 메서드, 풀에서 객체 가져오지 않음 
        // 풀링 객체 모두 사용 시 실행 
        public static SocketAsyncEventArgs GetRecvSendSocketAsyncEventArgs(eSocketType socketType)
        {
            CSession lUserToken = new CSession();
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.UserToken = lUserToken;
            if (socketType == eSocketType.RECV)
            {
                if (mBufferMng.SetBuffer(args))
                {
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(lUserToken.mTcpSocket.OnReceiveHandler);
                    return args;   
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (mBufferMng.SetBuffer(args))
                {
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(lUserToken.mTcpSocket.OnSendHandler);
                    return args;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

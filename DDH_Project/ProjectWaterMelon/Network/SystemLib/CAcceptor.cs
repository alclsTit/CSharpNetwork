using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.ConstDefine;
using static ProjectWaterMelon.GSocketState;
// -------------- //

// *Default => ConfigureAwait(true)
// ConfigureAwait(false) 지정시, 이후의 코드를 비동기함수를 호출한 스레드에서 다시 실행하는 것이 아닌
// Task에서 생성된(스레드풀에서 가져온 스레드)를 이용하여 실행 => 성능상 유리
//*(주의) 비동기메서드 내부적으로 AsyncOperation을 사용할 경우 ConfigureAwait(false) 무시하고 호출 스레드에서 진행
//var result = listenSocket.AcceptAsync().ConfigureAwait(false);
//OnAcceptHandler(acceptAsyncObject);

namespace ProjectWaterMelon.Network.SystemLib
{
    class CAcceptor
    {
        /// <summary>
        /// Client 소켓통신에 사용될 Socket 객체, 연결된 클라이언트 소켓 끝점에 대한 정보를 가진다
        /// </summary>
        private Socket mClientSocket;

        /// <summary>
        /// Accept 비동기 콜백함수가 순차적으로 진행될 수 있도록 하는데 사용
        /// AutoResetEvent 를 false로 지정시 해당 이벤트가 호출된 스레드는 다음 AutoResetEvent.Set을 만나기 전까지 대기상태
        /// </summary>
        private AutoResetEvent mFlowControlEvt = new AutoResetEvent(true);

        /// <summary>
        /// Accept SocketAsyncEventArgs 풀링객체
        /// </summary>
        private CSocketAsyncEventArgsPool mConcurrentAcceptPool;

        /// <summary>
        /// 비동기 작업 취소에 따른 후처리를 위한 객체
        /// </summary>
        private CancellationTokenSource mCancelTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 현재 서버에 accept 된 대상 카운트
        /// </summary>
        public int mCurAcceptCount { get; private set; } = 0;

        public delegate void OnSocketAsyncEventArgsInput(object sender, SocketAsyncEventArgs args);
        public delegate void OnNewClientHandler(Socket Socket, object UserToken);

        public CAcceptor(int numberOfMaxConnect)
        {
            mConcurrentAcceptPool = new CSocketAsyncEventArgsPool(numberOfMaxConnect);

            // SocketAsyncEventArgs의 UserToken 및 Buffer는 사용할 일이 없으므로 따로 세팅하지 않는다
            for(var idx = 0; idx < numberOfMaxConnect; ++idx)
            {
                SocketAsyncEventArgs accept = new SocketAsyncEventArgs();
                accept.Completed += new EventHandler<SocketAsyncEventArgs>(PrevWork_OnAcceptHandler);
                mConcurrentAcceptPool.Push(accept);
            }
        }

        /// <summary>
        /// Accept 전용 SocketAsyncEventArgs 객체 생성 
        /// 이벤트 핸들러만 지정해서 생성. 변동적인건 추후 세팅
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        private SocketAsyncEventArgs CreateDirectAcceptAsyncEventObject(OnSocketAsyncEventArgsInput handler)
        {
            SocketAsyncEventArgs accept = new SocketAsyncEventArgs();
            accept.Completed += new EventHandler<SocketAsyncEventArgs>(handler);

            return accept;
        }

        /// <summary>
        /// 전달받은 listen socket으로 비동기 accept 진행
        /// </summary>
        /// <param name="listenSocket"></param>
        /// <returns></returns>
        public void StartAcceptLoop(Socket listenSocket)
        {
            // 별도의 비동기 작업 취소가 없을 때까지 루프
            while(!mCancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    var acceptAsyncObject = mConcurrentAcceptPool.GetCurPoolingCount > 0 ? mConcurrentAcceptPool.Pop() : CreateDirectAcceptAsyncEventObject(PrevWork_OnAcceptHandler);
                    if (acceptAsyncObject != null)
                    {
                        if (!listenSocket.AcceptAsync(acceptAsyncObject))
                            OnAcceptHandler(acceptAsyncObject);

                        // AcceptAsync를 통해 비동기로 클라접속을 받은 뒤 처리될 때까지 start 함수호출 스레드 대기상태로 변경
                        mFlowControlEvt.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is NullReferenceException)
                        break;

                    if (ex is SocketException se)
                    {
                        var errorCode = se.ErrorCode;

                        //The listen socket was closed
                        if (errorCode == 125 || errorCode == 89 || errorCode == 995 || errorCode == 10004 || errorCode == 10038)
                            break;
                    }

                    GCLogger.Error(nameof(CAcceptor), $"Start", ex);
                    continue;
                }
            }
        }

        private void PrevWork_OnAcceptHandler(object sender, SocketAsyncEventArgs e)
        {
            OnAcceptHandler(e);
        }

        /*
         * 정의: accept 실패 시 해당 소켓과 연결된 대상 종료 및 리소스 초기화 진행
         * accept 에 사용된 SocketAsyncEventArgs 객체는 다른 곳에서 쓸 수 있도록 반환한다
         * accept - connect에 성공한 대상의 경우에만 session 생성
         * accpet는 connect와 다르게 사용한 SocketAsyncEventArgs 객체풀에 push  
         */
        private void onBadAcceptHandler(ref SocketAsyncEventArgs e)
        {
            // 1.accept 단계에서 SocketError 발생 대상들은 소켓만 Close 한다
            e.AcceptSocket?.Close();
           
            // 2.대상 속성 초기화
            e.AcceptSocket = null;
            e.UserToken = null;
            e.RemoteEndPoint = null;

            // 3.풀링 객체에 반납 
            mConcurrentAcceptPool.Push(e);
        }

        private void ResetAndSetCSession(in Socket socket, ref SocketAsyncEventArgs recvArgs, ref SocketAsyncEventArgs sendArgs)
        {
            recvArgs.UserToken = null;
            sendArgs.UserToken = null;

            CSession lUserToken = new CSession();
            lUserToken.mTcpSocket.SetSocket(socket);
            lUserToken.mTcpSocket.SetRemoteAndLocalEP(socket.RemoteEndPoint, socket.LocalEndPoint);
            lUserToken.mTcpSocket.SetSocketConnected(true);
            recvArgs.UserToken = lUserToken;
            sendArgs.UserToken = lUserToken;
            
            lUserToken.mTcpSocket.SetEventArgs(recvArgs, sendArgs);
        }

        /// <summary>
        /// 비동기 소켓통신 응답에 대한 콜백함수가 호출될 때, 체크사항 (1.통신 성공유무 2.수신된 데이터바이트 크기)
        /// 수신된 데이터바이트 크기가 0인 경우 TCP에서는 소켓통신이 종료된 것으로 인식
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool CheckCallbackHandler(in SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnAcceptHandler(SocketAsyncEventArgs e)
        {
            if (!this.CheckCallbackHandler(e))
            {
                GCLogger.Error(nameof(CAcceptor), "OnAcceptHandler", $"Accept callback error - [ErrorCode] = {e.SocketError}, [ByteTransferred] = {e.BytesTransferred.ToString()}");
            }
            else
            {
                Socket client = null;
                client = e.AcceptSocket;


            }
        }

        /*
         * 정의: accept 비동기 호출 완료 시 호출되는 콜백함수
         * 성공 시 recv 진행, 실패 시 onBadAcceptHandler 호출 
         */
        // Accept - Connect의 경우 buffer에 별도의 내용을 보내주지 않기때문에 bytetransferred = 0
        private void OnAcceptHandler(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine($"Thread[OnConnectHandler] ID => {Thread.CurrentThread.ManagedThreadId} ---- { Thread.CurrentThread.ThreadState}");
            if (!CheckCallbackHandler(e))
            {
                GCLogger.LogDebugMode(nameof(CAcceptor), $"OnAcceptHandler", $"Call OnAccpetHandler(Accpet is success)");
            }
            
            if (e.SocketError == SocketError.Success)
            {
                CLog4Net.LogDebugSysLog($"2.CAcceptor.OnAcceptHandler", $"Call OnAccpetHandler(Accpet is success)");

                var recvArgs = CSocketAsyncEventManager.GetRecvSendSocketAsyncEventArgsPools(eSocketType.RECV);
                if (recvArgs == null) recvArgs = CSocketAsyncEventManager.GetRecvSendSocketAsyncEventArgs(eSocketType.RECV);

                var sendArgs = CSocketAsyncEventManager.GetRecvSendSocketAsyncEventArgsPools(eSocketType.SEND);
                if (sendArgs == null) sendArgs = CSocketAsyncEventManager.GetRecvSendSocketAsyncEventArgs(eSocketType.SEND);

                // 여기서 pop을 통해 받아온 recv/sendargs 값의 UserToken의 socket 은 listen socket으로 되어있다
                // 반면, GetRecvSendSocketAsyncEventArgs 을 통해 받아온 UserToken의 socket은 accpet Socket으로 되어있다
                if (recvArgs != null && sendArgs != null)
                {
                    ResetAndSetCSession(e.AcceptSocket, ref recvArgs, ref sendArgs);
                    CLog4Net.LogDebugSysLog($"3.CAcceptor.OnAcceptHandler", $"Receive Start");

                    var lUserToken = recvArgs.UserToken as CSession;
                    CSessionManager.Add(ref lUserToken);

                    bool lPending = lUserToken.mTcpSocket.mRawSocket.ReceiveAsync(recvArgs);
                    if (!lPending)
                    {
                        CLog4Net.LogDebugSysLog($"3.CAcceptor.OnAcceptHandler", $"Call OnReceiveHandler(No Async Call)");
                        lUserToken.mTcpSocket.OnReceiveHandler(this, recvArgs);
                    }

                    lUserToken.NotifyConnected();
                }
                else
                {
                    onBadAcceptHandler(ref e);
                    if (recvArgs == null)
                    {
                        CLog4Net.LogError($"Error in CAcceptor.OnAcceptHandler!!! - Recv socketAsyncEventArgs is NULL");
                    }
                    else if (sendArgs == null)
                    {
                        CLog4Net.LogError($"Error in CAcceptor.OnAcceptHandler!!! - Send socketAsyncEventArgs is NULL");
                    }
                    else
                    {
                        CLog4Net.LogError($"Error in CAcceptor.OnAcceptHandler!!! - Recv && Send socketAsyncEventArgs is NULL");
                    }
                }
            }
            else
            {
                onBadAcceptHandler(ref e);
                CLog4Net.LogError($"Error in CAcceptor.OnAcceptHandler!!! - {e.SocketError}");
            }
            mFlowControlEvt.Set();
        }
    }
}


/// <summary>
/// Accept 스레드에서 비동기 Accept 진행
/// Thread-Safe (CAccept의 경우 스레드 공유자원은 없음)
/// </summary>
/*
public void Start()
{
    while (true)
    {
        try
        {
            // AcceptAsync를 통해 비동기로 클라접속을 받은 뒤 처리될 때까지 start 함수호출 스레드 대기상태로 변경
            mFlowControlEvt.WaitOne();

            GCLogger.LogDebugMode(nameof(CAcceptor), $"Start", $"1.Accpet Start");
            Console.WriteLine($"Thread[OnConnectHandler] ID => {Thread.CurrentThread.ManagedThreadId} ---- { Thread.CurrentThread.ThreadState}");

            var accpet = mAcceptAsyncPool.Count > 0 ? mAcceptAsyncPool.Pop() : CreateAcceptEventArgs(OnAcceptHandler); 
            if (accpet != null)
            {
                var lPending = mClientSocket.AcceptAsync(accpet);
                if (!lPending)
                {
                    // 비동기 함수 호출이 즉시완료 되지 않은경우 pending = false. 이 경우 비동기 함수 호출을 직접 진행 
                    OnAcceptHandler(this, accpet);
                }
            }
        }
        catch (Exception ex)
        {
            GCLogger.Error(nameof(CAcceptor), $"Start", ex);
            continue;
        }
    }
}
*/


/*private void OnAcceptHandler(object sender, SocketAsyncEventArgs e)
{
    if (e.SocketError == SocketError.Success && e.UserToken is CSession lUserToken)
    {
        CLog4Net.LogDebugSysLog($"2.CAcceptor.OnAcceptHandler", $"Call OnAccpetHandler(Accpet is success)");

        // 소켓정보 세팅
        lUserToken.mTcpSocket.SetSocket(e.AcceptSocket);
        lUserToken.mTcpSocket.SetRemoteAndLocalEP(e.AcceptSocket.RemoteEndPoint, e.AcceptSocket.LocalEndPoint);
        lUserToken.mTcpSocket.SetSocketConnected(true);

        // 세션 세팅 
        CSessionManager.Add(ref lUserToken);

        bool AsyncResult = lUserToken.mTcpSocket.mRawSocket.ReceiveAsync(e);
        if (!AsyncResult)
        {
            CLog4Net.LogDebugSysLog($"3.CAcceptor.OnAcceptHandler", $"Call OnReceiveHandler(No Async Call)");
            lUserToken.mTcpSocket.OnReceiveHandler(this, e);
        }

        lUserToken.NotifyConnected();
    }
    else
    {
        onBadAcceptHandler(ref e);
        CLog4Net.LogError($"Error in CAcceptor.OnAcceptHandler!!! - {e.SocketError}");
    }
    mFlowControlEvt.Set();
}
*/

/*public void Start()
{
    try
    {    
        while (true)
        {
            // AcceptAsync를 통해 비동기로 클라접속을 받은 뒤 처리될 때까지 start 함수호출 스레드 대기상태로 변경
            mFlowControlEvt.WaitOne();
            CLog4Net.LogDebugSysLog($"1.Acceptor.Start", $"Accept Start");
            Console.WriteLine($"Thread[OnConnectHandler] ID => {Thread.CurrentThread.ManagedThreadId} ---- { Thread.CurrentThread.ThreadState}");

            mAcceptArgs = mAcceptAsyncPool.Count > 0 ? mAcceptAsyncPool.Pop() : GetAndSetSocketAsyncEventArgs(OnAcceptHandler);
            if (mAcceptArgs != null)
            {
                //1. 풀링하거나 생성한 Accept SocketAsyncEventArgs 객체 세팅
                mAcceptArgs.RemoteEndPoint = mIPEndPoint;

                //2. Accept SocketAsyncEventArgs 객체속성인 UserToken 세팅
                var lUserToken = mAcceptArgs.UserToken as CSession;
                lUserToken.mTcpSocket.SetSocket(mListenSocket);
                lUserToken.mTcpSocket.SetRemoteIPEndPoint(mIPEndPoint);
                //mAcceptArgs.UserToken = lUserToken;

                var lPending = lUserToken.mTcpSocket.mRawSocket.AcceptAsync(mAcceptArgs);
                if (!lPending)
                {
                    // 비동기 함수 호출이 즉시완료 되지 않은경우 pending = false. 이 경우 비동기 함수 호출을 직접 진행 
                    OnAcceptHandler(this, mAcceptArgs);
                    CLog4Net.LogDebugSysLog($"1.Acceptor.Start", $"Call OnAcceptHandler(No Async Call)");
                }
            }  
       }
    }
    catch (Exception ex)
    {
        CLog4Net.LogError($"Exception in CAcceptor.Start!!! - {ex.Message}, {ex.StackTrace}");
    }
}
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Utility;
using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Network.CustomSocket
{
    /// <summary>
    /// TcpAsyncSocket이 상속, 
    /// </summary>
    public abstract partial class AbstractAsyncSocket : ISocketBase
    {
        /// <summary>
        /// Packet SendingQueue
        /// </summary>
        private CSendingQueue mSendingQueue;

        /// <summary>
        /// Packet SendingQueue pool
        /// </summary>
        private CPoolingManager<CSendingQueue> mSendingQueuePool;

        /// <summary>
        /// RecvMessage Managing 
        /// </summary>
        protected CMessageResolver mMsgRecevier = new CMessageResolver();

        /// <summary>
        /// connected client socket
        /// </summary>
        public Socket clientsocket { get; protected set; }

        /// <summary>
        /// local host (호스트 ip, port 정보)
        /// </summary>
        public IPEndPoint localEP { get; protected set; }
        
        /// <summary>
        /// state of socket
        /// </summary>
        private int mSocketState = GSocketState.NotInitialized;

        /// <summary>
        /// release resource check flag
        /// </summary>
        private bool mIsDisposed = false;

        /// <summary>
        /// get socket state
        /// </summary>
        public int SocketState => mSocketState;

        /// <summary>
        /// Get SendingQueue
        /// </summary>
        public CSendingQueue SendingQueue => mSendingQueue;

        /// <summary>
        /// Get Socket current state
        /// </summary>
        public int GetCurrentState => mSocketState & GSocketStateMask.NEW_MASK;

        /// <summary>
        /// Get Socket old state
        /// </summary>
        public int GetOldState => mSocketState & GSocketStateMask.OLD_MASK;

        /// <summary>
        /// AbstractAsyncSocket Constructor
        /// </summary>
        /// <param name="queuePerSize"></param> CSendingPool per size 
        /// <param name="queueMaxSize"></param> pool size
        public AbstractAsyncSocket(int queuePerSize, int queueMaxSize)
        {
            mSocketState = GSocketState.Initialized;

            mSendingQueuePool = new CPoolingManager<CSendingQueue>(new CSendingQueueCreator(queuePerSize), queueMaxSize);
        }

        /// <summary>
        /// Recv 가능한 상태인지 체크하는 함수
        /// </summary>
        /// <returns></returns>
        protected bool TryReceive()
        {
            if (this.CheckSocketIsNormal())
                return true;

            return false;
        }
        protected abstract void ReceiveAsync();

        public void Start()
        {
            
        }

        protected virtual void OnCloseHandler(eCloseReason reason)
        {

        }

        /// <summary>
        /// 소켓 종료시 처리되는 함수 
        /// 상속받는 대상에서도 소켓종료에 따른 처리가 필요해 가상함수로 선언
        /// </summary>
        /// <param name="reason"></param>
        public virtual void Close(eCloseReason reason)
        {
            if (!CheckSocketIsNormal())
                return;

            // 현재 소켓 IO가 Sending 중이면 Send data 를 모두 보낸 후 처리
            var curstate = mSocketState;
            if (curstate == GSocketState.Sending)
            {
                // Todo: socket close 작업 처리 중인데, sending 데이터가 남아 있을 경우 후처리는..?  
                return;
            }

            ChangeState(GSocketState.InClosing);
            OnCloseHandler(reason);
        }

        /// <summary>
        /// 소켓을 종료해도 되는지 체크
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="isSend"></param>
        /// <param name="forceClose"></param>
        public void CheckValidateClose(eCloseReason reason, bool isSend, bool forceClose)
        {
            var curstate = mSocketState;
            if (curstate == GSocketState.Disconnected)
                return;

            if (curstate == GSocketState.InClosing)
            {
                var socket = clientsocket;
                if (socket == null)
                    return;

                if (isSend)
                {
                    var sendingQ = mSendingQueue;
                    if (forceClose || (sendingQ != null && sendingQ.Count == 0))
                    {
                        try
                        {
                            clientsocket?.Shutdown(SocketShutdown.Both);
                            clientsocket?.Close();
                        }
                        catch (Exception ex)
                        {
                            GCLogger.Error(nameof(AbstractAsyncSocket), $"CheckValidateClose", ex, $"Socket Close Error!!!");
                        }
                    }

                    return;
                }

                if (curstate != GSocketState.Receiving)
                    Close(reason);
            }
            else if (forceClose)
            {
                // Socket이 InClosing 상태가 아니면서 강제종료 할 때  
                // 소켓 강제종료
                Close(reason);
            }
        }

        /// <summary>
        /// Send IO 진행 도중 오류발생 시 해당 함수호출
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="reason"></param>
        public void OnSendError(ref CSendingQueue queue, eCloseReason reason)
        {
            queue.Clear();
            mSendingQueuePool.Push(queue);
            OnSendEnd(reason, true);
        }

        /// <summary>
        /// Send IO 종료 시 해당 함수호출 (Default)
        /// Reason = Unknown
        /// </summary>
        public void OnSendEnd()
        {
            OnSendEnd(eCloseReason.UnKnown, false);
        }

        /// <summary>
        /// Send IO 종료 시 해당 함수호출
        /// </summary>
        /// <param name="reason"></param>
        public void OnSendEnd(eCloseReason reason, bool forceClose)
        {
            // 1. 소켓 상태 변경 
            // 2. 후처리 작업
            ChangeSocketCondition(GSocketState.Sending);
            CheckValidateClose(reason, true, forceClose);
        }

        /// <summary>
        /// Socket 상태 갱신함수
        /// </summary>
        /// <param name="state"></param>
        public void ChangeSocketCondition(int state)
        {
            if (!TryChangeSocketCondition(state))
                GCLogger.Error(nameof(AbstractAsyncSocket), $"ChangeSocketCondition", "Change of SocketState error");
        }

        /// <summary>
        /// Socket 상태 갱신함수 (다중스레드에서 접근가능하므로 동시성을 고려하여 구현)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool TryChangeSocketCondition(int state)
        {
            var oldstate = mSocketState;
            var newstate = state;

            // 소켓이 닫힐 때에는 상태를 변경하지 않는다
            if (oldstate == GSocketState.InClosing)
                return false;

            // 새로운 소켓상태가 이전과 같은 상태일 경우 변경하지 않는다
            if (newstate == mSocketState)
                return false;

            var compstate = Interlocked.CompareExchange(ref mSocketState, newstate, oldstate);

            if (compstate != oldstate)
                return false;

            return true;
        }


        /// <summary>
        /// Socket IO 진행 시, 해당 소켓이 패킷을 보낼 수 있는 상태인지 체크
        /// </summary>
        /// <returns></returns>
        public bool CheckSocketIsNormal()
        {
            var socket = clientsocket;
            var curstate = mSocketState;

            // 1. socket 이 null인지 체크 
            if (socket == null)
                return false;

            // 2. 소켓연결이 이미 끊어진 상태인지 체크
            if (curstate == GSocketState.Disconnected)
                return false;

            // 3. 소켓을 닫는 작업중인지 체크
            if (curstate == GSocketState.InClosing)
                return false;

            return true;
        }

        protected abstract void SendAsync(CSendingQueue queue);

        /// <summary>
        /// SendAsync -> TrySend(send 가능여부체크) -> StartSend(send 준비) -> AsyncSend 진행
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="trackID"></param>
        private void StartSend(CSendingQueue queue, ushort trackID, bool inital)
        {
            if (inital)
            {
                // 1.socket 상태 send로 변경
                if (!TryChangeSocketCondition(GSocketState.Sending))
                    return;

                var tmpbfqueue = mSendingQueue;

                if (tmpbfqueue != queue || tmpbfqueue.mTrackID != trackID)
                {
                    OnSendEnd();
                    return;
                }
            }

            CSendingQueue newqueue;
            if (!mSendingQueuePool.TryPop())
            {
                OnSendEnd();
                GCLogger.Error(nameof(AbstractAsyncSocket), $"StartSend", $"SendingPool Pop Error!!!");
                return;
            }
            else
            {
                newqueue = mSendingQueuePool.Pop();
            }

            var oldqueue = Interlocked.CompareExchange(ref mSendingQueue, newqueue, queue);
               
            if (!ReferenceEquals(oldqueue, queue))
            {
                if (newqueue == null)
                    mSendingQueuePool.Push(newqueue);

                var curstate = mSocketState;
                if (curstate == GSocketState.InClosing || curstate == GSocketState.Disconnected)
                {
                    OnSendEnd();
                }
                else
                {
                    OnSendEnd(eCloseReason.InternalError, true);
                    GCLogger.Error(nameof(AbstractAsyncSocket), $"StartSend", $"Fail to switch the sending queue");
                }

                return;
            }

            newqueue.StartEnqueue();
            queue.StopEnqueue();

            if (queue.Count == 0)
            {
                mSendingQueuePool.Push(queue);
                OnSendEnd(eCloseReason.InternalError, true);
                GCLogger.Error(nameof(AbstractAsyncSocket), $"StartSend", $"There is no data to be sent in the queue");
                return;
            }

            SendAsync(queue);
        }

        /// <summary>
        /// 현재 연결된 socket으로 send 가능한지 여부 체크
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public bool TrySend(IList<ArraySegment<byte>> segments)
        {
            if (!CheckSocketIsNormal())
                return false; 

            var queue = mSendingQueue;
            if (queue == null)
                return false;

            var trackID = queue.mTrackID;
            if (!queue.Enqueue(segments, trackID))
                return false;

            StartSend(queue, trackID, true);

            return true;
        }

        public bool TrySend(ArraySegment<byte> segment)
        {
            if (mSocketState == GSocketState.Disconnected)
                return false;

            var queue = mSendingQueue;
            if (queue == null)
                return false;

            var trackID = queue.mTrackID;
            if (!queue.Enqueue(segment, trackID))
                return false;

            StartSend(queue, trackID, true);

            return true;
        }

        /// <summary>
        /// Send 콜백함수에서 Send 완료시 후처리 작업 진행 
        /// 1. 사용한 queue 풀에 반납
        /// 2. Send 가능한 상태이면서 queue에 데이터가 남아있는 경우 Send 추가진행
        /// </summary>
        /// <param name="queue"></param>
        protected virtual void OnSendCompleted(CSendingQueue queue)
        {
            queue.Clear();
            mSendingQueuePool.Push(queue);

            var newqueue = mSendingQueue;
            if (CheckSocketIsNormal())
            {
                if (newqueue.Count > 0)
                {
                    StartSend(newqueue, newqueue.mTrackID, false);
                    return;
                }
            }
            else
            {
                OnSendEnd();
            }
        }

        /// <summary>
        /// Receive 콜백함수에서 Receive 완료시 후처리 작업 진행
        /// Receive의 경우 socket disconnect 시에만 풀에 사용한 객체 반납
        /// </summary>
        protected virtual void OnReceiveCompleted()
        {
            ReceiveAsync();
        }

        protected virtual void Dispose(bool disposed)
        {
            if (!mIsDisposed)
            {
                if (disposed)
                {
                    mSendingQueue = null;
                    localEP = null;
                    mMsgRecevier = null;
                    
                    foreach(var targetObj in mSendingQueuePool)
                    {
                        targetObj.Clear();
                    }
                    mSendingQueuePool = null;
                    
                    clientsocket.Dispose();
                }

                mIsDisposed = true;
            }
        }

        public bool ChangeState(int state)
        {
            var oldNewState = mSocketState;
            var oldState = (oldNewState & GSocketStateMask.NEW_MASK) << 16;
            
            var result = oldState | (state & GSocketStateMask.NEW_MASK);

            if (oldNewState == Interlocked.CompareExchange(ref mSocketState, result, oldNewState))
                return true;
            else
                return false;
        }

        public bool CheckOldAndCurrentState(int oldState, int newState)
        {
            //var oldNewState = mSocketState;
            var checkState = (oldState & GSocketStateMask.OLD_MASK) | (newState & GSocketStateMask.NEW_MASK);

            return mSocketState == checkState;
            //return oldNewState == Interlocked.CompareExchange(ref mSocketState, oldNewState, checkState);
        }

        public bool CheckCurrentState(int state)
        {
            return state == (mSocketState & GSocketStateMask.NEW_MASK);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Utility;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;
using ProjectWaterMelon.GameLib;

using ProjectWaterMelon.Network.Session;

namespace ProjectWaterMelon.Network.CustomSocket
{
    public sealed class CTcpAsyncSocket : AbstractAsyncSocket
    {
        public int mAlreadySendBytes { get; private set; } = 0;
        public int mHaveToSendBytes { get; private set; } = 0;

        /// <summary>
        /// 비동기 소켓 커스텀 객체에서 통신에 사용할 recv/send 비동기 통신 객체 
        /// </summary>
        private SocketAsyncEventArgs mRecvAsyncEvtObj;
        private SocketAsyncEventArgs mSendAsyncEvtObj;

        /// <summary>
        /// Session 클래스에 CTcpAsyncSocket 포함. Session 초기화 시, poolmanager 초기화
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CTcpAsyncSocket(in Socket socket, int sendingQueueSize, int queueMaxSize, ref SocketAsyncEventArgs recv, ref SocketAsyncEventArgs send, in CSessionTest session) : base(sendingQueueSize, queueMaxSize)
        {
            clientsocket = socket;

            localEP = (IPEndPoint)socket.LocalEndPoint;

            recv.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveHandler);
            recv.UserToken = session;
            
            send.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendHandler);
            send.UserToken = session;

            mRecvAsyncEvtObj = recv;
            mSendAsyncEvtObj = send;
        }

        public void SetSocketOption(bool noDelay, int recvBufferSize, int sendBufferSize, bool socketCloseDelay, int socketCloseDelayTime, bool keepAliveOpt = true)
        {
            clientsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, keepAliveOpt);
            clientsocket.LingerState = new LingerOption(socketCloseDelay, socketCloseDelayTime);
            
            if (noDelay)
                clientsocket.NoDelay = true;

            if (recvBufferSize > 0)
                clientsocket.ReceiveBufferSize = recvBufferSize;

            if (sendBufferSize > 0)
                clientsocket.SendBufferSize = sendBufferSize;

            // Send Timeout은 동기호출에서만 적용
        }

        public void Send()
        {

        }

        public void Receive()
        {
            // 비동기의 경우 ReceiveAsync 호출
            ReceiveAsync();
        }

        protected override void ReceiveAsync()
        {
            try
            {
                if (!TryReceive())
                {
                    GCLogger.Error(nameof(CTcpAsyncSocket), "ReceiveAsync", $"Socket state error[{SocketState}]");
                    return;
                }

                var lAsyncIOResult = clientsocket.ReceiveAsync(mRecvAsyncEvtObj);
                if (!lAsyncIOResult)
                {
                    OnReceiveHandler(clientsocket, mRecvAsyncEvtObj);
                }
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), "ReceiveAsync", ex);
                return;
            }
        }

        protected override void SendAsync(CSendingQueue queue)
        {
            try
            {
                if (queue.Count > 1)
                {
                    mSendAsyncEvtObj.BufferList = queue;
                }
                else
                {
                    var item = queue[0];
                    mSendAsyncEvtObj.SetBuffer(item.Array, item.Offset, item.Count);
                }

                var lAsyncIOResult = clientsocket.SendAsync(mSendAsyncEvtObj);
                if (!lAsyncIOResult)
                {
                    var socket = clientsocket;
                    OnSendHandler(socket, mSendAsyncEvtObj);
                }
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"SendAsync", ex);
                OnSendError(ref queue, eCloseReason.SocketError);
                OnClearSendData(ref mSendAsyncEvtObj);
            }
        }

        private void OnClearSendData(ref SocketAsyncEventArgs e)
        {
            e.UserToken = null;

            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            else if(e.BufferList != null)
            {
                e.BufferList = null;
            }
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

        /// <summary>
        /// Async Recv Callback Function 
        /// </summary>
        /// <param name="send">Socket Object</param>
        /// <param name="e"></param>
        public void OnReceiveHandler(object send, SocketAsyncEventArgs e)
        {
            var lUserToken = e.UserToken as CSessionTest;
            if (lUserToken == null)
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"OnReceiveHandler", $"e.UserToken cast error!!!");
                return;
            }

            if (!AsyncSocketCommonFunc.CheckCallbackHandler(e.SocketError, e.BytesTransferred))
            {
                try
                {
                    mMsgRecevier.OnReceive(lUserToken, e.Buffer, e.Offset, e.BytesTransferred);
                    OnReceiveCompleted();
                }
                catch (Exception ex)
                {
                    GCLogger.Error(nameof(CTcpAsyncSocket), $"OnReceiveHandler", ex);
                    return;
                }
            }     
            else
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"OnReceiveHandler", $"ReceAsync function error - [ErrorCode] = {e.SocketError}, [ByteTransferred] = {e.BytesTransferred.ToString()}");
                return;
            }
        }

        private void OnSendHandler(object send, SocketAsyncEventArgs e)
        {
            var lUserToken = e.UserToken as CSessionTest;
            if (lUserToken == null)
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"OnSendHandler", $"e.UserToken cast error!!!");
                return;
            }

            var queue = lUserToken.clientsocket.SendingQueue;
            if (!CheckCallbackHandler(e))
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"OnSendHandler", $"Callback check error!!! - {e.SocketError} - {e.BytesTransferred}");
                OnClearSendData(ref e);
                OnSendError(ref queue, eCloseReason.SocketError);
                return;
            }

            var count = queue.Sum(n => n.Count);
            if (e.BytesTransferred != count)
            {
                queue.InternalTrim(e.BytesTransferred);
                GCLogger.Info(nameof(CTcpAsyncSocket), $"OnSendHandler", $"{e.BytesTransferred} of {count} were transferred, send the rest {queue.Sum(n => n.Count)} bytes now");
                OnClearSendData(ref e);
                SendAsync(queue);
                return;
            }

            OnClearSendData(ref e);
            base.OnSendCompleted(queue);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Utility;
using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;

using ProjectWaterMelon.Network.Session;

namespace ProjectWaterMelon.Network.CustomSocket
{
    public sealed class CTcpAsyncSocket : AbstractAsyncSocket
    {
        public int mAlreadySendBytes { get; private set; } = 0;
        public int mHaveToSendBytes { get; private set; } = 0;
        private SocketAsyncEventArgs mSocketEventSend;
        public SocketAsyncEventArgs SocketEventSend => mSocketEventSend;
        public SocketAsyncEventArgs mSocketEventRecv { get; private set; } = new SocketAsyncEventArgs();

        /// <summary>
        /// Session 클래스에 CTcpAsyncSocket 포함. Session 초기화 시, poolmanager 초기화
        /// </summary>
        /// <param name="config"></param>
        /// <param name="socket"></param>
        /// <param name="queueMaxSize"></param>
        public CTcpAsyncSocket(IServerConfig config, in Socket socket, int queueMaxSize) : base(config.sendingQueueSize, queueMaxSize)
        {
            clientsocket = socket;
            localEP = (IPEndPoint)socket.LocalEndPoint; 
        }

        public void Send()
        {

        }

        public void SetSocketEventSendRecv(in SocketAsyncEventArgs sendEvt, in SocketAsyncEventArgs recvEvt )
        {
            mSocketEventSend = sendEvt;
            mSocketEventRecv = recvEvt;
        }

        public void SetSocketEventSend(in SocketAsyncEventArgs sendEvt)
        {
            mSocketEventSend = sendEvt;
        }

        public void SetSocketEventRecv(in SocketAsyncEventArgs recvEvt)
        {
            mSocketEventRecv = recvEvt;
        }

        protected override void ReceiveAsync()
        {
            try
            {
                var lAsyncIOResult = clientsocket.ReceiveAsync(mSocketEventRecv);
                if (!lAsyncIOResult)
                {
                    var socket = clientsocket;
                    OnReceiveHandler(socket, mSocketEventRecv);
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected override void SendAsync(CSendingQueue queue)
        {
            try
            {
                if (queue.Count > 1)
                {
                    mSocketEventSend.BufferList = queue;
                }
                else
                {
                    var item = queue[0];
                    mSocketEventSend.SetBuffer(item.Array, item.Offset, item.Count);
                }

                var lAsyncIOResult = clientsocket.SendAsync(mSocketEventSend);
                if (!lAsyncIOResult)
                {
                    var socket = clientsocket;
                    OnSendHandler(socket, mSocketEventSend);
                }
            }
            catch (Exception ex)
            {
                GCLogger.Error(nameof(CTcpAsyncSocket), $"SendAsync", ex);
                OnSendError(ref queue, eCloseReason.SocketError);
                OnClearSendData(ref mSocketEventSend);
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

            if (!CheckCallbackHandler(e))
            {
                mMsgRecevier.OnReceive(lUserToken, e.Buffer, e.Offset, e.BytesTransferred);
                base.OnReceiveCompleted();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Log;
using ProjectWaterMelon.Utility;

namespace ProjectWaterMelon.Network.CustomSocket
{
    public abstract partial class AAsyncSocket : IAsyncSocketBase
    {
        /// <summary>
        /// 비동기 소켓 
        /// </summary>
        public Socket socket { get; private set; }

        /// <summary>
        /// 호스트 주소 
        /// </summary>
        protected IPEndPoint mIPEndPoint { get; private set; }

        /// <summary>
        /// 패킷 메시지 Recv 처리 클래스
        /// </summary>
        public CMessageResolver mReceiver = new CMessageResolver();
         
        /// <summary>
        /// Send/Recv 비동기 콜백함수 조건 체크 
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
            else
            {
                // 20211103 GetLogger 로거 세팅필요
                GCLogger.Error(nameof(AAsyncSocket), "CheckCallbackHandler", $"SocketError = {e.SocketError} - BytesTransferred = {e.BytesTransferred}");
            }

            return false;
        }

        public abstract void StartSend(in CPacket packet);

        public abstract void StartRecv();

        public abstract bool AsyncSend(in CPacket packet);

        public abstract bool AsyncRecv();

        public void OnSendHandler(object sender, SocketAsyncEventArgs e)
        {
            if (!CheckCallbackHandler(e))
                return;

            if (e.UserToken is CSession lUserToken)
            {
                if (e.BytesTransferred == lUserToken.mTcpSocket.mHaveToSendBytes)
                {
           
                }
                else
                {

                }
            }
            else
            {
                GCLogger.Error(nameof(AAsyncSocket), "OnSendHandler", $"CSession Casting Error!!!");
            }
        }

        public void OnRecvHandler(object sender, SocketAsyncEventArgs e)
        {

        }

        public void ClearSendData(in SocketAsyncEventArgs e)
        {
            e.UserToken = null;

            if (e.Buffer != null)
            {
                e.SetBuffer(null, 0, 0);
            }
            else if (e.BufferList != null)
            {
                e.BufferList = null;
            }         
        }
       
        public void ClearRecvData()
        {

        }

        public void Close(eSocketCloseReason reason)
        {

        }

        public void OnSendError(CSendingQueue queue ,CloseReason reason)
        {
            queue.Clear();

        }
    }
}

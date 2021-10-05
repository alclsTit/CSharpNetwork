using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
// --- custom --- //
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using static ConstModule.ConstDefine;
// -------------- //

namespace ProjectWaterMelon.Network.CustomSocket
{
    // Session(UserToken)개별 소유
    public sealed class CTcpSocket : CSocketBase
    {
        private bool mIsDisposed = false;
        // 메시지 Recv 처리 
        private CMessageResolver mMessageReceiver = new CMessageResolver();
        
        // 메시지 Send 처리
        private int mAlreadySendBytes;
        private int mHaveToSendBytes;

        // tcp 소켓의 경우 소켓 송, 수신 버퍼 사용 (클라 -> 송신 버퍼 -> 전송 -> 수신 버퍼 -> 서버)
        private ConcurrentQueue<CPacket> mSendPacketQ = new ConcurrentQueue<CPacket>();
        private ConcurrentQueue<CPacket> mRecvPacketQ = new ConcurrentQueue<CPacket>();

        public System.Net.IPEndPoint mRemoteEP { get; private set;}

        public CTcpSocket()
        {
        }

        // Accept 이후 비동기 소켓 통신을 위해 사용되는 소켓 세팅 
        public void SetSocket(in Socket socket)
        {
            base.mRawSocket = socket;
            SetSocketOpt();
        }

        public void SetSocketOpt()
        {
            // nagle 알고리즘은 리얼타임 어플리케이션에서는 성능이 안좋음(반응속도가 느려지기 때문에)
            // nagle 알고리즘을 사용하지 않는다. (네트워크 트래픽 부하 감소 보단 성능선택)
            base.mRawSocket.NoDelay = true;
            base.mRawSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            base.mRawSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        }

        public void SetRemoteIPEndPoint(in System.Net.IPEndPoint IPEndPoint)
        {
            mRemoteEP = IPEndPoint;
        }
  
        protected override sealed void Dispose(bool isDisposed) 
        {
            if (mIsDisposed)
                return;

            if (isDisposed)
            {
                // dispose managed resource
                mMessageReceiver = null;
            }
            // dispose unmanaged resource

            mIsDisposed = true;

            Dispose(isDisposed);
        }

        public void PushPacketToSendQ(CPacket packet)
        {
            mSendPacketQ.Enqueue(packet);
        }

        public bool TryPopPacketForSendQ(out CPacket packet)
        {
            return mSendPacketQ.TryDequeue(out packet);
        }

        public bool TryPeekPacketForSendQ(out CPacket packet)
        {
            return mSendPacketQ.TryPeek(out packet);
        }

        public void AsyncSend<T>(Protocol.PacketId msgId, T handler) where T : class
        {
            CPacket packet = new CPacket(this, CProtobuf.ProtobufSerialize<T>(handler), msgId);

            if (mRawSocket == null)
            {
                CLog4Net.LogError($"Error in CTcpSocket.AsyncSend - Socket NULL Error");
                return;
            }

            if (!IsAbleToSend())
            {
                CLog4Net.LogError($"Error in CTcpSocket.AsyncSend - Socket state isn't to send packet");
                return;
            }

            if (!packet.CheckValidate())
            {
                CLog4Net.LogError($"Error in CTcpSocket.AsyncSend - Packet CheckValidate Error");
                return;
            }

            // thread-safe 
            mSendPacketQ.Enqueue(packet);
            StartSend(packet);
        }
        public void AsyncSend(Protocol.PacketId messageId, IMessageBase handler)
        {
            CPacket packet = new CPacket();


        }
        public void StartSend(in CPacket packet)
        {
            try
            {
                if (mAlreadySendBytes == 0)
                {
                    // 해당 패킷을 처음 보내는 경우
                    mHaveToSendBytes = packet.GetTotalBufferSize();
                    // 송신 버퍼 세팅
                    mSendArgs.SetBuffer(new byte[packet.GetTotalBufferSize()], mAlreadySendBytes, packet.GetTotalBufferSize());
                    // 패킷 버퍼에 담긴 내용을 비동기 소켓 전달 객체인 SocketAsyncEventArgs 버퍼(송신버퍼)에 담는다
                    Buffer.BlockCopy(packet.mMsgHeaderBuffer, 0, mSendArgs.Buffer, mSendArgs.Offset, packet.GetHeaderBufferSize());
                    Buffer.BlockCopy(packet.mMsgBuffer, 0, mSendArgs.Buffer, packet.GetHeaderBufferSize(), packet.GetBodyBufferSize());
                }
                else
                {
                    // 해당 패킷을 한번에 보내지 못한 경우
                    mHaveToSendBytes = packet.GetTotalBufferSize() - mAlreadySendBytes;

                    // 송신버퍼 세팅
                    mSendArgs.SetBuffer(mAlreadySendBytes, mHaveToSendBytes);
                    if (mAlreadySendBytes < packet.GetHeaderBufferSize())
                    {
                        // 1. 헤더 패킷의 데이터를 모두 보내지 못한 경우, 송신버퍼에 남은 헤더 및 바디 패킷데이터를 담는다
                        var lLeftHeaderSendBytes = packet.GetHeaderBufferSize() - mAlreadySendBytes;
                        Buffer.BlockCopy(packet.mMsgHeaderBuffer, mAlreadySendBytes, mSendArgs.Buffer, mSendArgs.Offset, lLeftHeaderSendBytes);
                        Buffer.BlockCopy(packet.mMsgBuffer, 0, mSendArgs.Buffer, mSendArgs.Offset + lLeftHeaderSendBytes, packet.GetBodyBufferSize());
                    }
                    else
                    {
                        // 2. 헤더 패킷의 데이터를 모두 보낸 경우, 송신버퍼에 남은 바디 패킷 데이터를 담는다
                        Buffer.BlockCopy(packet.mMsgBuffer, mAlreadySendBytes, mSendArgs.Buffer, mSendArgs.Offset, mHaveToSendBytes);
                    }
                }

                bool lPending = mRawSocket.SendAsync(mSendArgs);
                if (!lPending)
                {
                    OnSendHandler(this, mSendArgs);
                }
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CTcpSocket.StartSend({(mSendArgs.UserToken as CSession)?.mSessionID.ToString()}) - {ex.Message} - {ex.StackTrace}");
            }
        }

        public void OnBadSendHandler(in SocketAsyncEventArgs e)
        {
            var lUserToken = e.UserToken as CSession;
            var lSessionID = 0L;
            if (lUserToken != null)
            {
                lSessionID = lUserToken.mSessionID;
                lUserToken.mTcpSocket?.Disconnect();

                // TODO: lUserToken 객체 초기화
            }

            CLog4Net.LogError($"Error in CTcpSocket.OnSendHandler - Packet Send Error(SessionID = {lSessionID}, BytesTransferred = {e.BytesTransferred}, SocketError = {e.SocketError}, SendQueueSize = {mSendPacketQ.Count}");
        }

        // Send Handler after operating async send
        public void OnSendHandler(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    CSession lUserToken = e.UserToken as CSession;
                    if (lUserToken != null)
                    {
                        CLog4Net.LogDebugSysLog($"5.CTcpSocket.OnSendHandler({lUserToken.mSessionID})", $"Async Send End(Success)(total = {e.BytesTransferred}, sent = {lUserToken.mTcpSocket.mHaveToSendBytes})");

                        CPacket packet = new CPacket();
                        if (e.BytesTransferred == lUserToken.mTcpSocket.mHaveToSendBytes)
                        {
                            lUserToken.mTcpSocket.mAlreadySendBytes = e.BytesTransferred;
                            if (!lUserToken.mTcpSocket.TryPopPacketForSendQ(out packet))
                                CLog4Net.LogError($"Error in CTcpSocket.OnSendHandler - Packet Send queue Pop Error");
                        }
                        else
                        {
                            // 패킷을 정상적으로 모두 보내지 못했을 때
                            lUserToken.mTcpSocket.mAlreadySendBytes = e.BytesTransferred;
                            if (lUserToken.mTcpSocket.TryPeekPacketForSendQ(out packet))
                            {
                                StartSend(packet);
                            }
                            else
                            {
                                CLog4Net.LogError("$Error in CTcpSocket.OnSendHandler - Packet Send queue Peek Error");
                            }
                        }
                    }
                }
            }
            else
            {
                // 패킷 메시지 정상 전송 실패, 후처리 작업 진행 
                OnBadSendHandler(e);
            }
        }

        public void AsyncRecv()
        {

        }

        // Recv Handler after operating async recv
        public void OnReceiveHandler(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {             
                if (e.BytesTransferred > 0)
                {
                    // 2021-09-20 테스트
                    /*
                    var notify_msg = new Protocol.msg_test.hanlder_notify_test_packet_game2user();
                    notify_msg.cur_datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    AsyncSend<Protocol.msg_test.hanlder_notify_test_packet_game2user>(notify_msg.msg_id, notify_msg);
                    */
                    mMessageReceiver.OnReceive(this, e.Buffer, e.Offset, e.BytesTransferred);
                    var lPending = mRawSocket.ReceiveAsync(e);
                    if (!lPending)
                    {
                        OnReceiveHandler(this, mRecvArgs);
                    }
                }
                else
                {
                    // 서버에서 받은 패킷 데이터가 없는 경우(수신된 패킷 데이터 사이즈 = 0)
                    // 클라로 부터 패킷 전송이 안된 경우 받은 데이터가 없을 수 있다 
                    //CLog4Net.LogError($"Exception in CTcpSocket.OnReceiveHandler - Packet receive bytesTransferred error!!!(BytesTransferred = {e.BytesTransferred})");
                    //return;
                }
            }
            else
            {
                // 소켓 통신 에러 - 리시브 에러 
                CLog4Net.LogError($"Exception in CTcpSocket.OnReceiveHandler - Data receive error!!!(BytesTransferred = {e.BytesTransferred}, SocketError = {e.SocketError})");
                Disconnect();
                return;
            }
        }

        // TODO
        // 나중에 필요없으면 삭제 
        public override void Disconnect()
        {
            base.Disconnect();
        }
    }
}



/*
public void AsyncSend()
{
    var lSentMsgCount = 0;

    if (mSocket == null)
    {
        CLog4Net.LogError($"Exception in CTcpSocket.AsyncSend - Socket NULL Error!!!");
        return;
    }

    if (IsAbleToSend() == false)
    {
        CLog4Net.LogError($"Exception in CTcpSocket.AsyncSend - Socket state isn't to send packet!!!");
        return;
    }

    while (lSentMsgCount < mSendPacketQ.Count)
    {
        CPacket packet = new CPacket();
        if (!TryPeekPacketForSendQ(ref packet))
            break;

        mSendArgs.SetBuffer(mSendArgs.Offset, packet.mSize);

        Array.Copy(packet.mMsgBuffer, 0, mSendArgs.Buffer, mSendArgs.Offset, packet.mSize);

        mSentPacketQ.Enqueue(packet);
        bool lPending = mSocket.SendAsync(mSendArgs);
        if (!lPending)
        {
            OnSendHandler(this, mSendArgs);
        }
        ++lSentMsgCount;
    }         
}
*/

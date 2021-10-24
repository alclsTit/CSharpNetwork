using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
// --- custom --- //
using ProjectWaterMelon.Network.MessageWorker;
using ProjectWaterMelon.Network.Packet;
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.ConstDefine;
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

        public EndPoint mRemoteEP { get; private set;}
        public EndPoint mLocalEP { get; private set; }

        // tcp 소켓의 경우 소켓 송, 수신 버퍼 사용 (클라 -> 송신 버퍼 -> 전송 -> 수신 버퍼 -> 서버)
        private ConcurrentQueue<CPacket> mSendPacketQ = new ConcurrentQueue<CPacket>();
        private ConcurrentQueue<CPacket> mRecvPacketQ = new ConcurrentQueue<CPacket>();

        //private AutoResetEvent mFlowControlEvt = new AutoResetEvent(false);

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
            base.mRawSocket.NoDelay = true;
            base.mRawSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            base.mRawSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);          
        }

        public void SetRemoteAndLocalEP(EndPoint remoteEP, EndPoint localEP)
        {
            mRemoteEP = remoteEP;
            mLocalEP = localEP;
        }

        protected override sealed void Dispose(bool isDisposed) 
        {
            if (mIsDisposed)
                return;

            if (isDisposed)
            {
                // dispose managed resource
                mMessageReceiver = null;
                mSendPacketQ = null;
                mRecvPacketQ = null;
                //mFlowControlEvt = null;
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

        public ConcurrentQueue<CPacket> GetPacketSendQ()
        {
            return mSendPacketQ;
        }

        public void Relay<T>(Protocol.PacketId msgid, T hander, bool directFlag = false) where T : class
        {
            CPacket lPacket = new CPacket(this, CProtobuf.ProtobufSerialize<T>(hander), directFlag, msgid);

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

            if (!lPacket.CheckValidate())
            {
                CLog4Net.LogError($"Error in CTcpSocket.AsyncSend - Packet CheckValidate Error");
                return;
            }

            if (directFlag)
            {
                StartSend(lPacket);
            }
            else
            {
                // thread-safe 
                // 20211013 SendQ 패킷처리 변경(CMessageProcessManager -> TcpSocket의 mSendPacketQ)
                //CMessageProcessorManager.PushPacketToSendQ(lPacket);
                PushPacketToSendQ(lPacket);
            }
        }
       

        public void StartSend(in CPacket packet)
        {
            try
            {
                packet.SetSendingStart();
                if (mAlreadySendBytes == 0)
                {
                    // 해당 패킷을 처음 보내는 경우
                    mHaveToSendBytes = packet.GetTotalSize();
                    // 송신 버퍼 세팅
                    mSendArgs.SetBuffer(new byte[mHaveToSendBytes], 0, mHaveToSendBytes);
                    // 패킷 버퍼에 담긴 내용을 비동기 소켓 전달 객체인 SocketAsyncEventArgs 버퍼(송신버퍼)에 담는다
                    Buffer.BlockCopy(BitConverter.GetBytes(packet.GetHeaderSize()), 0, mSendArgs.Buffer, mSendArgs.Offset, MAX_PACKET_HEADER_SIZE);
                    Buffer.BlockCopy(packet.mMsgHeaderBuffer, 0, mSendArgs.Buffer, MAX_PACKET_HEADER_SIZE, packet.GetHeaderSize());
                    Buffer.BlockCopy(packet.mMsgBuffer, 0, mSendArgs.Buffer, MAX_PACKET_HEADER_SIZE + packet.GetHeaderSize(), packet.GetBodySize());
                }
                else
                {
                    // 해당 패킷을 한번에 보내지 못한 경우
                    mHaveToSendBytes = packet.GetTotalSize() - mAlreadySendBytes;

                    // 송신버퍼 세팅
                    mSendArgs.SetBuffer(mAlreadySendBytes, mHaveToSendBytes);
                    if (mAlreadySendBytes < MAX_PACKET_HEADER_SIZE)
                    {
                        // 1. 헤더 사이즈를 담은 데이터 조차 보내지 못한 경우, 송신버퍼에 헤더사이즈 + 헤더 + 바디 패킷데이터를 담는다
                        var lLeftOnlyHeaderSize = MAX_PACKET_HEADER_SIZE - mAlreadySendBytes;
                        Buffer.BlockCopy(BitConverter.GetBytes(packet.GetHeaderSize()), mAlreadySendBytes, mSendArgs.Buffer, mSendArgs.Offset, lLeftOnlyHeaderSize);
                        Buffer.BlockCopy(packet.mMsgHeaderBuffer, 0, mSendArgs.Buffer, mAlreadySendBytes + lLeftOnlyHeaderSize, packet.GetHeaderSize());
                        Buffer.BlockCopy(packet.mMsgBuffer, 0, mSendArgs.Buffer, mAlreadySendBytes + lLeftOnlyHeaderSize + packet.GetHeaderSize(), packet.GetBodySize());
                    }
                    else if (mAlreadySendBytes >= MAX_PACKET_HEADER_SIZE && mAlreadySendBytes < packet.GetHeaderSize())
                    {
                        // 2. 헤더 패킷의 데이터를 모두 보내지 못한 경우, 송신버퍼에 남은 헤더 및 바디 패킷데이터를 담는다
                        var lLeftHeaderClassSendBytes = MAX_PACKET_HEADER_SIZE + packet.GetHeaderSize() - mAlreadySendBytes;
                        Buffer.BlockCopy(packet.mMsgHeaderBuffer, mAlreadySendBytes - MAX_PACKET_HEADER_SIZE, mSendArgs.Buffer, mSendArgs.Offset, lLeftHeaderClassSendBytes);
                        Buffer.BlockCopy(packet.mMsgBuffer, 0, mSendArgs.Buffer, MAX_PACKET_HEADER_SIZE + lLeftHeaderClassSendBytes, packet.GetBodySize());
                    }
                    else
                    {
                        // 3. 헤더 패킷의 데이터를 모두 보낸 경우, 송신버퍼에 남은 바디 패킷 데이터를 담는다
                        Buffer.BlockCopy(packet.mMsgBuffer, packet.GetBodySize() - mHaveToSendBytes, mSendArgs.Buffer, mSendArgs.Offset, mHaveToSendBytes);
                    }
                }

                bool lPending = mRawSocket.SendAsync(mSendArgs);
                if (!lPending)
                {
                    OnSendHandler(this, mSendArgs);
                }
                //mFlowControlEvt.WaitOne();
            }
            catch (Exception ex)
            {
                CLog4Net.LogError($"Exception in CTcpSocket.StartSend({(mSendArgs.UserToken as CSession)?.mSessionID.ToString()}) - {ex.Message} - {ex.StackTrace}");
            }
        }

        public void OnBadSendHandler(in SocketAsyncEventArgs e)
        {
            if (e.UserToken is CSession lUserToken)
            {
                var lSessionID = lUserToken.mSessionID;
                lUserToken.mTcpSocket.Disconnect();

                // TODO: lUserToken 객체 초기화
                CLog4Net.LogError($"Error in CTcpSocket.OnBadSendHandler - Packet Send Error(SessionID = {lSessionID}, BytesTransferred = {e.BytesTransferred}, SocketError = {e.SocketError}, SendQueueSize = {mSendPacketQ.Count}");
            }
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
                        
                        if (e.BytesTransferred == lUserToken.mTcpSocket.mHaveToSendBytes)
                        {
                            // 패킷을 정상적으로 모두 보냈을 때 (큐잉된 패킷은 pop)
                            CPacket packet = new CPacket();
                            lUserToken.mTcpSocket.TryPopPacketForSendQ(out packet);

                            lUserToken.mTcpSocket.mHaveToSendBytes = 0;
                            lUserToken.mTcpSocket.mAlreadySendBytes = 0;
                        }
                        else
                        {
                            // 패킷을 정상적으로 모두 보내지 못했을 때
                            lUserToken.mTcpSocket.mAlreadySendBytes = e.BytesTransferred;

                            // 같은 Session에서 보낸 패킷전송은 큐에 순차적으로 들어가있기 때문에 
                            // 미전송된 패킷 처리도 순차처리해도 된다 
                            CPacket packet = new CPacket();
                            if (lUserToken.mTcpSocket.TryPeekPacketForSendQ(out packet))
                                StartSend(packet);
                            else  
                                CLog4Net.LogError("$Error in CTcpSocket.OnSendHandler - Packet Send queue Peek Error");
                        }
                    }
                }
            }
            else
            {
                // 패킷 메시지 정상 전송 실패, 후처리 작업 진행 
                OnBadSendHandler(e);
            }
            //mFlowControlEvt.Set();
        }   

        // Recv Handler after operating async recv
        public void OnReceiveHandler(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {              
                if (e.BytesTransferred > 0 && e.UserToken is CSession lUserToken)
                {
                    mMessageReceiver.OnReceive(lUserToken, e.Buffer, e.Offset, e.BytesTransferred);
                    var lPending = lUserToken.mTcpSocket.mRawSocket.ReceiveAsync(e);
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

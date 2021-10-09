using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
// --- custom --- //
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.ConstDefine;
using static ProjectWaterMelon.GSocketState;
// -------------- //

namespace ProjectWaterMelon.Network.CustomSocket
{
    public class CSocketBase : IDisposable
    {
        private bool mIsDisposed = false;
        public Socket mRawSocket { get; protected set; }
        public SocketAsyncEventArgs mSendArgs { get; protected set; }
        public SocketAsyncEventArgs mRecvArgs { get; protected set; }
        public short mSocketState { get; private set; }
        private int mHeartBeatGauge;
        private bool mHeartBeatOnOff, mReconnectOnOff;

        private System.Timers.Timer m_heartbeat_timer = new System.Timers.Timer();
        private System.Timers.Timer m_check_connection_timer = new System.Timers.Timer();
        private System.Timers.Timer mReconnectTimer = new System.Timers.Timer();

        public bool mIsConnected { get; private set; }
        public uint mReconnectCount { get; set; } = MAX_SOCKET_RECONNECT_COUNT;

        ~CSocketBase()
        {
            Dispose(true);
        }

        public void SetEventArgs(SocketAsyncEventArgs recvArgs, SocketAsyncEventArgs sendArgs)
        {
            mRecvArgs = recvArgs;
            mSendArgs = sendArgs;
        }

        public void SetSendRecvArgs(eSocketType type, SocketAsyncEventArgs args)
        {
            switch (type)
            {
                case eSocketType.RECV:
                    mRecvArgs = args;
                    break;
                case eSocketType.SEND:
                    mSendArgs = args;
                    break;
                default:
                    CLog4Net.LogError("Exception in CSocketBase.SetSendRecvArgs!!! - Socket Type Set Error");
                    break;
            }
        }


        public void CreateTimers()
        {
            m_heartbeat_timer.Interval = MAX_HEARTBEAT_INTERVAL;
            m_heartbeat_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnHeartBeatTimerHandler);
            m_heartbeat_timer.Enabled = true;

            /*
            m_check_connection_timer.Interval = MAX_CHECK_CONNECTION_INTERVAL;
            m_check_connection_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnCheckConnectionTimerHandler);
            m_check_connection_timer.AutoReset = true;
            m_check_connection_timer.Enabled = true;
            */
        }

        public void ReconnectTimer()
        {
            mReconnectOnOff = true;
            mReconnectTimer.Interval = MAX_RECONNECT_INTERVAL;
            mReconnectTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnReconnectTimerHandler);
            mReconnectTimer.Enabled = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposed)
        {
            if (mIsDisposed)
                return;

            if (isDisposed)
            {
                // dispose managed resource
                DisposeAllTimers();
                m_heartbeat_timer.Dispose();
                //m_check_connection_timer.Dispose();
            }
            // dispose unmanaged resource
            mRawSocket = null;

            mIsDisposed = true;
        }

        public void DisposeAllTimers()
        {
            OnCancelHeartBeatTimer();
            //OnCheckConnectionTimer();
        }

        public void OnHeartBeatTimerHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mIsConnected)
            {
                OnCancelHeartBeatTimer();
                // [주석] CSocketBase 로그 추가 - Socket Disconnect
                return;
            }

            if (mHeartBeatGauge <= 0)
            {
                // [주석] CSocketBase 로그 추가 - Socket Disconnect // 하트비트 기준 (20초 * 8연속) 초과
                Disconnect();
                return;
            }

            --mHeartBeatGauge;

            // 하트비트 메시지 클라이언트에 전송 
        }

        public void OnReconnectTimerHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mIsConnected)
            {
                OnCancelReconnectTimer();
                CLog4Net.gLog4Net.InfoFormat($"CSocketBase.OnReconnectTimerHandler - Already Socket Reconnected!!!");
                return;
            }
            else
            {
                // DOTO: Reconnect 기능 구현 (소켓 재접속)
                if (mReconnectCount >= 0)
                {
                    mReconnectCount -= 1;
                }
                else
                {
                    OnCancelReconnectTimer();
                    Disconnect();
                }

            }
        }

        /*
        public void OnCheckConnectionTimerHandler(object sender, System.Timers.ElapsedEventArgs e)
        {

        }
        */

        public void OnCreateHeartBeatTimer()
        {
            if (mIsConnected == false)
            {
                // [주석] CSocketBase 로그 추가 - Socket Disconnect
                return;
            }

            if (mHeartBeatOnOff)
                OnCancelHeartBeatTimer();

            mHeartBeatOnOff = true;
            m_heartbeat_timer.Enabled = true;
            m_heartbeat_timer.Start();
        }

        /*
        public void OnCreateCheckConnectionTimer()
        {
            if (IsSocketConnected == false)
            {
                // [주석] CSocketBase 로그 추가 - Socket Disconnect
                return;
            }

            if (mCheckConnectionOnOff)
                OnCheckConnectionTimer();

            mCheckConnectionOnOff = true;
            m_check_connection_timer.Enabled = true;
            m_check_connection_timer.Start();
        }
        */

        public void OnCancelHeartBeatTimer()
        {
            mHeartBeatOnOff = false;
            m_heartbeat_timer.Stop();
            m_heartbeat_timer.Enabled = false;
        }

        public void OnCancelReconnectTimer()
        {
            mReconnectOnOff = false;
            mReconnectTimer.Stop();
            mReconnectTimer.Enabled = false;
        }

        /*
        public void OnCheckConnectionTimer()
        {
            mCheckConnectionOnOff = false;  
            m_check_connection_timer.Stop();
            m_check_connection_timer.Enabled = false;
        }
        */

        public void SetSocketConnected(bool flag)
        {
            mIsConnected = flag;
            mSocketState = (short)eSocketState.CONNECTED;
        }

        protected bool IsAbleToSend()
        {
            if (mSocketState == (short)eSocketState.CONNECTING ||
                mSocketState == (short)eSocketState.DISCONNECTED ||
                mSocketState == (short)eSocketState.DISABLED)
                return false;
            else
                return true;
        }

        public void SetSocketState(short state)
        {
            short oldstate = mSocketState;
            mSocketState = state;
            OnSocketStateChanged(oldstate, state);
        }

        // 소켓 연결 끊어짐 
        public void OnSocketStateChanged(short oldState, short curState)
        {
            if (oldState == (short)eSocketState.CONNECTED && curState != (short)eSocketState.CONNECTED)
            {
                //하트비트 캔슬

            }
        }

        public virtual void Disconnect()
        {
            if (!mIsConnected)
            {
                // [주석] CSocketBase 로그 추가 - Already Socket Disconnect   
                CLog4Net.LogError("Exception in CSocketBase.Disconnect!!! - Socket is already destroyed");
                return;
            }

            mSocketState = (short)eSocketState.DISCONNECTED;
            mRawSocket.Shutdown(SocketShutdown.Both);
            // 소켓 닫을 때 관리, 비관리 리소스 모두 Close를 통해 해제한다(내부적으로 Dispose 호출함)
            mRawSocket.Close();

            // 소켓 다시 연결?

            // [주석] CSocketBase 로그 추가 - Socket Disconnected
        }
    }
}

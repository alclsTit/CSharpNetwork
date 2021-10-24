using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.GSocketState;

namespace Network.SystemLib
{
    class CHeartbeatManager
    {
        private CSession mSession = new CSession();
        private System.Timers.Timer mHeartbeatTimer = new System.Timers.Timer();

        public CHeartbeatManager()
        {
            Init();
        }

        public CHeartbeatManager(in CSession session)
        {
            mSession = session;
            Init();
        }

        public void Init()
        {
            mHeartbeatTimer.Enabled = false;
            mHeartbeatTimer.Interval = MAX_HEARTBEAT_INTERVAL;
            mHeartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnHeartbeatHandler);
            mHeartbeatTimer.AutoReset = true;
        }

        public void TimerOff()
        {
            mHeartbeatTimer.Enabled = false;
            mHeartbeatTimer.Stop();
        }

        public void TimerOn()
        {
            mHeartbeatTimer.Enabled = true;
            mHeartbeatTimer.Start();
        }

        public void ChgTimerState(bool autoreset, double interval)
        {
            mHeartbeatTimer.Interval = interval;
            mHeartbeatTimer.AutoReset = autoreset;
        }

        public void OnHeartbeatHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!mSession.mTcpSocket.mIsConnected)
            {
                CLog4Net.LogError($"Error in CHeartbeatManager.OnHeartbeatHandler - Socket is already closed!!!");
                return; 
            }


            
        }
    }
}

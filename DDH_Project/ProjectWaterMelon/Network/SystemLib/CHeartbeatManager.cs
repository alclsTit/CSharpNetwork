using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// --- custom --- //
using ProjectWaterMelon.Network.Session;
using ProjectWaterMelon.Log;
using static ProjectWaterMelon.GSocketState;

namespace ProjectWaterMelon.Network.SystemLib
{
    // 하트비트, 서버 모니터링 
    class CHeartbeatManager
    {
        private CSession mSession = new CSession();
        private System.Timers.Timer mHeartbeatTimer = new System.Timers.Timer();
        private System.Timers.Timer mMoniterTimer = new System.Timers.Timer();

        public CHeartbeatManager()
        {
            Init();
        }

        public CHeartbeatManager(in CSession session)
        {
            mSession = session;
            Init();
        }

        ~CHeartbeatManager()
        {
            HeartbeatTimerOff();
            MoniterTimerOff();
        }

        public void Init()
        {
            mHeartbeatTimer.Enabled = false;
            mMoniterTimer.Enabled = false;  

            mHeartbeatTimer.Interval = MAX_HEARTBEAT_INTERVAL * 1000;
            mHeartbeatTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnHeartbeatHandler);

            mMoniterTimer.Interval = MAX_SERVER_MONITER_INTERVAL * 1000;
            mMoniterTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnMoniterHandler);
        }

        public void HeartbeatTimerOff()
        {
            mHeartbeatTimer.Enabled = false;
            mHeartbeatTimer.Stop();
        }

        public void HeartbeatTimerOn()
        {
            mHeartbeatTimer.Enabled = true;
            mHeartbeatTimer.Start();
        }

        public void MoniterTimerOff()
        {
            mMoniterTimer.Enabled = false;
            mMoniterTimer.Stop();
        }

        public void MoniterTimerOn()
        {
            mMoniterTimer.Enabled = true;
            mMoniterTimer.Start();
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

            // Todo: 소켓 디스커넥션을 위해서 주기적으로 하트비트 체크 및 후작업 진행
            
        }

        public void OnMoniterHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine($"Server Sate => [TotalSession = {CSessionManager.Count()}]");
        }
    }
}

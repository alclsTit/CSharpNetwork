using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProjectWaterMelon.Network.SystemLib
{
    public interface IThreadBase
    {
        int id { get; }

        string jobName { get; }

        bool isBackground { get; }

        Thread thread { get; }

        bool isRunning { get; }
    }
   
    public class CThreadBase : IThreadBase
    {
        public int id { get; private set; }

        public string jobName { get; private set; }

        public bool isBackground { get; private set; }

        public Thread thread { get; private set; }

        public bool isRunning { get; private set; } = false;

        public CThreadBase(int id, string jobName, bool isBackground, in Thread thread)
        {
            this.id = id;
            this.jobName = jobName;
            this.isBackground = isBackground;
            this.thread = thread;
        }

        public void SetState(bool flag)
        {
            isRunning = flag;
        }
    }
}

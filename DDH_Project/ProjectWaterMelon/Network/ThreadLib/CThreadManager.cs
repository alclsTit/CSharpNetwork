using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectWaterMelon.Network.ThreadLib
{
    public sealed class CThreadManager
    {
        private static int msCustomThreadCount = 0;
        private static int msCustomThreadMax => Environment.ProcessorCount;
        private static int msCustomThreadAvailableCount => msCustomThreadMax - msCustomThreadCount;

        private Dictionary<string, Thread> mCustomThreads = new Dictionary<string, Thread>();


        public async Task<bool> TryAddThread(string name, Action work, bool isBackground = false)
        {
            return await AddThread(name, work, isBackground);
        }

        /// <summary>
        /// 관리대상이 되는 Thread 추가. 직접 Thread를 생성하여 관리하는 대상은 많지 않을 것이므로 따로 객체풀링하지 않음
        /// </summary>
        /// <param name="work"></param>
        public Task<bool> AddThread(string name, Action work, bool isBackground)
        {
            if (msCustomThreadCount >= msCustomThreadMax)
                return Task.FromResult<bool>(false);

            var threadObj = new Thread(new ThreadStart(work));
            threadObj.Name = name;
            threadObj.IsBackground = isBackground;

            mCustomThreads.Add(name, threadObj);

            ++msCustomThreadCount;

            return Task.FromResult<bool>(true);
        }

        public bool IsThreadAlive(string name)
        {
            return mCustomThreads[name].IsAlive;
        }

        public ThreadState GetThreadState(string name)
        {
            return mCustomThreads[name].ThreadState;
        }

        public async Task StartThreads()
        {
            await Task.Run(() =>
            {
                foreach (var threadObj in mCustomThreads)
                {
                    threadObj.Value.Start();
                }
            });
        }

        public IEnumerator<KeyValuePair<string, Thread>> GetCustomThreads()
        {
            using(var lEnumerator = mCustomThreads.GetEnumerator())
            {
                while (lEnumerator.MoveNext())
                    yield return lEnumerator.Current;
            }
        }
    }
}

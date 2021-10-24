using System;
using System.Threading;
// --- custom --- //
using ProjectWaterMelon.Network.SystemLib;
using ProjectWaterMelon.Network.MessageWorker;
// -------------- //

namespace ProjectWaterMelon
{
    class MyGame
    {
        public static void Main()
        {
            // register MessageHandler
            CMessageReceiver.Init();

            CConnector lConnector = new CConnector();
            lConnector.Init(8800);

            Thread lAsyncConnectThread = new Thread(lConnector.Start);
            lAsyncConnectThread.Start();

            CThreadWorker lWorkerThread = new CThreadWorker();
            lWorkerThread.Start();

            while (true)
            {
                Thread.Sleep(1000);
            }

        }

    }
}

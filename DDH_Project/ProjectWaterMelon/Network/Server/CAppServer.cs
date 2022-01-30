using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProjectWaterMelon.Network.Config;
using ProjectWaterMelon.Log;

namespace ProjectWaterMelon.Network.Server
{
    public class CAppServer : CAppServerBase
    {
        private int mServerState = GServerState.NotInitialized;

        public int ServerState => mServerState;

        public override void Initialize()
        {
            throw new NotImplementedException();
        }
        public override void Setup()
        {
            throw new NotImplementedException();
        }

        // Todo: 반환 값 변경, 어느 부분에서 문제가 생긴건지 정보 전달 필요
        private async Task<bool> SubSetup(IServerConfig serverConfig, IListenConfig listenConfig, IAppServer server)
        {
            try
            {
                if (await SetupBasic(serverConfig))
                {
                    if (await SetupListener(listenConfig, server))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex) 
            {
                GCLogger.Error(nameof(CAppServer), "SubSetup", ex);
                return false;
            }
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

    }
}

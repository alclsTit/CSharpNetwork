using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace WebSystem
{
    /// <summary>
    /// 컨텐츠 별 웹 전송 데이터세팅 및 전송진행 메서드 구현
    /// </summary>

    public enum eHTTPTYPE
    {
        GET = 1,
        POST = 2
    }

    public class CWebCotentManager 
    {
        public CWebCotentManager(string iniFileName)
        {
            LoadConfig(iniFileName);
        }

        /// <summary>
        /// Ini 파일 로드
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadConfig(string fileName)
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(exePath, @"..\..\..\..\")) + @"Config\" + $"{fileName}.ini";

            var temp = GameLib.IniConfig.IniFileRead("URL", "TEST", "http://192.168.0.12:8800", filePath);

            DoHttpReqest(eHTTPTYPE.GET, temp);
        }

        public void DoHttpReqest(eHTTPTYPE type, string url, string param = "", HttpContent client = null)
        {
            var result = WorkHttpRequestByMethodAsync(type, url, param, client);
        }
  
        private async Task WorkHttpRequestByMethodAsync(eHTTPTYPE type, string url, string param = "", HttpContent client = null)
        {
            try
            {
                switch (type)
                {
                    case eHTTPTYPE.GET:
                        await CWebManager.DoHttpGetAsync(url, param);
                        break;
                    case eHTTPTYPE.POST:
                        await CWebManager.DoHttpPostAsync(url, param, client);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.GCLogger.Error(nameof(CWebCotentManager), "BuildHttpQuery", ex);
            }
        }

    }
}

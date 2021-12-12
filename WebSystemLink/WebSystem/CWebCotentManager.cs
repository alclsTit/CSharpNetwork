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
        POST
    }

    public class CWebCotentManager 
    {
        public CWebCotentManager()
        {

        }

        public void LoadConfig(string fileName)
        {
            var ExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var filePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(ExePath, @"..\..\..\")) + $"Config\\{fileName}.ini";


            GameLib.IniConfig.IniFileRead("URL", "TEST", "", filePath);

        }

        public void BuildHttpQuery(eHTTPTYPE type, string url, string param, HttpContent client = null)
        {
            switch (type)
            {
                case eHTTPTYPE.GET:
                    CWebManager.DoHttpGetAsync(url, param);
                    break;
                case eHTTPTYPE.POST:
                    CWebManager.DoHttpPostAsync(url, param, client);
                    break;
                default:
                    break;
            }
        }

    }
}

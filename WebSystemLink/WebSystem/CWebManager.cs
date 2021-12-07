using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace LAB_ROOM4
{
    class CWebCancelToken
    {

    }

    class CWebManager
    {
        /// <summary>
        /// httpclient 를 클래스 종속 인스턴스로 만들면 부하발생 및 소켓 수 고갈
        /// 정적 멤버변수로 한번만 초기화하여 전역으로 사용
        /// </summary>
        private static readonly HttpClient httpClient;

        private bool mDisposeOnce = false;

        static CWebManager()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// Do HTTP GET METHOD (ASYNC)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<bool> DoHttpGetAsync(string url, string param)
        {
            var real_url = url + "?" + param;
            var result = await httpClient.GetAsync(real_url);
            if (result.IsSuccessStatusCode)
            {
                var response_message = result.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Do HTTP POST METHOD (ASYNC)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task<bool> DoHttpPostAsync(string url, string param, HttpContent client)
        {
            var real_url = url + param;
            var result = await httpClient.PostAsync(real_url, client);
            if (result.IsSuccessStatusCode)
            {
                var response_message = result.Content.ReadAsStringAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        

        public void Dispose(bool flag)
        {
            if (mDisposeOnce)
                return;

            if (flag)
            {

            }

            httpClient.Dispose();

            mDisposeOnce = true;
        }
    }
}

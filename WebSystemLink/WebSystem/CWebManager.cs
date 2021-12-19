using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace WebSystem
{
    class CWebCancelToken
    {

    }

    public static class CWebManager
    {
        /// <summary>
        /// httpclient 를 클래스 종속 인스턴스로 만들면 부하발생 및 소켓 수 고갈
        /// 정적 멤버변수로 한번만 초기화하여 전역으로 사용
        /// </summary>
        private static readonly HttpClient httpClient;

        private static bool mDisposeOnce = false;

        static CWebManager()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// Do HTTP GET METHOD (ASYNC)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task DoHttpGetAsync(string url, string param)
        {
            var real_url = url + "?" + param;
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await httpClient.GetAsync(real_url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in CWebManager.DoHttpGetAsync - {ex.Message}");           
            }

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                ResultSuccess(response);
            }      
        }

        /// <summary>
        /// Do HTTP POST METHOD (ASYNC)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static async Task DoHttpPostAsync(string url, string param, HttpContent client)
        {
            var real_url = url + param;
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                result = await httpClient.PostAsync(real_url, client);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception in CWebManager.DoHttpPostAsync - {ex.Message}");
            }

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                ResultSuccess(response);
            }
        }

        /// <summary>
        /// Http Method에 대한 웹 응답이 성공하였을 때 후처리 작업 진행 함수
        /// </summary>
        /// <param name="response">웹 서버 응답 메시지</param>
        public static void ResultSuccess(string response)
        {
            var responseArray = response.Split(',');
            foreach (var target in responseArray)
            {
                Console.WriteLine($"{target}");
            }
        }
        

        public static void Dispose(bool flag)
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

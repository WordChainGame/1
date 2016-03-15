using System;
using System.Net;


namespace WordChainGame
{
    class WebRequests
    {
        static HttpWebRequest CreateRequest(string uri, string method)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.ContinueTimeout = 10000;
            return request;
        }

        static HttpWebResponse CreateResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)(request.GetResponseAsync().Result);
            }
            catch { }
            return response;
        }

        static HttpWebResponse CreateGETRequest(string uri)
        {
            HttpWebRequest request = CreateRequest(uri, "Get");
            return CreateResponse(request);
        }

        static public HttpWebResponse WikiRequest(string arg) // Поиск информации о городе в Википедии
        {
            return CreateGETRequest(String.Format("https://ru.wikipedia.org/w/api.php?action=opensearch&search={0}&prop=info&format=json&inprop=url", arg));
        }

        static public HttpWebResponse PostVk(string message, string access_token, string user_id) // Пост записи в ВК
        {
            return CreateGETRequest(String.Format("https://api.vk.com/method/wall.post?owner_id={0}&message={1}&access_token={2}", user_id, message, access_token));
        }
    }
}

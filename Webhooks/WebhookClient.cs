using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace Webhooks
{
    public static class HttpClientExtensions
    {
        public static void Send<T>(this HttpClient httpClient,string url, T data)
        {
            try
            {
                httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(data)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
    public class WebhookClient
    {
        private HttpClient httpClient = new HttpClient();
        private string url;

        public WebhookClient(string url) { this.url = url; }

        public void Send(WebhookData data)
        {
            httpClient.Send(url, data);
        }
    }
}

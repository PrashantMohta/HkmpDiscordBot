using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Webhooks
{
    public static class HttpListenerContextExtensions
    {
        public static async void Respond(this HttpListenerContext ctx, string response, int status = 200)
        {
            var R = ctx.Response;
            try
            {
                Console.WriteLine($"response:{response}");
                byte[] data = Encoding.UTF8.GetBytes(response);
                R.ContentType = "application/json";
                R.ContentEncoding = Encoding.UTF8;
                R.ContentLength64 = data.LongLength;
                await R.OutputStream.WriteAsync(data, 0, data.Length);
                R.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"response error:{ex}");
                R.Close();
            }
        } 

        public static T GetBody<T>(this HttpListenerContext ctx)
        {
            var body = "";
            using (var inputStream = new StreamReader(ctx.Request.InputStream))
            {
                body = inputStream.ReadToEnd();
            }
            Console.WriteLine(body);
            return JsonConvert.DeserializeObject<T>(body);
        }
    }
    public class WebhookServer
    {

        internal HttpListener listener;
        internal string url;
        internal int requestCount = 0;
        internal Action<HttpListenerContext, WebhookData> Callback;
        public Action<HttpListenerContext, Exception> ExceptionHandler;

        public WebhookServer(string url, Action<HttpListenerContext,WebhookData> callback)
        {
            this.url = url;
            this.Callback = callback;
        }

        public async Task HandleIncomingConnections()
        {
            bool runServer = true;

            while (runServer)
            {

                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await this.listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                try
                {
                    Console.WriteLine(req.RemoteEndPoint);
                    if (req.HttpMethod != "POST" )
                    {
                        ctx.Respond("Not Allowed", 405);
                        continue;
                    }
                    WebhookData data = ctx.GetBody<WebhookData>();
                    if(data != null)
                    {
                        Callback(ctx, data);
                    }
                    else
                    {
                        ctx.Respond("Error",500);
                    }
                    
                }
                catch (Exception ex)
                {
                    ctx.Respond($"{ex}", 500);
                    if(ExceptionHandler != null) { 
                        ExceptionHandler(ctx, ex);
                    }
                }
            }
        }


        public  void Start()
        {
            // Create a Http server and start listening for incoming connections
            listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        }
    }
}


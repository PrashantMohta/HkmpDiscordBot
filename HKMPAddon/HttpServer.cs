using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HKMPDiscordBot
{
    internal class HttpServer
    {
        public static HttpListener listener;
        public static string url;
        public static int pageViews = 0;
        public static int requestCount = 0;

        public static async Task RespondWith(HttpListenerResponse resp, string response)
        {
            Console.WriteLine($"response:{response}");
            byte[] data = Encoding.UTF8.GetBytes(response);
            resp.ContentType = "text/html";
            resp.ContentEncoding = Encoding.UTF8;
            resp.ContentLength64 = data.LongLength;

            // Write out to the response stream (asynchronously), then close it
            await resp.OutputStream.WriteAsync(data, 0, data.Length);
            resp.Close();
        }
        public static async Task HandleIncomingConnections()
        {
            bool runServer = true;

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine($"Request #: {++requestCount}");
                var body = "";
                using (var inputStream = new StreamReader(req.InputStream))
                {
                    body = inputStream.ReadToEnd();
                }
                Console.WriteLine(body);
                WebhookData data = JsonConvert.DeserializeObject<WebhookData>(body);

                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                if (data.UserName != null)
                {
                    Server.Instance.Broadcast($"{data.UserName} from {data.CurrentScene}: {data.Message}");
                    await RespondWith(resp,"ok");  
                }

            }
        }


        public static void Start()
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


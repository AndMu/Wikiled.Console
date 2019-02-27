using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Utilities.Helpers;
using Wikiled.Console.HelperMethods;

namespace Wikiled.Console.Auth
{
    public class OAuthHelper
    {
        private readonly ILogger<OAuthHelper> logger;

        public OAuthHelper(ILogger<OAuthHelper> logger)
        {
            this.logger = logger;
            RedirectUri = $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
            logger.LogInformation("redirect URI: " + RedirectUri);
        }

        public string RedirectUri { get; }

        public string Code { get; private set; }

        public bool IsSuccessful { get; private set; }

        public async Task Start(string serviceUrl, string state = null)
        {
            IsSuccessful = false;
            var http = new HttpListener();
            http.Prefixes.Add(RedirectUri);
            logger.LogInformation("Listening...");
            http.Start();

            // Opens request in the browser.
            ExternaApp.OpenUrl(serviceUrl);

            // Waits for the OAuth authorization response.
            HttpListenerContext context = await http.GetContextAsync().ConfigureAwait(false);

            // Sends an HTTP response to the browser.
            HttpListenerResponse response = context.Response;
            var responseString = "<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length)
                                              .ContinueWith(
                                                  task =>
                                                  {
                                                      responseOutput.Close();
                                                      http.Stop();
                                                      logger.LogInformation("HTTP server stopped.");
                                                  });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                logger.LogInformation(($"OAuth authorization error: {context.Request.QueryString.Get("error")}"));
                return;
            }

            if (context.Request.QueryString.Get("code") == null || context.Request.QueryString.Get("state") == null)
            {
                logger.LogInformation("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");

            // Compares the received state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (state != null &&
                incomingState != state)
            {
                logger.LogInformation($"Received request with invalid state ({incomingState})");
                return;
            }

            logger.LogInformation("Authorization code: " + code);
            Code = code;
            IsSuccessful = true;
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private void BringConsoleToFront()
        {
            NativeMethods.SetForegroundWindow(NativeMethods.GetConsoleWindow());
        }
    }
}

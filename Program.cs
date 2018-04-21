using System;
using System.Net;
using System.Text;
using System.Threading;
using Leap;

namespace LeapServer
{
    class Program
    {
        /// <summary>
        /// Listen to HTTP Traffic
        /// </summary>
        static HttpListener _sHttpListener;

        static string _sPort = "80";

        static Program()
        {
            try
            {
                _sHttpListener = new HttpListener();
                _sHttpListener.Prefixes.Add(string.Format("http://*:{0}/", _sPort));
                _sHttpListener.Start();
                Console.WriteLine("Leap Server is listening.");
                Console.WriteLine("Ready to browse: http://localhost:{0}/ or http://localhost:{0}/refresh", _sPort);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to start listener: {0}", ex);
            }
        }

        static string GetRefreshContent()
        {
            return @"<html>
<head>
    <title>Leap Server - Refresh Page</title>
</head>
<body>
    <h1>Leap Server - Refresh Page</h1>
    <pre id='preLeap' style='border:2px solid Black; padding: 1em;'></pre>
    <script>
        setInterval(function() {
            var req = new XMLHttpRequest();
            var url = 'http://localhost:" + _sPort + @"';
            req.open('GET', url, true);
            req.onreadystatechange = function(evt) {
                if (req.readyState == 4 && req.status == 200)
                {
                    preLeap.innerHTML = this.responseText;
                }
            };
            req.send();
        }, 100);
    </script>
</body>
</html>";
        }

        static void Main(string[] args)
        {
            // Create a sample listener and controller
            SampleListener listener = new SampleListener();
            Controller controller = new Controller();

            Console.WriteLine("Leap is connected: {0}", controller.IsConnected);
            if (!controller.IsConnected)
            {
                Console.Error.Write("Be sure to install the Leap Orion beta software: https://developer.leapmotion.com/get-started");
                Console.Error.WriteLine();
            }

            controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            controller.FrameReady += listener.OnFrame;

            while (true)
            {
                HttpListenerContext context = null;
                try
                {
                    context = _sHttpListener.GetContext();

                    if (context.Request.Url.LocalPath.ToLower().StartsWith("/refresh"))
                    {
                        string page = GetRefreshContent();
                        context.Response.ContentLength64 = page.Length;
                        context.Response.ContentType = "text/html";
                        byte[] bytes = UTF8Encoding.UTF8.GetBytes(page);
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                        context.Response.OutputStream.Flush();
                    }
                    else
                    {
                        byte[] bytes = UTF8Encoding.UTF8.GetBytes(SampleListener._sJSON.ToString());
                        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                        context.Response.OutputStream.Flush();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HttpListener: exception={0}", ex);
                }
                finally
                {
                    try
                    {
                        if (null != context)
                        {
                            context.Response.Close();
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                Thread.Sleep(0);
            }
        }
    }
}

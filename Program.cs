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

        static Program()
        {
            try
            {
                _sHttpListener = new HttpListener();
                _sHttpListener.Prefixes.Add("http://*/");
                _sHttpListener.Start();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to start listener: {0}", ex);
            }
        }

        static void Main(string[] args)
        {
            // Create a sample listener and controller
            SampleListener listener = new SampleListener();
            Controller controller = new Controller();

            controller.SetPolicy(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            controller.FrameReady += listener.OnFrame;

            while (true)
            {
                HttpListenerContext context = null;
                try
                {
                    context = _sHttpListener.GetContext();

                    byte[] bytes = UTF8Encoding.UTF8.GetBytes(SampleListener._sJSON.ToString());
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    context.Response.OutputStream.Flush();
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

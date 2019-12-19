using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace ru.pflb.VirtualController
{
    class HTTPListener_VirtualController : ConnectionHandler
    {
    /*    public HTTPListener_VirtualController()
        {
            this.listener = new HttpListener();
        }*/
        public void CreateListener(int port, VirtualController VC)
        { //Virtual Controller Listener


                if (!HttpListener.IsSupported)
                {
                    Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                    return;
                }
                listener.Prefixes.Add("http://localhost:" + port + "/");
                listener.Start();
                Console.WriteLine(port + ":(VC) Ожидание подключений...");

                while (true)
                {
                    // метод GetContext блокирует текущий поток, ожидая получение запроса 
                    HttpListenerContext context = listener.GetContext();


                    //    ConnectionInfo(context.Request);
                    HttpListenerRequest request = context.Request;
                    NameValueCollection BodyCol = new NameValueCollection();
                    BodyCol = KeysAndValuesFromBody(request.InputStream);

                    switch (Path(context.Request.RawUrl))
                    {
                        default: SimpleTextResponse(context, "I'm Virtual Controller"); break;
                        case "/CreateRobots/":

                            SimpleTextResponse(context, "CreatingRobots " + BodyCol.Get("count"));
                            VC.CreateRobots(Convert.ToInt32(BodyCol.Get("count")));

                            break;
                        case "/StopRobots/":

                            SimpleTextResponse(context, "CreatingRobots " + BodyCol.Get("count"));
                            VC.StopRobots(Convert.ToInt32(BodyCol.Get("count")));

                            break;
                        case "/Values/": SimpleTextResponse(context, VC.PrintValues()); break;
                    }
                    Thread.Sleep(50);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.WaitForFullGCComplete();
                    GC.Collect();
                }
        }

    }
}

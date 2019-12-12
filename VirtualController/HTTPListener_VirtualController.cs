using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace ru.pflb.VirtualController
{
    class HTTPListener_VirtualController : HTTPListener
    {
        public void CreateListener(int port, VirtualController VC)
        { //Virtual Controller Listener

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            HttpListener listener = new HttpListener();
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
                    default: SimpleResponse(context, "I'm Virtual Controller"); break;
                    case "/CreateRobots/":
                        VC.CreateRobots(Convert.ToInt32(BodyCol.Get("count")));
                        SimpleResponse(context, "CreatingRobots " + BodyCol.Get("count"));
                        break;
                    case "/Values/": SimpleResponse(context, VC.GetValues()); break;
                }
                Thread.Sleep(0);
            }
        }

    }
}

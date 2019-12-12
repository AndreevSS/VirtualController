using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace ru.pflb.VirtualController
    {
    class HTTPListener_Robots : HTTPListener
    {
        Random rnd = new Random();

        public void CreateListener(int port, VirtualRobot VR, ConcurrentQueue<string> VCQueue)
        {  //Virtual Robot Listener

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();
            Console.WriteLine(port + ":(VR) Ожидание подключений...");

            while (true)
            {
                // метод GetContext блокирует текущий поток, ожидая получение запроса 
                HttpListenerContext context = listener.GetContext();
                //ConnectionInfo(context.Request);
                HttpListenerRequest request = context.Request;

                NameValueCollection BodyCol = new NameValueCollection();
                BodyCol = KeysAndValuesFromBody(request.InputStream);

                switch (Path(context.Request.RawUrl))
                {
                    default: SimpleResponse(context, "Nothing is found!"); break;
                    case "/Token/":
                        CreateToken(VR, context);
                        break;
                    case "/CreateSession/":
                        CreateSession(VR, context, Convert.ToInt32(BodyCol.Get("time")), Convert.ToInt32(BodyCol.Get("duration")), VCQueue);
                        break;
                    case "/StartSession/":
                        StartSession(VR, context, VCQueue);
                        break;
                    case "/Values/": SimpleResponse(context, VR.GetValues()); break;
                }
                Thread.Sleep(0);
            }
        }

        void CreateToken(VirtualRobot VR, HttpListenerContext context)
        { VR.token = Convert.ToString(Guid.NewGuid());
            SimpleResponse(context, "Token: " + VR.token);
        }
        void CreateSession(VirtualRobot VR, HttpListenerContext context, int time, int duration, ConcurrentQueue<string> VCQueue)
        {
            Thread.Sleep(rnd.Next(500));
            VR.CreateSession(VR.id, time, duration, "created");
            SimpleResponse(context, "Session Created\n id = " + VR.VS.id +
                                    " time = " + VR.VS.time +
                                    " duration = " + VR.VS.duration);
            String RequestString = "insert into Sessions(Status, ID) " +
                                   "values('" + VR.VS.status + "' , " + VR.VS.id + ");";
            VCQueue.Enqueue(RequestString);
        }
        void StartSession(VirtualRobot VR, HttpListenerContext context, ConcurrentQueue<string> VCQueue)
        {
            if (!(VR.VS is null) && VR.VS.status != "working")
             {
             Thread.Sleep(rnd.Next(500));
             SimpleResponse(context, "Session " + VR.VS.id + " started");
             VR.VS.Start(VCQueue);
             }
            else
             {
              SimpleResponse(context, "Session not started");
             }
        }

    }
    }


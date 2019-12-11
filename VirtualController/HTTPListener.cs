using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace ru.pflb.VirtualController
{
    class HTTPListener
    {
        Random rnd = new Random();

        public static NameValueCollection KeysAndValuesFromBody(Stream stream)
        {

            NameValueCollection BodyCol = new NameValueCollection();
            string BodyString = ToString(stream);
            BodyCol = HttpUtility.ParseQueryString(BodyString);
            return BodyCol;
        }

        static void SimpleResponse(HttpListenerContext context, string Answer)
        {
            HttpListenerResponse response = context.Response;

            string ResponseString = Answer;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ResponseString);
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static Stream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string ToString(Stream stream)
        {

            StreamReader reader = new StreamReader(stream);
            string String = reader.ReadToEnd();
            return String;
        }

        public static string Path(string url)
        {
            if (url.IndexOf('?') > 0)
                url = url.Substring(0, url.IndexOf('?'));
            return url;
        }
        public static void ConnectionInfo(HttpListenerRequest request)
        {
              Console.WriteLine("URL: {0}", request.Url.OriginalString);
              Console.WriteLine("Raw URL: {0}", request.RawUrl);
              Console.WriteLine("Path: {0}", Path(request.RawUrl));
              Console.WriteLine("method: {0}", request.HttpMethod);
              Console.WriteLine("{0} request was caught: {1}",
              request.HttpMethod, request.Url);
              Console.WriteLine("Query: {0}", request.QueryString);
        }

        public static void CreateListener(int port, VirtualController VC)
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
                        SimpleResponse(context, "CreatingRobots");
                        break;
                    case "/Values/": SimpleResponse(context, VC.GetValues()); break;
                }
                Thread.Sleep(0);
            }
        }

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
                        VR.token = Convert.ToString(Guid.NewGuid());
                        SimpleResponse(context, "Token: " + VR.token);
                        break;
                    case "/CreateSession/":
                        //     if ( /*(VR.token == BodyCol.Get("token")) && */BodyCol.Get("time") != null && BodyCol.Get("duration") != null)
                        //      {                            
                        Thread.Sleep(rnd.Next(500));
                        int time = Convert.ToInt32(BodyCol.Get("time"));
                        int duration = Convert.ToInt32(BodyCol.Get("duration"));
                        VR.CreateSession(VR.id, time, duration, "created");
                        SimpleResponse(context, "Session Created\n id = " + VR.VS.id + " time = " + VR.VS.time + " duration = " + VR.VS.duration);
                        String RequestString = "insert into Sessions(Status, ID) " +
                                               "values('" + VR.VS.status + "' , " + VR.VS.id + ");";
                        VCQueue.Enqueue(RequestString);
                        //       }
                        //         else
                        //     {
                        //           SimpleResponse(context, "Wrong Request");
                        //   };
                        break;
                    case "/StartSession/":
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

                        break;
                    case "/Values/": SimpleResponse(context, VR.GetValues()); break;
                }

                Thread.Sleep(0);

            }
        }


    }
}

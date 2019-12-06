using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;

namespace VirtualController
{
    class HTTPListener
    {

        public static NameValueCollection KeysAndValuesFromBody(Stream stream)
        {

            NameValueCollection BodyCol = new NameValueCollection();
            String BodyString = ToString(stream);
            BodyCol = HttpUtility.ParseQueryString(BodyString);
            return BodyCol;
        }

        static void SimpleResponse(HttpListenerContext context, String Answer)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            String ResponseString = "I'm response!\nAnswer is: " + Answer;

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

        public static String ToString(Stream stream)
        {

     //       stream.Position = 0;
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
        {

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }


            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();


  
            //      HttpListenerResponse response = context.Response;
            Console.WriteLine(port + ":(VC) Ожидание подключений...");

            while (true)
                {
                    // метод GetContext блокирует текущий поток, ожидая получение запроса 
                    HttpListenerContext context = listener.GetContext();
                    ConnectionInfo(context.Request);
                    HttpListenerRequest request = context.Request;

                    NameValueCollection BodyCol = new NameValueCollection();
                    BodyCol = KeysAndValuesFromBody(request.InputStream);
               
                switch (Path(context.Request.RawUrl))
                    {
                        default: SimpleResponse(context, "I'm Virtual Controller"); break;
                    case "/CreateRobots/":
                        if (Convert.ToInt32(BodyCol.Get("count")) > 0)
                        {
                             VC.CreateRobots(Convert.ToInt32(BodyCol.Get("count"))); SimpleResponse(context, "CreatingRobots"); break;
                        }
                        else
                            SimpleResponse(context, "Wrong Value");
                        break;

                    case "/Values/": SimpleResponse(context, VC.GetValues()); break;
                    }



                Thread.Sleep(0);

                }
            }
            public static void CreateListener(int port, VirtualRobot VR)
        {

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port +"/");
            listener.Start();

            Console.WriteLine(port + ":(VR) Ожидание подключений...");



                while (true)
                {
                    // метод GetContext блокирует текущий поток, ожидая получение запроса 
                    HttpListenerContext context = listener.GetContext();
                    ConnectionInfo(context.Request);
                    HttpListenerRequest request = context.Request;

                    NameValueCollection BodyCol = new NameValueCollection();
                    BodyCol = KeysAndValuesFromBody(request.InputStream);


                    switch (Path(context.Request.RawUrl))
                        {
                            default: SimpleResponse(context, "Nothing is found!"); break;
                            case "/Token/":
                                if (BodyCol.Get("token") != null)
                                {
                                   VR.token = BodyCol.Get("token");
                                   SimpleResponse(context, "Token is added: " + BodyCol.Get("token"));
                                }
                                else
                                {
                                   SimpleResponse(context, "Token is not added");
                                };
                                break;
                            case "/CreateSession/":
                                if ( (VR.token == BodyCol.Get("token")) && (BodyCol.Get("time") != null) && (BodyCol.Get("duration") != null) ) 
                                {

                                  int time = Convert.ToInt32(BodyCol.Get("time"));
                                  int duration = Convert.ToInt32(BodyCol.Get("time"));
                                  VR.CreateSession(VR.id, time , duration, "created");

                                  SimpleResponse(context, "Session Created\n id = " + VR.VS.id + " time = " + VR.VS.time + " duration = " + VR.VS.duration);
                                }
                                else
                                {
                                  SimpleResponse(context, "Wrong Request");
                                };
                                break;
                            case "/StartSession/": 
                                if (!(VR.VS is null) && (VR.VS.status != "working"))
                                    {
                                    SimpleResponse(context, "Session " + VR.VS.id + "started");
                                    VR.VS.Start();                              
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

using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading;


namespace VirtualController
{
    class HTTPListener
    {

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

            stream.Position = 0;
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


            //      HttpListenerRequest request = context.Request;
            //      HttpListenerResponse response = context.Response;
            Console.WriteLine(port + ":(VC) Ожидание подключений...");

            while (true)
                {
                    // метод GetContext блокирует текущий поток, ожидая получение запроса 
                    HttpListenerContext context = listener.GetContext();
                    ConnectionInfo(context.Request);

                    switch (Path(context.Request.RawUrl))
                    {
                        default: SimpleResponse(context, "I'm Virtual Controller"); break;
                        case "/CreateRobots/": VC.CreateRobots(1); SimpleResponse(context, "CreatingRobots"); break;
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


            //      HttpListenerRequest request = context.Request;
            //      HttpListenerResponse response = context.Response;
            Console.WriteLine(port + ":(VR) Ожидание подключений...");

                while (true)
                {
                    // метод GetContext блокирует текущий поток, ожидая получение запроса 
                    HttpListenerContext context = listener.GetContext();
                    ConnectionInfo(context.Request);

                    switch (Path(context.Request.RawUrl))
                    {
                        default: SimpleResponse(context, "Nothing is found!"); break;
                        case "/Token/": SimpleResponse(context, "Token"); break;
                        case "/CreateSession/": SimpleResponse(context, "CreateSession"); break;
                        case "/StartSession/": SimpleResponse(context, "StartSession"); break;
                    case "/Values/": SimpleResponse(context, VR.GetValues()); break;
                }

                    Thread.Sleep(0);

                }
            }

        
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Text.RegularExpressions;

namespace ru.pflb.VirtualController
{
    class HTTPListener_Robots : HTTPListener
    {


        public HttpListener listener = new HttpListener();
        Random rnd = new Random();
        string userid = "";
        string processid = "";
        public void CreateListener(int port, VirtualRobot VR, ConcurrentQueue<string> VCQueue, bool isStopped)
        {  //Virtual Robot Listener

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            
            
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();
            Console.WriteLine(port + ":(VR) Ожидание подключений...");


            while (!VR.isStopped)
            {

                // метод GetContext блокирует текущий поток, ожидая получение запроса 
                HttpListenerContext context = listener.GetContext();
                //ConnectionInfo(context.Request);
                HttpListenerRequest request = context.Request;
                NameValueCollection BodyCol = new NameValueCollection();
                BodyCol = KeysAndValuesFromBody(request.InputStream);
                GroupCollection pathGroup = Regex.Match(context.Request.RawUrl, "(.+?)(%20.*)").Groups;
                GroupCollection groups;

                switch (pathGroup[1].Value)
                {
                    default: SimpleTextResponse(context, "400"); break;
                    case "/getauthtoken":
                        //CreateToken(VR, context);
                        groups = Regex.Match(pathGroup[2].Value, "%20(.*?)%20(.*?)%20(.*)").Groups;
                        processid = groups[1].Value;
                        userid = groups[2].Value;
                        //password = groups[3].Value;
                        CreateToken(VR, context, userid, VCQueue);
                        break;
                    case "/user":

                        if (Regex.IsMatch(pathGroup[2].Value, "createas"))
                        {
                            
                            groups = Regex.Match(pathGroup[2].Value, "%20(.*?)&password%20(.*?)&busy&createas%20(.*)%20(.*)").Groups;

                            CreateSession(VR, context, Convert.ToInt32(BodyCol.Get("time")), Convert.ToInt32(BodyCol.Get("duration")), VCQueue, userid);
                        }

                        if (Regex.IsMatch(pathGroup[2].Value, "startas"))
                        {
                           
                            groups = Regex.Match(pathGroup[2].Value, "%20(.*?)&password%20(.*?)&busy&startas%20(.*)%20(.*)").Groups;
                            StartSession(VR, context, VCQueue);
                        }
                        break;
                    case "/Values/": SimpleTextResponse(context, VR.PrintValues()); break;
                }
                Thread.Sleep(0);     
            }
        }

        void CreateToken(VirtualRobot VR, HttpListenerContext context, String UserID, ConcurrentQueue<string> VCQueue)
        {
            VR.token = Convert.ToString(Guid.NewGuid());

            String letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            String Result = "";
            Random Random = new Random();

            for (int i = 0; i < 73; i++)
            {
                int RND = Random.Next(letters.Length);
                Result = Result + letters[RND];
            }

            VR.token = Result;          
            VCQueue.Enqueue(DBQueries.CreateToken(userid, VR.token));
            SimpleTextResponse(context, UserID + "" + VR.token);
        }
        void CreateSession(VirtualRobot VR, HttpListenerContext context, int time, int duration, ConcurrentQueue<string> VCQueue, string userid)
        {
            String sessionid = Guid.NewGuid().ToString();
            Thread.Sleep(rnd.Next(500));
            VR.CreateSession(sessionid, time, duration, "0 (Pending)");           
            VCQueue.Enqueue(DBQueries.CreateSession(sessionid,processid,userid, VR.id));

            SimpleTextResponse(context,
                                    "USER SET\r\n" +
                                    "USER AUTHENTICATED\r\n" +
                                    "no\r\n" +
                                    "SESSION CREATED : " + sessionid);

        }
        void StartSession(VirtualRobot VR, HttpListenerContext context, ConcurrentQueue<string> VCQueue)
        {
            if (!(VR.VS is null) && VR.VS.status != "2 (Running)")
            {

                Thread.Sleep(rnd.Next(500));
 
                VR.VS.Start(VCQueue);

                SimpleTextResponse(context,
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "...\r\n" +
                     "STARTED");
            }
            else
            {

                SimpleTextResponse(context,
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "...\r\n" +
                     "NOT STARTED");
                
            }
        }

    }
}


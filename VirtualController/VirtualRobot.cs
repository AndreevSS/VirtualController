using CSharpTest.Net.Http;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class VirtualRobot
    {
        public ConcurrentQueue<string> VCQueue;
        public int port;
        public string id;
        public string token;
        public VirtualSession VS;
        public HttpServer server;

        public HttpListener listener = new HttpListener();
        Random rnd = new Random();
        string userid = "";
        string processid = "";

        public VirtualRobot(int port, string id, ConcurrentQueue<string> VCQueue, HttpServer server)
        {
            this.port = port;
            this.id = id;
            this.token = null;
            this.VCQueue = VCQueue;
            this.server = server;
          
            Console.WriteLine("VirtualRobot created on " + port);
        }


        public void RobotRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            // Construct a response.
            NameValueCollection BodyCol = new NameValueCollection();
            BodyCol = ConnectionHandler.KeysAndValuesFromBody(request.InputStream);
            String Path = context.Request.RawUrl;

            String[] SplitPath = Path.Split('&');

            string pattern = "\\b(.+?)(?(?=(%20.*))(.*)|$)";

            string responseText = "";

            if (request.LocalEndPoint.Port == port)
            {
                foreach (String command in SplitPath)
                {
                    bool isValidCommand = true;
                    GroupCollection pathGroup = Regex.Match(command, pattern).Groups;
                    GroupCollection groups;
                    switch (pathGroup[1].Value)
                    {
                        default: isValidCommand = false; break;
                        case "user":
                            break;
                        case "password":
                            break;
                        case "busy":
                            break;
                        case "getauthtoken":
                            groups = Regex.Match(pathGroup[2].Value, "%20(.*?)%20(.*?)%20(.*)").Groups;
                            this.processid = groups[1].Value;
                            userid = groups[2].Value;
                            responseText += CreateToken(userid, VCQueue);
                            break;
                        case "createas":
                            responseText += CreateSession(Convert.ToInt32(BodyCol.Get("time")), Convert.ToInt32(BodyCol.Get("duration")), VCQueue, userid);
                            //    isValidCommand = true;
                            break;
                        case "startas":
                            responseText += StartSession(VCQueue);
                            //    isValidCommand = true;
                            break;
                            //     case "/Values/": SimpleTextResponse(context, VR.PrintValues()); break;
                    }
                    if (!(isValidCommand))
                    {
                        responseText = "400\r\ncommand '" + pathGroup[1] + "' not found";
                        context.Response.StatusCode = 400;
                        break;
                    }

                }
                ConnectionHandler.SimpleTextResponse(context, responseText);
                Thread.Sleep(10);
            }
        }



        public string PrintValues()
        {
            Console.WriteLine("Robot ID = " + id);
            Console.WriteLine("port:  " + port);
            Console.WriteLine("token = " + token);

            return "Robot ID = " + id + "\nport: " + port + "\ntoken = " + token + "";
        }

        public void CreateSession(string id, int time, int duration, int status)
        {
            if ((time != 0) && (duration != 0))
                VS = new VirtualSession(id, time, duration, status);
        }

        string CreateToken(String UserID, ConcurrentQueue<string> VCQueue)
        {
            token = Convert.ToString(Guid.NewGuid());

            String letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            String Result = "";
            Random Random = new Random();

            for (int i = 0; i < 73; i++)
            {
                int RND = Random.Next(letters.Length);
                Result = Result + letters[RND];
            }

            token = Result;

            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            
            VCQueue.Enqueue(DBQueries.CreateToken(userid, token));
            Console.WriteLine("VCQueue.Enqueue: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start));


            return UserID + "" + token;
            //   SimpleTextResponse(context, UserID + "" + VR.token);
        }
        string CreateSession(int time, int duration, ConcurrentQueue<string> VCQueue, string userid)
        {
            String sessionid = Guid.NewGuid().ToString();
           // Thread.Sleep(rnd.Next(50));
            VS = new VirtualSession (sessionid, time, duration, 0);
            VCQueue.Enqueue(DBQueries.CreateSession(sessionid, processid, userid, id));

            return
                   "USER SET\r\n" +
                   "USER AUTHENTICATED\r\n" +
                   "no\r\n" +
                   "SESSION CREATED : " + sessionid;

        }
        string StartSession(ConcurrentQueue<string> VCQueue)
        {
            if (!(VS is null) && VS.status != 2)
            {

           //     Thread.Sleep(rnd.Next(50));

                VS.Start(VCQueue);

                return
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "...\r\n" +
                     "STARTED";
            }
            else
            {

                return
                     "USER SET\r\n" +
                     "USER AUTHENTICATED\r\n" +
                     "...\r\n" +
                     "NOT STARTED";

            }
        }

    }


}



















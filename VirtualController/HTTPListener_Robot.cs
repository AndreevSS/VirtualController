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
    class HTTPListener_Robot : ConnectionHandler
    {
        public HttpListener listener = new HttpListener();
        Random rnd = new Random();
        string userid = "";
        string processid = "";

        int port;
        VirtualRobot VR;
        ConcurrentQueue<string> VCQueue;
        bool isStopped;

        public void CreateListener(int port, VirtualRobot VR, ConcurrentQueue<string> VCQueue, bool isStopped)
        {  //Virtual Robot Listener


            this.port = port;
            this.VR = VR;
            this.VCQueue = VCQueue;
            this.isStopped = isStopped;
            //  try
            //   {

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }





            // while (!isStopped)
            // {

            Console.WriteLine("Thread " + Thread.CurrentThread.Name + " port:" + port + ":(VR) Ожидание подключений...");
            //{
            //  HttpListenerContext context = null;
            // метод GetContext блокирует текущий поток, ожидая получение запроса
            //     if ((listener.IsListening) && VR.thread.IsAlive)
            //     {
            //try
            //{
            //     IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            //


            if (!isStopped)
            {
                listener.Prefixes.Add("http://localhost:" + port + "/");
                listener.Start();
            }

            while (!isStopped)
            {
                try
                {
                    IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                    result.AsyncWaitHandle.WaitOne();
                    //                
                }
                catch (ObjectDisposedException e)
                {
                    return;
                //    Console.WriteLine(e);
                };
                //  listener.Stop();
            };



        }
        public void ListenerCallback(IAsyncResult result)
        {

            //  while (!VR.isStopped)
            //  {

            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            try
            {
                HttpListenerContext context = listener.EndGetContext(result);


                //ConnectionInfo(context.Request);
                HttpListenerRequest request = context.Request;
                NameValueCollection BodyCol = new NameValueCollection();
                BodyCol = KeysAndValuesFromBody(request.InputStream);
                String Path = context.Request.RawUrl;

                String[] SplitPath = Path.Split('&');

                string pattern = "\\b(.+?)(?(?=(%20.*))(.*)|$)";

                string responseText = "";

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
                            //CreateToken(VR, context);
                            groups = Regex.Match(pathGroup[2].Value, "%20(.*?)%20(.*?)%20(.*)").Groups;
                            processid = groups[1].Value;
                            userid = groups[2].Value;
                            //password = groups[3].Value;
                            responseText += CreateToken(VR, context, userid, VCQueue);
                            //   isValidCommand = true;
                            break;
                        case "createas":
                            responseText += CreateSession(VR, context, Convert.ToInt32(BodyCol.Get("time")), Convert.ToInt32(BodyCol.Get("duration")), VCQueue, userid);
                            //    isValidCommand = true;
                            break;
                        case "startas":
                            responseText += StartSession(VR, context, VCQueue);
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
                SimpleTextResponse(context, responseText);
                Thread.Sleep(50);
            }
            catch (Exception e)
            {
                //      listener.Close();
                //Console.WriteLine(e);
                return;
            }
            finally
            {
                if (isStopped)
                    listener.Close();
            };
        }


        //   catch (Exception e)
        //    {
        //        Console.WriteLine(e);

        //    };

        //////
        //Thread.CurrentThread.Abort();
    

    string CreateToken(VirtualRobot VR, HttpListenerContext context, String UserID, ConcurrentQueue<string> VCQueue)
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

        return UserID + "" + VR.token;
        //   SimpleTextResponse(context, UserID + "" + VR.token);
    }
    string CreateSession(VirtualRobot VR, HttpListenerContext context, int time, int duration, ConcurrentQueue<string> VCQueue, string userid)
    {
        String sessionid = Guid.NewGuid().ToString();
        Thread.Sleep(rnd.Next(50));
        VR.CreateSession(sessionid, time, duration, 0);
        VCQueue.Enqueue(DBQueries.CreateSession(sessionid, processid, userid, VR.id));

        return
                                "USER SET\r\n" +
                                "USER AUTHENTICATED\r\n" +
                                "no\r\n" +
                                "SESSION CREATED : " + sessionid;

    }
    string StartSession(VirtualRobot VR, HttpListenerContext context, ConcurrentQueue<string> VCQueue)
    {
        if (!(VR.VS is null) && VR.VS.status != 2)
        {

            Thread.Sleep(rnd.Next(50));

            VR.VS.Start(VCQueue);

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
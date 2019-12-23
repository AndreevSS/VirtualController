using CSharpTest.Net.Http;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ru.pflb.VirtualController
{
    public class VirtualController
    {
        
        public List<int> RobotPorts;
        public List<DBSender> DBSenders;
        public Thread mainThread;

        string[] DBData = new string[4];
        
        Dictionary<int, VirtualRobot> Robots = new Dictionary<int, VirtualRobot>();

        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();
        HttpServer server;

        int VRCount;
        int VCport;
        
        public VirtualController(int port, List<int> RobotPorts, string[] DBData, List<DBSender> DBSenders)
        {
            this.VCport = port;
            this.RobotPorts = RobotPorts;
            this.DBSenders = DBSenders;
            this.mainThread = Thread.CurrentThread;
            this.DBData = DBData;

            StartServer();

            Console.WriteLine("VirtualController created on " + port);

            Console.WriteLine(RobotPorts.Contains(VCport));

        }


        public void Server_ProcessRequest(object sender, HttpContextEventArgs e)
        {
            // Note: The GetContext method blocks while waiting for a request.
            HttpListenerContext context = e.Context;
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.

            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (context.Request.LocalEndPoint.Port == VCport)
            {
                ControllerRequest(context);
            };


            if (Robots.ContainsKey(context.Request.LocalEndPoint.Port))
            {
                VirtualRobot VR = Robots[context.Request.LocalEndPoint.Port];
                VR.RobotRequest(context);
            }

            Console.WriteLine(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start);

        }

        public void ControllerRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            NameValueCollection BodyCol = new NameValueCollection();
            BodyCol = ConnectionHandler.KeysAndValuesFromBody(request.InputStream);

            switch (context.Request.RawUrl.ToLower())
            {
                default: ConnectionHandler.SimpleTextResponse(context, "I'm Virtual Controller"); break;
                case "/createrobots/":
                    ConnectionHandler.SimpleTextResponse(context, "CreatingRobots " + BodyCol.Get("count"));
                    CreateRobots(Convert.ToInt32(BodyCol.Get("count")));
                   break;
                case "/reset/":
                    ConnectionHandler.SimpleTextResponse(context, "reseting");
                    new Thread(Reset).Start();
                    break;
                case "/createdbsender/":
                    ConnectionHandler.SimpleTextResponse(context, "Creating DB Senders" + BodyCol.Get("count"));
                    CreateDBSender(Convert.ToInt32(BodyCol.Get("count")));
                    break;
                //case "/values/": ConnectionHandler.SimpleTextResponse(context, PrintValues()); break;
            }
        }
        public void CreateRobots(int VRCount)
        {

            for (int i = 0; i < VRCount; i++)
            {
                int port = getFreePort();
                VirtualRobot VR = new VirtualRobot(port, Convert.ToString(port), VCQueue, server);
                Robots.Add(port, VR);
            }

            this.VRCount = this.VRCount + VRCount;
        }

        public int getFreePort()
        {

            List<int> occupiedPorts = Robots.Select(k => k.Key).ToList();
            var freePorts = RobotPorts.Except(occupiedPorts);
            return freePorts.Min();

        }

        public void StartServer()
        {
            server = new HttpServer(100);
            List<string> prefixes = new List<string>();

            prefixes = RobotPorts.ConvertAll<string>(x => "http://*:" + x.ToString() + "/");
            prefixes.Add("http://*:" + Convert.ToString(VCport) + "/");
            server.Start(prefixes.ToArray());
            server.ProcessRequest += Server_ProcessRequest;

        }

        public void ClearDBSenders()
        {
            foreach (DBSender DBSender in DBSenders)
            {
                DBSender.isStopped = true;
            }
            DBSenders.Clear();
        }

        public void Reset()
        {
            Thread.Sleep(500);
            server.Stop();
            Console.WriteLine("server is down");

            Robots.Clear();
            Console.WriteLine("robots are cleared");

            ClearDBSenders();
            Console.WriteLine("DBSenders are cleared");

            Thread.Sleep(5000);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
            StartServer();
            Console.WriteLine("server is up");
            Thread.CurrentThread.Join();

        }
        public void CreateDBSender(int DBProcessorCount)
        {
            for (int i = 0; i < DBProcessorCount; i++)
            {
                DBSender DBSender = new DBSender(DBData);
                DBSenders.Add(DBSender);

                Thread th = new Thread(() =>
                {
                    DBSender.StartSender(VCQueue);
                    
                });
                th.Name = "DBSender_" + (DBSenders.Count);
                Console.WriteLine(th.Name + " created");
                th.Start();    
            }
        }

        public string PrintValues()
        {
            Console.WriteLine("Virtual Controller port = " + VCport);
            Console.WriteLine("Robots count = " + Robots.Count);
            return "Virtual Controller port = " + VCport + "\nRobots count = " + VRCount + "";
        }

    }
}

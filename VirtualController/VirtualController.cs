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
    public class DBQueueObject
    {

        public VirtualRobot VR;
        public String Query;
        public bool needupdate;
        public DBQueueObject(VirtualRobot VR)
        {
            this.VR = VR;
            this.needupdate = false;
            this.Query = "";

        }
    }
    public class VirtualController
    {
        InfluxSender influxSender;
        public List<int> RobotPorts;
        public List<DBSender> DBSenders;


        string[] DBData = new string[4];
        string[] InfluxData = new string[4];
        int HTTP_Threads_Count;
        int UseBPAResourceUpdater;
        Dictionary<int, VirtualRobot> Robots = new Dictionary<int, VirtualRobot>();

        public ArrayList TT = new ArrayList();




        ConcurrentQueue<DBQueueObject> VCQueue = new ConcurrentQueue<DBQueueObject>();

        ConcurrentQueue<DBQueueObject> InfluxQueue = new ConcurrentQueue<DBQueueObject>();

        HttpServer server;

        int VRCount;
        int VCport;
        bool isCreatingRobots;

        string BPAPrefix;

        public VirtualController(int VCport, List<int> RobotPorts, string[] DBData, List<DBSender> DBSenders, 
            int HTTP_Threads_Count, int UseBPAResourceUpdater, string[] InfluxData, string BPAPrefix)
        {
            this.VCport = VCport;
            this.RobotPorts = RobotPorts;
            this.DBSenders = DBSenders;
            this.DBData = DBData;
            this.InfluxData = InfluxData;
            this.isCreatingRobots = false;
            this.HTTP_Threads_Count = HTTP_Threads_Count;
            this.UseBPAResourceUpdater = UseBPAResourceUpdater;
            this.BPAPrefix = BPAPrefix;
            StartServer();

            DBQueueObject dBQueueObject = new DBQueueObject(null);
            dBQueueObject.Query = DBQueries.RebuildSessions();
            VCQueue.Enqueue(dBQueueObject);

            CreateInfluxDBSender(1);

            

            DBQueueObject InfluxQueueObject = new DBQueueObject(null);
            InfluxQueueObject.Query = DBQueries.InsertIntoInflux("200", "1090");

            InfluxQueue.Enqueue(InfluxQueueObject);

            //();

            

            new Thread(BPAResourceUpdater).Start();

            Console.WriteLine("VirtualController created on " + VCport);
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

            //        Console.WriteLine(context.Request.Url);
            // Console.WriteLine(context.Request.RawUrl);
            //        Console.WriteLine(context.Request.QueryString.ToString());

            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (context.Request.LocalEndPoint.Port == VCport)
            {
                ControllerRequest(context);
            };


            if (Robots.ContainsKey(context.Request.LocalEndPoint.Port))
            {

                long finish = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start);
                Console.WriteLine("checkporttime: " + finish.ToString());
                VirtualRobot VR = Robots[context.Request.LocalEndPoint.Port];
                VR.RobotRequest(context);
            }


            

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
            isCreatingRobots = true;

            for (int i = 0; i < VRCount; i++)
            {
                int port = getFreePort();
                VirtualRobot VR = new VirtualRobot(port, VCQueue, server, BPAPrefix, influxSender);
                Robots.Add(port, VR);
            }

            this.VRCount = this.VRCount + VRCount;

            isCreatingRobots = false;
        }

        public int getFreePort()
        {

            List<int> occupiedPorts = Robots.Select(k => k.Key).ToList();
            var freePorts = RobotPorts.Except(occupiedPorts);
            return freePorts.Min();

        }

        public void StartServer()
        {
            server = new HttpServer(HTTP_Threads_Count);
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
                DBSender DBSender = new DBSender(DBData, influxSender);
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

        public void CreateInfluxDBSender(int DBProcessorCount)
        {
            Console.WriteLine("creating if");
            //  for (int i = 0; i < DBProcessorCount; i++)
            //   {
            //InfluxSender influxSender= new InfluxSender();
            //DBSenders.Add(DBSender);
            influxSender = new InfluxSender(InfluxData[0], InfluxData[1]);


            Thread th = new Thread(() =>
                {
                    while (true)
                    {
                        influxSender.SendPing();
                        Thread.Sleep(10000);
                    }
//                    DBSender.StartSender(InfluxQueue);
                });
                th.Name = "Inlfux_Sender" + (DBSenders.Count);
                Console.WriteLine(th.Name + " created");
                th.Start();
         //   }
        }
        public void BPAResourceUpdater()
        {
            while (UseBPAResourceUpdater == 1)
            {
                try
                { 
                    DBQueueObject DBQueueObject = new DBQueueObject(null);
                    DBQueueObject.Query = DBQueries.UpdateBPAResourcesLastUpdated();
                    VCQueue.Enqueue(DBQueueObject);
                }
                catch (Exception e)
                { Console.WriteLine(e); }
                Thread.Sleep(20000);
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

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
        ArrayList ThreadListDBProcessors = new ArrayList();
        public List<int> RobotPorts;
        public Thread mainThread;
        //  ArrayList Robots = new ArrayList();

        Dictionary<int, VirtualRobot> Robots = new Dictionary<int, VirtualRobot>();

        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();
        HttpServer server;

        int VRCount;
        int VCport;
        int DBProcessorCount;

        public VirtualController(int port, List<int> RobotPorts)
        {
            this.VCport = port;
            this.RobotPorts = RobotPorts;
            this.mainThread = Thread.CurrentThread;

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
                  //Thread.CurrentThread.Join();
                    new Thread(Reset).Start();
                    //th.Start();                    
                  // Reset();
                    break;
                case "/stoprobots/":
                    ConnectionHandler.SimpleTextResponse(context, "StoppingRobots " + BodyCol.Get("count"));
                    StopRobots(Convert.ToInt32(BodyCol.Get("count")));
                    break;
                case "/values/": ConnectionHandler.SimpleTextResponse(context, PrintValues()); break;
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

        public void Reset()
        {
            Thread.Sleep(500);
            server.Stop();
            Robots.Clear();

            Console.WriteLine("server is down");

            Thread.Sleep(5000);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();

            StartServer();

            Console.WriteLine("server is up");

            Thread.CurrentThread.Join();

            

        }
        public void StopRobots(int VRCount)

        {

            for (int i = 0; i < VRCount; i++)
            {
                if (Robots.Count > 0)
                {
                    VirtualRobot VR = Robots.First().Value;
                    Robots.Remove(VR.port);
                    Console.WriteLine("Robot " + VR.port + " stopped " + Robots.Count + " left");
                }
                else
                {
                    Console.WriteLine("No Robots left");
                    break;
                }
            }

            this.VRCount = this.VRCount - VRCount;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }

        public void CreateDBSender(int DBProcessorCount, string DataSource, string UserID, string Password, string InitialCatalog)
        {
            for (int i = 0; i < DBProcessorCount; i++)
            {
                Thread th = new Thread(() =>
                {
                    DBSender DBSender = new DBSender(DataSource, UserID, Password, InitialCatalog);
                    DBSender.StartSender(VCQueue);
                });
                th.Name = "DBProcessor_" + (this.DBProcessorCount + i);
                th.Start();
                ThreadListDBProcessors.Add(th);
            }
            this.DBProcessorCount = this.DBProcessorCount + DBProcessorCount;
        }

        public string PrintValues()
        {
            Console.WriteLine("Virtual Controller port = " + VCport);
            Console.WriteLine("Robots count = " + Robots.Count);

            return "Virtual Controller port = " + VCport + "\nRobots count = " + VRCount + "";
        }

    }
}

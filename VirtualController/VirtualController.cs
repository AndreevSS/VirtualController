using CSharpTest.Net.Http;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class VirtualController
    {
        ArrayList ThreadListDBProcessors = new ArrayList();
        ArrayList RobotPorts = new ArrayList();
        ArrayList Robots = new ArrayList();
        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();
        HttpServer server;

        int VCport;
        int VRCount;
        int DBProcessorCount;
        
        public VirtualController(int port,  ArrayList RobotPorts)
        {
            VCport = port;
            this.RobotPorts = RobotPorts;
            server = new HttpServer(10);
            server.Start(new string[] { "http://*:" + port + "/" });
            server.ProcessRequest += Server_ProcessRequest;

            Console.WriteLine("VirtualController created on " + port);
        }

        public void Server_ProcessRequest(object sender, HttpContextEventArgs e)
        {
            // Note: The GetContext method blocks while waiting for a request.
            HttpListenerContext context = e.Context;
            HttpListenerRequest request = context.Request;
            // Obtain a response object.
            HttpListenerResponse response = context.Response;
            // Construct a response.

            NameValueCollection BodyCol = new NameValueCollection();
            BodyCol = ConnectionHandler.KeysAndValuesFromBody(request.InputStream);

            if (context.Request.LocalEndPoint.Port == VCport)
            switch (context.Request.RawUrl)
            {
                default: ConnectionHandler.SimpleTextResponse(context, "I'm Virtual Controller"); break;
                case "/CreateRobots/":
                        ConnectionHandler.SimpleTextResponse(context, "CreatingRobots " + BodyCol.Get("count"));
                    CreateRobots(Convert.ToInt32(BodyCol.Get("count")));
                    break;
                case "/StopRobots/":
                        ConnectionHandler.SimpleTextResponse(context, "StoppingRobots " + BodyCol.Get("count"));
                    StopRobots(Convert.ToInt32(BodyCol.Get("count")));
                    break;
                case "/Values/": ConnectionHandler.SimpleTextResponse(context, PrintValues()); break;
            }
        }
        public void CreateRobots(int VRCount)
        {
            string[] prefixesToAdd = new string[VRCount];

            for (int i = 0; i < VRCount; i++)
            {
                if (RobotPorts.Count > 0)
                {
                    int port = (int)RobotPorts[0];
                    RobotPorts.Remove(port);
                    VirtualRobot VR = new VirtualRobot(port, Convert.ToString(port), VCQueue, server);
                    prefixesToAdd[i] = "http://*:" + VR.port + "/";
                    Robots.Add(VR);
                }
                else
                    Console.WriteLine("RobotPorts Array is Empty");
            }

            server.Start(new string[] { "prefixesToAdd" });
            this.VRCount = this.VRCount + VRCount;
        }
        public void StopRobots(int VRCount)

        {
            string[] prefixesToRemove = new string[VRCount];

            for (int i = 0; i < VRCount; i++)
            {
                if (Robots.Count > 0)
                {
                    VirtualRobot VR = (VirtualRobot)Robots[Robots.Count - 1];
                    prefixesToRemove[i] = "http://*:" + VR.port + "/";
                    Robots.Remove(VR);                    
                    RobotPorts.Add(VR.port);
                    RobotPorts.Sort();
                    Console.WriteLine("Robot " + VR.port + " stopped " + Robots.Count + " left");
                }
                else
                    Console.WriteLine("No Robots left");
            }

            server.RemovePrefixes(prefixesToRemove);

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

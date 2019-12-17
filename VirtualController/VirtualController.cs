using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class VirtualController
    {
        ArrayList ThreadListRobots = new ArrayList();
        ArrayList ThreadListDBProcessors = new ArrayList();
        ArrayList RobotPorts = new ArrayList();
        ArrayList Robots = new ArrayList();

   //     Dictionary<int, VirtualRobot> RobotPorts = new Dictionary<int, VirtualRobot>();


        int VCport;
        int VRCount;
        int VRPorts;
        int DBProcessorCount = 0;

        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();

        public void CreateController(int port, int VRPorts, ArrayList RobotPorts)
        {
            VCport = port;
            this.VRPorts = VRPorts;
            this.RobotPorts = RobotPorts;

            Thread VCThread = new Thread(() =>
            {
                HTTPListener_VirtualController HTTPListener = new HTTPListener_VirtualController();
                HTTPListener.CreateListener(VCport, this);
            });
            VCThread.Name = "VirtualController_Thread";
            VCThread.Start();
        }

        public void CreateRobots(int VRCount)

        {
            for (int i = 0; i < VRCount; i++)
            {
                if (RobotPorts.Count > 0)
                {
                    int port = (int)RobotPorts[0];
                    RobotPorts.Remove(port);
                    VirtualRobot VR = new VirtualRobot(port, Convert.ToString(port) /*Robots.Count*/, VCQueue, ThreadListRobots);
                    Robots.Add(VR);
                 //   VRPortsCount++;
                }
                else
                    Console.WriteLine("RobotPorts Array is Empty");
            }

            this.VRCount = this.VRCount + VRCount;
        }

        public void StopRobots(int VRCount)

        {
            for (int i = 0; i < VRCount; i++)
            {
                if (Robots.Count > 0 )
                {
                    VirtualRobot VR = (VirtualRobot) Robots[Robots.Count - 1];

                    Console.WriteLine("Robot " + VR.port + " stopped");
                    VR.isStopped = true;                    
                    Robots.Remove(VR);
                    RobotPorts.Add(VR.port);
                    RobotPorts.Sort();
                    VR.HTTPListener.listener.Abort();

                }
                else
                    Console.WriteLine("No Robots left");
            }

            this.VRCount = this.VRCount - VRCount;
        }

        public void CreateDBProcessor(int DBProcessorCount, string DataSource, string UserID, string Password, string InitialCatalog)

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
            Console.WriteLine("1st Virtual Robot = " + VRPorts);
            Console.WriteLine("Robot count = " + VRCount);

            return "Virtual Controller port = " + VCport + "\n1st Virtual Robot = " + VRPorts + "\nRobot count = " + VRCount + "";
        }

    }
}

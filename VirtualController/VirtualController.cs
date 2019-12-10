using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ru.pflb.VirtualController
{
    public class VirtualController
    {
        ArrayList ThreadListRobots = new ArrayList();
        ArrayList ThreadListDBProcessors = new ArrayList();


        int VCport;
        int VRCount;
        int VRPorts;
        int VRPortsCount = 0;
        int DBProcessorCount = 0;
        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();

        public void CreateController(int port, int VRPorts)
        {

            VCport = port;
            this.VRPorts = VRPorts;

            Thread VCThread = new Thread(() =>
            {
                HTTPListener.CreateListener(VCport, this);
            });
            VCThread.Name = "VirtualController_Thread";
            VCThread.Start();
        }

        public void CreateRobots(int VRCount)

        {
            for (int i = 0; i < VRCount; i++)
            {
                int port = VRPortsCount + VRPorts;
                VirtualRobot VR = new VirtualRobot(port, VRPortsCount, null);

                VCQueue.Enqueue(Convert.ToString("Robot Created: " + (this.VRCount + i)));

                Thread th = new Thread(() =>
                {
                    HTTPListener.CreateListener(port, VR, VCQueue);
                });
                th.Name = "Robot_" + i;
                th.Start();
                ThreadListRobots.Add(th);

                VRPortsCount++;
            }

            this.VRCount = this.VRCount + VRCount;
        }

        public void CreateDBProcessor(int DBProcessorCount)

        {
            for (int i = 0; i < DBProcessorCount; i++)
            {
                Thread th = new Thread(() =>
                {
                    DBProcessor.StartProcessor(VCQueue);
                });
                th.Name = "DBProcessor_" + (this.DBProcessorCount + i);
                th.Start();
                ThreadListDBProcessors.Add(th);
            }

            this.DBProcessorCount = this.DBProcessorCount + DBProcessorCount;
        }


        public string GetValues()
        {
            Console.WriteLine("Virtual Controller port = " + VCport);
            Console.WriteLine("1st Virtual Robot = " + VRPorts);
            Console.WriteLine("Robot count = " + VRCount);

            return "Virtual Controller port = " + VCport + "\n1st Virtual Robot = " + VRPorts + "\nRobot count = " + VRCount + "";
        }

    }
}

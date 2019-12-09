using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace VirtualController
{
    public class VirtualController
    {
        ArrayList ThreadList = new ArrayList();

        int VCport;
        int VRCount;
        int VRPorts;
        int VRPortsCount = 0;
        int DBProcessorCount = 0;
        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();

        public void CreateController(int port, int VRPorts)
        {
            
            this.VCport = port;
            this.VRPorts = VRPorts;
            
                Thread th = new Thread(() => {
                    HTTPListener.CreateListener(VCport, this);
                });
                th.Name = "VirtualController_Thread";
                th.Start();
                ThreadList.Add(th);
        }

        public void CreateRobots(int VRCount)

        {
               for (int i = 0; i < VRCount; i++)
            {
                int port = VRPortsCount + VRPorts;
                VirtualRobot VR = new VirtualRobot(port, VRPortsCount, null);

                VCQueue.Enqueue(Convert.ToString("Robot Created: " + (this.VRCount + i)));

                Thread th = new Thread(() => {
                    HTTPListener.CreateListener(port, VR, VCQueue);
                });
                th.Name = "Robot_" + i;
                th.Start();
                ThreadList.Add(th);

                VRPortsCount++;
            }

            this.VRCount = this.VRCount  + VRCount;
        }

        public void CreateDBProcessor(int DBProcessorCount)

        {
            for (int i = 0; i < DBProcessorCount; i++)
            {
                Thread th = new Thread(() => {
                    DBProcessor.StartProcessor(VCQueue);
                });
                th.Name = "DBProcessor_" + (this.DBProcessorCount + i);
                th.Start();
                ThreadList.Add(th);
            }

            this.DBProcessorCount = this.DBProcessorCount + DBProcessorCount;
        }


        public String GetValues()
        {
            Console.WriteLine("Virtual Controller port = " + VCport);
            Console.WriteLine("1st Virtual Robot = " + VRPorts);
            Console.WriteLine("Robot count = " + VRCount);

            return "Virtual Controller port = " + VCport + "\n1st Virtual Robot = " + VRPorts + "\nRobot count = " + VRCount + "";
        }

    }
}

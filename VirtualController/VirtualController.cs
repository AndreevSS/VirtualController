using System;
using System.Collections;
using System.Threading;


namespace VirtualController
{
    public class VirtualController
    {
        ArrayList ThreadList = new ArrayList();

        int VCport;
        int VRCount;
        int VRPorts;
        int VRPortsCount = 0;

        public void CreateController(int port, int VRPorts)
        {
            this.VCport = port;
            this.VRPorts = VRPorts;
            
                Thread th = new Thread(() => {
                    HTTPListener.CreateListener(VCport, this);
                    //calling callback function
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

                Thread th = new Thread(() => {
                    HTTPListener.CreateListener(port, VR);
                    //calling callback function
                });
                th.Name = "Robot_" + i;
                th.Start();
                ThreadList.Add(th);

                VRPortsCount++;
            }




            this.VRCount = this.VRCount  + VRCount;
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
                int port = VRPortsCount + VRPorts + i;

                VirtualRobot VR = new VirtualRobot(port, i, null);

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

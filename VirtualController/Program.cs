using System;
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
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            VirtualController VC = new VirtualController();

            VC.CreateController(8800, 8900);
    //      VC.CreateRobots(50);
    //      VC.CreateRobots(20);
    //      VC.GetValues();

        }
    }
}

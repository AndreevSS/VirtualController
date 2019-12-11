using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ru.pflb.VirtualController
{
    class Program
    {
        static void Main(string[] args)
        {
            int VirtualController_Port = 8800;
            int VirtualRobots_Port = 8900;
            int VirtualRobots_Count = 5;
            int DBProcessors_Count = 5;
            int VirtualRobots_FirstPort = 8900;
            int VirtualRobots_LastPort = 9000;

            String DataSource = "DESKTOP-4BU392E";
            String UserID = "TestLogin";
            String Password = "pwd";
            String InitialCatalog = "MyDB";

            //   DBSender DBSender = new DBSender(DataSource, UserID, Password, InitialCatalog);

            // DBSender.StartConnection();

            ArrayList RobotPorts = new ArrayList();

//            Dictionary<int, VirtualRobot> RobotPorts = new Dictionary<int, VirtualRobot>();

            VirtualController VC = new VirtualController();

            RobotPorts = GeneratePorts(VirtualRobots_FirstPort, VirtualRobots_LastPort);

            VC.CreateController(VirtualController_Port, VirtualRobots_Port, RobotPorts);
            VC.CreateDBProcessor(DBProcessors_Count, DataSource, UserID, Password, InitialCatalog);
            VC.CreateRobots(VirtualRobots_Count);
            //      VC.GetValues();

        }

        public static ArrayList GeneratePorts(int firstport, int lastport)
        {
            // Dictionary<int, VirtualRobot> PortsDictionary = new Dictionary<int, VirtualRobot>();
            ArrayList PortsDictionary = new ArrayList();
            int i = firstport;
            while (i <= lastport)
                PortsDictionary.Add(i++);
            return PortsDictionary;
        }
    }
}

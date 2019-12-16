using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace ru.pflb.VirtualController
{
    class Program
    {
        static void Main(string[] args)
        {

            //load
            //     Properties properties = new Properties("Properties.txt");
            //get value whith default value
            //     string Text = properties.get("VirtualController_Port");
            Dictionary<string, string> Properties = new Dictionary<string, string>();

            Properties = (Dictionary<string, string>) ReadDictionaryFile("Properties.txt");


            int VirtualController_Port = 0;
            int VirtualRobots_Port = 0;

            int VirtualRobots_Count = 0;
            int DBProcessors_Count = 0;
            int VirtualRobots_FirstPort = 0;
            int VirtualRobots_LastPort = 0;

            String DataSource;
            String UserID;
            String Password;
            String InitialCatalog;


            Properties.TryGetValue("VirtualController_Port", out string Result);
            VirtualController_Port = Convert.ToInt32(Result);

            Properties.TryGetValue("VirtualRobots_Port ", out Result);
            VirtualRobots_Port = Convert.ToInt32(Result);

            Properties.TryGetValue("VirtualRobots_Count", out Result);
            VirtualRobots_Count = Convert.ToInt32(Result);

            Properties.TryGetValue("DBProcessors_Count", out Result);
            DBProcessors_Count = Convert.ToInt32(Result);


            Properties.TryGetValue("VirtualRobots_FirstPort", out Result);
            VirtualRobots_FirstPort = Convert.ToInt32(Result);

            Properties.TryGetValue("VirtualRobots_LastPort", out Result);
            VirtualRobots_LastPort = Convert.ToInt32(Result);
          

            Properties.TryGetValue("DataSource", out DataSource);
           
            Properties.TryGetValue("UserID", out UserID);

            Properties.TryGetValue("Password", out Password);

            Properties.TryGetValue("InitialCatalog", out InitialCatalog);

//            Properties.TryGetValue("UserID", out UserID);

            /*
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
            */
           
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

        public static IDictionary ReadDictionaryFile(string fileName)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(fileName))
            {
                if ((!string.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    dictionary.Add(key, value);
                }
            }

            return dictionary;
        }


    }
}

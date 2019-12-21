using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
            
            int DBProcessors_Count = 0;
            int VirtualRobots_FirstPort = 0;
            int VirtualRobots_LastPort = 0;

            String DataSource;
            String UserID;
            String Password;
            String InitialCatalog;


            Properties.TryGetValue("VirtualController_Port", out string Result);
            VirtualController_Port = Convert.ToInt32(Result);

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

            List<int> RobotPorts = new List<int>();
            
            RobotPorts = GeneratePorts(VirtualRobots_FirstPort, VirtualRobots_LastPort);
        
            

            VirtualController VC = new VirtualController(VirtualController_Port, RobotPorts);
            VC.CreateDBSender(DBProcessors_Count, DataSource, UserID, Password, InitialCatalog);
            VC.CreateDBSender(DBProcessors_Count, DataSource, UserID, Password, InitialCatalog);
            VC.CreateDBSender(DBProcessors_Count, DataSource, UserID, Password, InitialCatalog);
            // VC.Reset();
            Console.WriteLine(   VC.getFreePort());

        }

        public static List<int> GeneratePorts(int firstport, int lastport)
        {
            // Dictionary<int, VirtualRobot> PortsDictionary = new Dictionary<int, VirtualRobot>();
            List<int> Ports = new List<int>();
            int i = firstport;
            while (i <= lastport)
                Ports.Add(i++);
            return Ports;
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

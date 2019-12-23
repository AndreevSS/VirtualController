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

            int VirtualController_Port = 0;
            int DBProcessors_Count = 0;
            int[] VRPorts = new int[2]; //FirstPort, LastPort
            string[] DBData = new string[4]; //"DataSource", "UserID", "Password", "InitialCatalog"

            Dictionary<string, string> Properties = new Dictionary<string, string>();
            
            try
            {
                Properties = (Dictionary<string, string>)ReadDictionaryFile("Properties.txt");


                VirtualController_Port = Convert.ToInt32(Properties["VirtualController_Port"]);
                DBProcessors_Count = Convert.ToInt32(Properties["DBProcessors_Count"]);
                VRPorts[0] = Convert.ToInt32(Properties["VirtualRobots_FirstPort"]);
                VRPorts[1] = Convert.ToInt32(Properties["VirtualRobots_LastPort"]);

                DBData[0] = Properties["DataSource"];
                DBData[1] = Properties["UserID"];
                DBData[2] = Properties["Password"];
                DBData[3] = Properties["InitialCatalog"];

                List<int> RobotPorts = GeneratePorts(VRPorts[0], VRPorts[1]);
                List<DBSender> DBSenders = new List<DBSender>();

                VirtualController VC = new VirtualController(VirtualController_Port, RobotPorts, DBData, DBSenders);
                VC.CreateDBSender(DBProcessors_Count);

            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("\n\rProperties.txt имеет неверный формат");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("\n\rProperties.txt не найден");
            }



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

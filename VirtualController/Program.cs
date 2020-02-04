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
            int UseBPAResourceUpdater = 0;
            int VirtualController_Port = 0;
            int DBProcessors_Count = 0;
            int Robots_Count = 0;
            int HTTP_Threads_Count = 0;
            int[] VRPorts = new int[2]; //FirstPort, LastPort
            string[] DBData = new string[4]; //"DataSource", "UserID", "Password", "InitialCatalog"
            string[] InfluxData = new string[4];
            string EmulName = "";
            string BPAPrefix = "";

            Dictionary<string, string> Properties = new Dictionary<string, string>();
            /*
            try
            {
                string path = @"log\hlo.log";
                using (StreamWriter sw = File.AppendText(path))
                {
                  
                        sw.WriteLine("5r5");
                    
                }
             //   System.IO.File.WriteAllText(@"log\hlo.log", "5r5");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            */
            try
            {
                Properties = (Dictionary<string, string>)ReadDictionaryFile("Properties.txt");

                VirtualController_Port = Convert.ToInt32(Properties["VirtualController_Port"]);
                DBProcessors_Count = Convert.ToInt32(Properties["DBProcessors_Count"]);
                Robots_Count = Convert.ToInt32(Properties["Robots_Count"]);
                HTTP_Threads_Count = Convert.ToInt32(Properties["HTTP_Threads_Count"]);
                UseBPAResourceUpdater = Convert.ToInt32(Properties["UseBPAResourceUpdater"]);

                VRPorts[0] = Convert.ToInt32(Properties["VirtualRobots_FirstPort"]);
                VRPorts[1] = Convert.ToInt32(Properties["VirtualRobots_LastPort"]);

                DBData[0] = Properties["DataSource"];
                DBData[1] = Properties["UserID"];
                DBData[2] = Properties["Password"];
                DBData[3] = Properties["InitialCatalog"];

                InfluxData[0] = Properties["DataSourceInflux"];
                //InfluxData[1] = Properties["UserIDInflux"];
                InfluxData[1] = Properties["EmulName"];
                InfluxData[2] = "";
                InfluxData[3] = "RRtest";

                //EmulName = Properties["EmulName"];
                BPAPrefix = Properties["BPAPrefix"];

                List<int> RobotPorts = GeneratePorts(VRPorts[0], VRPorts[1]);
                List<DBSender> DBSenders = new List<DBSender>();

                VirtualController VC = new VirtualController(VirtualController_Port, RobotPorts, DBData,
                    DBSenders, HTTP_Threads_Count, UseBPAResourceUpdater, InfluxData, BPAPrefix);
                VC.CreateDBSender(DBProcessors_Count);
                VC.CreateRobots(Robots_Count);

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
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("rerror" + e);
                //              Console.WriteLine("\n\rProperties.txt не найден");
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

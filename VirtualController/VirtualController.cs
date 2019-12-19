using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class VirtualController
    {

        Thread thread1 = null;
        ArrayList ThreadListRobots = new ArrayList();
     //   bool firstrun = true;

        ArrayList ThreadListDBProcessors = new ArrayList();
        ArrayList RobotPorts = new ArrayList();
        ArrayList Robots = new ArrayList();

        //     Dictionary<int, VirtualRobot> RobotPorts = new Dictionary<int, VirtualRobot>();

        Exception myexp = new Exception("Thread shutdown");

        int VCport;
        int VRCount;
        int VRPorts;
        int DBProcessorCount = 0;

        ConcurrentQueue<string> VCQueue = new ConcurrentQueue<string>();

        public VirtualController(int port, int VRPorts, ArrayList RobotPorts)
        {
            VCport = port;
            this.VRPorts = VRPorts;
            this.RobotPorts = RobotPorts;

            Thread VCThread = new Thread(() =>
            {
                HTTPListener_VirtualController HTTPListener = new HTTPListener_VirtualController();
                HTTPListener.CreateListener(VCport, this);
            });
            VCThread.Name = "VirtualController_Thread";
            VCThread.Start();
        }

        public void CreateRobots(int VRCount)

        {



            for (int i = 0; i < VRCount; i++)
            {

                if (RobotPorts.Count > 0)
                {

                    int port = (int)RobotPorts[0];
                    RobotPorts.Remove(port);
                    VirtualRobot VR = new VirtualRobot(port, Convert.ToString(port), VCQueue);
                    Thread thread = new Thread(VR.VRThread);
                    thread.Name = "VR_" + VR.id;
                    VR.thread = thread;
                    ThreadListRobots.Add(thread);

                    if (thread1 is null)
                    thread1 = thread;


                    Robots.Add(VR);
                    //   VRPortsCount++;
                }
                else
                    Console.WriteLine("RobotPorts Array is Empty");
            }

            foreach (Thread thread in ThreadListRobots)
            {
                if (!(thread.IsAlive))
                {
                    try
                    {
                        thread.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            this.VRCount = this.VRCount + VRCount;
        }



        public void StopRobots(int VRCount)

        {
            foreach (VirtualRobot VR in Robots)
            {
                Console.WriteLine("RobotPorts.Count: " + RobotPorts.Count + " Robots Count: " + Robots.Count + " VR " + VR.id);
            }

            PrintValues();



            for (int i = 0; i < VRCount; i++)
            {
                if (Robots.Count > 0)
                {

                    VirtualRobot VR = (VirtualRobot)Robots[Robots.Count - 1];
                    Robots.Remove(VR);
                    ThreadListRobots.Remove(VR.thread);

                    RobotPorts.Add(VR.port);
                    RobotPorts.Sort();

                    Console.WriteLine("Robot " + VR.port + " stopped " + Robots.Count + " left");
                    Console.WriteLine("VR.isFinished " + VR.isFinished);
                    
                    //  Thread.Sleep(5000);
                    //   FieldInfo f = VR.HTTPListener_Robot.listener.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //   Console.WriteLine(f.GetValue(VR.HTTPListener_Robot.listener));
                    //  Thread.Sleep(500);
          //       VR.HTTPListener_Robot.listener.Stop();
                    //     f = VR.HTTPListener_Robot.listener.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //     Console.WriteLine(f.GetValue(VR.HTTPListener_Robot.listener));
                    ////  Thread.Sleep(500);/*/*
           //         VR.HTTPListener_Robot.listener.Abort();
                    //  f = VR.HTTPListener_Robot.listener.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //  Console.WriteLine(f.GetValue(VR.HTTPListener_Robot.listener));
                    ////  Thread.Sleep(500);
                    //VR.HTTPListener_Robot.listener.Close();
                    //  f = VR.HTTPListener_Robot.listener.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //   Console.WriteLine(f.GetValue(VR.HTTPListener_Robot.listener));*/*/

                    //    VR.HTTPListener_Robot = null;


                    VR.StopThread();

                    Console.WriteLine("VR.isFinished " + VR.isFinished);
                //    VR = null;

             //       throw (myexp);
                }
                else
                    Console.WriteLine("No Robots left");
            }

            this.VRCount = this.VRCount - VRCount;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
            
        }

        public void PrintDebug()
        {
         
          
              while (true)
              {
                  foreach (Thread thread in ThreadListRobots)
                  {
                     Console.WriteLine(thread.Name + " " + thread.ThreadState);
                }
                Thread.Sleep(1000);
             }
        }

        public void CreateDBProcessor(int DBProcessorCount, string DataSource, string UserID, string Password, string InitialCatalog)

        {
            for (int i = 0; i < DBProcessorCount; i++)
            {
                Thread th = new Thread(() =>
                {
                    DBSender DBSender = new DBSender(DataSource, UserID, Password, InitialCatalog);
                    DBSender.StartSender(VCQueue);
                });
                th.Name = "DBProcessor_" + (this.DBProcessorCount + i);
                th.Start();
                ThreadListDBProcessors.Add(th);
            }

            this.DBProcessorCount = this.DBProcessorCount + DBProcessorCount;
        }


        public string PrintValues()
        {
            Console.WriteLine("Virtual Controller port = " + VCport);
            Console.WriteLine("1st Virtual Robot = " + VRPorts);
            Console.WriteLine("Robot count = " + Robots.Count);

            return "Virtual Controller port = " + VCport + "\n1st Virtual Robot = " + VRPorts + "\nRobot count = " + VRCount + "";
        }

    }
}

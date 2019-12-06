using System;


namespace VirtualController
{
    class Program
    {
        static void Main(string[] args)
        {
            int VirtualController_Port = 8800;
            int VirtualRobots_Port = 8900;
            int VirtualRobots_Count = 0;


            Console.WriteLine("Hello World!");

            VirtualController VC = new VirtualController();

            VC.CreateController(VirtualController_Port, VirtualRobots_Port);
            VC.CreateRobots(VirtualRobots_Count);
    //      VC.CreateRobots(20);
    //      VC.GetValues();

        }
    }
}

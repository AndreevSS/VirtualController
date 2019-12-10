using System;


namespace ru.pflb.VirtualController
{
    class Program
    {
        static void Main(string[] args)
        {
            int VirtualController_Port = 8800;
            int VirtualRobots_Port = 8900;
            int VirtualRobots_Count = 1;
            int DBProcessors_Count = 5;

            VirtualController VC = new VirtualController();

            VC.CreateController(VirtualController_Port, VirtualRobots_Port);
            VC.CreateDBProcessor(DBProcessors_Count);
            VC.CreateRobots(VirtualRobots_Count);
            //      VC.GetValues();

        }
    }
}

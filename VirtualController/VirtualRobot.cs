using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class VirtualRobot
    {
        int port;
        public int id;
        public string token;
        public VirtualSession VS;
        public VirtualRobot(int port, int id, string token, ConcurrentQueue<string> VCQueue, ArrayList ThreadListRobots)
        {
            this.port = port;
            this.id = id;
            this.token = token;

            Thread th = new Thread(() =>
            {
                HTTPListener_Robots HTTPListener = new HTTPListener_Robots();
                HTTPListener.CreateListener(port, this, VCQueue);
            });
            th.Name = "Robot_" + id;
            th.Start();
            ThreadListRobots.Add(th);
        }

        public string GetValues()
        {
            Console.WriteLine("Robot ID = " + id);
            Console.WriteLine("port:  " + port);
            Console.WriteLine("token = " + token);

            return "Robot ID = " + id + "\nport: " + port + "\ntoken = " + token + "";
        }

        public void CreateSession(int id, int time, int duration, string status)
        {
            if ((time != null) && (duration != null))
                VS = new VirtualSession(id, time, duration, status);
        }
    }
}

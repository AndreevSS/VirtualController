using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class VirtualRobot
    {
        int port;
        public string id;
        public string token;
        public VirtualSession VS;
        public HTTPListener_Robots HTTPListener;
        public VirtualRobot(int port, string id, ConcurrentQueue<string> VCQueue, ArrayList ThreadListRobots)
        {
            this.port = port;
            this.id = id;
            this.token = null;

            Thread th = new Thread(() =>
            {
                HTTPListener_Robots HTTPListener = new HTTPListener_Robots();
                this.HTTPListener = HTTPListener;
                HTTPListener.CreateListener(port, this, VCQueue);
            });
            th.Name = "Robot_" + id;
            th.Start();
            ThreadListRobots.Add(th);
        }

        public string PrintValues()
        {
            Console.WriteLine("Robot ID = " + id);
            Console.WriteLine("port:  " + port);
            Console.WriteLine("token = " + token);

            return "Robot ID = " + id + "\nport: " + port + "\ntoken = " + token + "";
        }

        public void CreateSession(string id, int time, int duration, string status)
        {
            if ((time != 0) && (duration != 0))
                VS = new VirtualSession(id, time, duration, status);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class VirtualRobot  //: IDisposable
    {
        public bool isFinished;
        public Thread thread;
        public ConcurrentQueue<string> VCQueue;
        public bool isStopped = false;
        public int port;
        public string id;
        public string token;
        public VirtualSession VS;
        public HTTPListener_Robot HTTPListener_Robot;
        public VirtualRobot(int port, string id, ConcurrentQueue<string> VCQueue)
        {
            this.port = port;
            this.id = id;
            this.token = null;
            this.VCQueue = VCQueue;
 //           isFinished = false;
        }

        public string PrintValues()
        {
            Console.WriteLine("Robot ID = " + id);
            Console.WriteLine("port:  " + port);
            Console.WriteLine("token = " + token);

            return "Robot ID = " + id + "\nport: " + port + "\ntoken = " + token + "";
        }

        public void CreateSession(string id, int time, int duration, int status)
        {
            if ((time != 0) && (duration != 0))
                VS = new VirtualSession(id, time, duration, status);
        }

        public void VRThread()
        {
  
            HTTPListener_Robot = new HTTPListener_Robot();
            HTTPListener_Robot.CreateListener(port, this, VCQueue, isStopped);
            isFinished = true;
        }

        public void StopThread()
        {
            isStopped = true;
            HTTPListener_Robot.listener.Abort();      
        }




    }
}



















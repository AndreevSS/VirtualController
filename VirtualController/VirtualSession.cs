using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime;
using System.Collections.Concurrent;

namespace ru.pflb.VirtualController
{
    class VirtualSession
    {
        public int id;
        public int time;
        public int duration;

        public string status;


        public VirtualSession(int id, int time, int duration, string status)
        {
            this.id = id;
            this.time = time;
            this.duration = duration;
            this.status = status;
        }

        public void Start(ConcurrentQueue<string> VCQueue)
        {
            Random Random = new Random();
            double SleepTime = duration * 0.9 + duration * (Random.NextDouble() * 0.2);

            
            status = "working";
            VCQueue.Enqueue("Session " + id + " is " + status + " for " + SleepTime);
            Thread sleep = new Thread(() =>
            {
                Thread.Sleep(Convert.ToInt32(SleepTime));
                status = "finished";
                VCQueue.Enqueue("Session " + id + " is " + status);
            });
            sleep.Start();
        }
    }
}

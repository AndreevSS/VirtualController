using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime;

namespace VirtualController
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

        public void Start()
        {
            Random Random = new Random();

            double SleepTime = duration * 0.9 + duration * (Random.Next(1000) * 0.002 - 0.1 );
            this.status = "working";
            Thread.Sleep(Convert.ToInt32(SleepTime * 1000));
            this.status = "finished";
        }
    }
}

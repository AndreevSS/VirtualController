using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ru.pflb.VirtualController
{
    class VirtualSession
    {
        public string id;
        public int time;
        public int duration;
        public int status;


        public VirtualSession(string id, int time, int duration, int status)
        {
            this.id = id;
            this.time = time;
            this.duration = duration;
            this.status = status;
        }

        public void Start(ConcurrentQueue<string> VCQueue)
        {
            if (!(this is null) || this.status != 1)
            {
                Random Random = new Random();
                double SleepTime = duration * 0.9 + duration * (Random.NextDouble() * 0.2);

                status = 1;
                VCQueue.Enqueue(DBQueries.UpdateSession(id, status));

                Thread sleep = new Thread(() =>
                {
                    Thread.Sleep(Convert.ToInt32(SleepTime));
                    status = 2;
                    VCQueue.Enqueue(DBQueries.UpdateSession(id, status));
                });
                sleep.Start();
            }
        }
    }
}

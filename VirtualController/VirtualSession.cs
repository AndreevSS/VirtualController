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
        public string status;


        public VirtualSession(string id, int time, int duration, string status)
        {
            this.id = id;
            this.time = time;
            this.duration = duration;
            this.status = status;
        }

        public void Start(ConcurrentQueue<string> VCQueue)
        {
            if (!(this is null) || this.status != "working")
            {
                Random Random = new Random();
                double SleepTime = duration * 0.9 + duration * (Random.NextDouble() * 0.2);

                status = "1 (Running)";
                String RequestString = "update BPASession " +
                                       "set statusid = '" + status + "' " +
                                       "where sessionid = '" + id + "';";

                VCQueue.Enqueue(RequestString);

                Thread sleep = new Thread(() =>
                {
                    Thread.Sleep(Convert.ToInt32(SleepTime));
                    status = "2";

                    String RequestString = "update BPASession " +
                                           "set statusid = '" + status + "' " +
                                           "where sessionid = '" + id + "';";

                    VCQueue.Enqueue(RequestString);
                });
                sleep.Start();
            }
        }
    }
}

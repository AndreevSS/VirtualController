using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ru.pflb.VirtualController
{
  public  class VirtualSession
    {
        VirtualRobot VR;
        public string id;
        public int time;
     //   public int duration;
        public int status;
        public int port;
      

        public long nextUpdate;
        public long finishTime;

        public VirtualSession(string id, int status, int port, VirtualRobot VR)
        {
            this.VR = VR;
            this.id = id;
            this.time = 0;            
     //       this.duration = 0;
            this.status = status;
            this.port = port;
            
            this.nextUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    }

        public void Start(ConcurrentQueue<DBQueueObject> VCQueue, int duration, bool UseBPAQueue, string bpaqueuename, string tag)
        {

    //        this.duration = duration;
            if (!(this is null) || this.status != 1)
            {
               
                Random Random = new Random();
                double SleepTime = duration * 0.9 + duration * (Random.NextDouble() * 0.2);
                SleepTime = SleepTime * 1000;

                this.finishTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (int)(SleepTime * 0.001);

                status = 1;

                DBQueueObject DBQueueObject = new DBQueueObject(VR);
                DBQueueObject.Query = DBQueries.InsertBPACaseLock(VR.VS.id, bpaqueuename, tag);
                VCQueue.Enqueue(DBQueueObject);

                Thread.Sleep(1);
                DBQueueObject = new DBQueueObject(VR);
                DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 1, 1);               
                VCQueue.Enqueue(DBQueueObject);

                Thread.Sleep(1);

                DBQueueObject = new DBQueueObject(VR);
                DBQueueObject.needupdate = true;
                DBQueueObject.Query = DBQueries.UpdateSession(id, status);
                VCQueue.Enqueue(DBQueueObject);

                Thread sleep = new Thread(() =>
                {
                    Thread.Sleep(Convert.ToInt32(SleepTime));
                    status = 2;
                    Console.WriteLine("Sleeptime = " + SleepTime);


                    DBQueueObject = new DBQueueObject(VR);
                    //DBQueueObject.needupdate = true;
                    DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 2, 0);
                    VCQueue.Enqueue(DBQueueObject);

                    Thread.Sleep(1);

                    DBQueueObject = new DBQueueObject(VR);
                //    DBQueueObject.needupdate = true;
                    DBQueueObject.Query = DBQueries.UpdateSessionEndTime(id, status);
                    VCQueue.Enqueue(DBQueueObject);

                    if (UseBPAQueue)
                    {

                        DBQueueObject DBQueueObject;

                        
                        DBQueueObject = new DBQueueObject(VR);
                    //    DBQueueObject.needupdate = true;
                        DBQueueObject.Query = DBQueries.BPAWorkQueueItem(id, bpaqueuename,tag);
                        VCQueue.Enqueue(DBQueueObject);

                        DBQueueObject = new DBQueueObject(VR);
                        DBQueueObject.Query = DBQueries.DeleteFromBPACaseLock(VR.VS.id);
                        VCQueue.Enqueue(DBQueueObject);



                    }

                });
                sleep.Start();
            }
        }
    }
}

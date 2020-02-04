using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ru.pflb.VirtualController
{
    public class VirtualSession
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
            if (!(this is null) && this.status != 1)
            {

                Random Random = new Random();
                double SleepTime = duration * 0.9 + duration * (Random.NextDouble() * 0.2);
                SleepTime = SleepTime * 1000;

                //                this.finishTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (int)(SleepTime * 0.001);

                status = 1;

                DBQueueObject DBQueueObject;

                if (UseBPAQueue)
                {
                    //                Thread.Sleep(1);
                    DBQueueObject = new DBQueueObject(VR);
                    DBQueueObject.needupdate = true;
                    DBQueueObject.Query = DBQueries.InsertBPACaseLock(VR.VS.id, bpaqueuename, tag) + ";" + DBQueries.UpdateBPAResource(port, 1, 1, VR.BPAPrefix) + "; " + DBQueries.UpdateSession(id, status); ;
                    VCQueue.Enqueue(DBQueueObject);
                }

                else
                {
                    DBQueueObject = new DBQueueObject(VR);
                    DBQueueObject.needupdate = true;
                    DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 1, 1, VR.BPAPrefix) + "; " + DBQueries.UpdateSession(id, status); ;
                    VCQueue.Enqueue(DBQueueObject);
                }

                Thread sleep = new Thread(() =>
                {
                    Thread.Sleep(Convert.ToInt32(SleepTime));
                    status = 2;
                    Console.WriteLine("Sleeptime = " + SleepTime);
                    if (!(VR is null))
                    {
                        try
                        {
                            DBQueueObject = new DBQueueObject(VR);

                            if (UseBPAQueue)
                            {
                                DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 2, 0, VR.BPAPrefix) + "; " +
                                DBQueries.UpdateSessionEndTime(id, status) + "; " + DBQueries.BPAWorkQueueItem(id, bpaqueuename, tag) +
                                " ; " + DBQueries.DeleteFromBPACaseLock(VR.VS.id);
                                ;
                                VCQueue.Enqueue(DBQueueObject);
                            }
                            else
                            {
                                DBQueueObject.Query = DBQueries.UpdateBPAResource(port, 2, 0, VR.BPAPrefix) + "; " +
                                DBQueries.UpdateSessionEndTime(id, status);
                                VCQueue.Enqueue(DBQueueObject);
                            }

                        }

                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine("Session Start Error:");
                            if (DBQueueObject.Query is null)
                                Console.WriteLine("\nDBQueueObjectQuery is null");
                            else
                                Console.WriteLine("\nDBQueueObject.Query: " + DBQueueObject.Query);
                            if (DBQueueObject.VR is null)
                                Console.WriteLine("\nDBQueueObjectVR is null");
                            else
                                Console.WriteLine("\nDBQueueObject.VR: " + DBQueueObject.VR.id);
                            if (tag is null)
                                Console.WriteLine("\ntag is null");
                            else Console.WriteLine("tag ="+ tag);
                            Console.WriteLine(e);
                        }
                    }
                });
                sleep.Start();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Collections;

using System.Threading;

namespace ru.pflb.VirtualController
{
    class DBProcessor
    {
        public static void StartProcessor(ConcurrentQueue<string> VCQueue)
        {
            while (true)
            {
                while (VCQueue.IsEmpty)
                {
                    string Result = "";
                    if (VCQueue.TryDequeue(out Result))
                        Console.WriteLine("Thread " + Thread.CurrentThread.Name + " Dequeuing '{0}'", Result);
                }
                Thread.Sleep(50);
            }
            //     Console.WriteLine("End");
        }


    }
}

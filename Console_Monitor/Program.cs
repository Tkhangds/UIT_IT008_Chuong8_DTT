﻿using System.Diagnostics.Metrics;
using System.Reflection;

namespace Console_Monitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // make an instance of this class
            Tester t = new Tester();
            // run outside static Main
            t.DoTest();
        }
    }
    public class Tester
    {
        long counter = 0;

        public void DoTest()
        {
            // create an array of unnamed threads
            Thread[] myThreads = { new Thread(new ThreadStart(Decrementer)), new Thread(new ThreadStart(Incrementer)) };
            // start each thread
            int ctr = 1;
            foreach (Thread myThread in myThreads)
            {
                myThread.IsBackground = true;
                myThread.Start();
                myThread.Name = "Thread" + ctr.ToString();
                ctr++;
                Console.WriteLine("Started thread {0}", myThread.Name);
                Thread.Sleep(50);
            }

            foreach (Thread myThread in myThreads)
            {
                myThread.Join();
            }
            // after all threads end, print a message
            Console.WriteLine("All my threads are done.");
        }

        void Decrementer()
        {
            try
            {
                // synchronize this area of code
                Monitor.Enter(this);
                // if counter is not yet 10 then free the monitor to other 
                // waiting threads, but wait in line for your turn
                if (counter < 10)
                {
                    Console.WriteLine("[{0}] In Decrementer. Counter:{1}. GottaWait!", Thread.CurrentThread.Name, counter);
                    Monitor.Wait(this);
                }

                while (counter > 0)
                {
                    long temp = counter;
                    temp--;
                    Thread.Sleep(1);
                    counter = temp;
                    Console.WriteLine("[{0}] In Decrementer. Counter: {1}.",
                    Thread.CurrentThread.Name, counter);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        void Incrementer()
        {
            try
            {
                Monitor.Enter(this);
                while (counter < 10)
                {
                    long temp = counter;
                    temp++;
                    Thread.Sleep(1);
                    counter = temp;
                    Console.WriteLine("[{0}] In Incrementer. Counter:{1}", Thread.CurrentThread.Name, counter);
                }
                // I'm done incrementing for now, let another
                // thread have the Monitor
                Monitor.Pulse(this);
            }
            finally
            {
                Console.WriteLine("[{0}] Exiting...",
                Thread.CurrentThread.Name);
                Monitor.Exit(this);
            }
        } 
    }
}
    



 
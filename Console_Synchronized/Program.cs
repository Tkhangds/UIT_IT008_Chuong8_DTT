namespace Console_Synchronized
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // create shared object used by threads
            HoldIntegerSynchronized holdInteger = new HoldIntegerSynchronized();

            Random random = new Random();

            // create Producer and Consumer objects
            Producer producer = new Producer(holdInteger, random);
            Consumer consumer = new Consumer(holdInteger, random);

            // output column heads and initial buffer state
            Console.WriteLine("{0,-35}{1,-9}{2}\n",
                "Operation", "Buffer", "Occupied Count");
            holdInteger.DisplayState("Initial state");

            // create threads for producer and consumer and set 
            // delegates for each thread
            Thread producerThread = new Thread(new ThreadStart(producer.Produce));
            producerThread.Name = "Producer";

            Thread consumerThread = new Thread(new ThreadStart(consumer.Consume));
            consumerThread.Name = "Consumer";

            // start each thread
            producerThread.Start();
            consumerThread.Start();

        }
    }

    public class HoldIntegerSynchronized
    {
        // buffer shared by producer and consumer threads 
        private int buffer = -1;

        // occupiedBufferCount maintains count of occupied buffers
        private int occupiedBufferCount = 0;

        // property Buffer
        public int Buffer
        {
            get
            {
                Monitor.Enter(this);

                if (occupiedBufferCount == 0)
                {
                    Console.WriteLine(Thread.CurrentThread.Name + " tries to read.");

                    DisplayState("Buffer empty. " + Thread.CurrentThread.Name + " waits.");

                }
                Monitor.Wait(this);

                --occupiedBufferCount;

                DisplayState(Thread.CurrentThread.Name + " reads " + buffer);
                Monitor.Pulse(this);
                int bufferCopy = buffer;


                Monitor.Exit(this);

                return bufferCopy;

            }
            set
            {
                // acquire the lock for this object
                Monitor.Enter(this);

                // if there are no empty locations, place invoking
                // thread in WaitSleepJoin state
                if (occupiedBufferCount == 1)
                {
                    Console.WriteLine(
                        Thread.CurrentThread.Name + " tries to write."
                    );

                    DisplayState("Buffer full. " +
                        Thread.CurrentThread.Name + " waits.");

                    Monitor.Wait(this);
                }

                // set the new buffer value
                buffer = value;

                // indicate the producer cannot store another value 
                // until the consumer retrieves the current buffer value
                ++occupiedBufferCount;

                DisplayState(
                    Thread.CurrentThread.Name + " writes " + buffer
                );

                // tell the waiting thread (if there is one) to 
                // become ready to execute (Started state)
                Monitor.Pulse(this);
                // release the lock on this object
                Monitor.Exit(this);

            } // end set

        }

        // display current operation and buffer state
        public void DisplayState(string operation)
        {
            Console.WriteLine("{0,-35}{1,-9}{2}\n",
                operation, buffer, occupiedBufferCount);
        }

    }

    class Producer
    {
        private HoldIntegerSynchronized sharedLocation;
        private Random randomSleepTime;

        // constructor
        public Producer(HoldIntegerSynchronized shared, Random random)
        {
            sharedLocation = shared;
            randomSleepTime = random;
        }
        public void Produce()
        {
            // sleep for random interval up to 3000 milliseconds
            // then set sharedLocation's Buffer property
            for (int count = 1; count <= 4; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sharedLocation.Buffer = count;
            }

            Console.WriteLine(Thread.CurrentThread.Name +
                " done producing.\nTerminating " +
                Thread.CurrentThread.Name + ".\n");
        }

    } 

    class Consumer
    {
        private HoldIntegerSynchronized sharedLocation;
        private Random randomSleepTime;

        // constructor
        public Consumer(HoldIntegerSynchronized shared, Random random)
        {
            sharedLocation = shared;
            randomSleepTime = random;
        }

        public void Consume()
        {
            int sum = 0;

            // get current thread
            Thread current = Thread.CurrentThread;

            // sleep for random interval up to 3000 milliseconds
            // then add sharedLocation's Buffer property value
            // to sum
            for (int count = 1; count <= 4; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sum += sharedLocation.Buffer;
            }

            Console.WriteLine(Thread.CurrentThread.Name +
                " read values totaling: " + sum +
                ".\nTerminating " + Thread.CurrentThread.Name + ".\n");
        }

    } 


}



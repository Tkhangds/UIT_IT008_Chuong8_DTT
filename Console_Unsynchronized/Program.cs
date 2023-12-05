namespace Console_Unsynchronized
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // create shared object used by threads
            HoldIntegerUnsynchronized holdInteger = new HoldIntegerUnsynchronized();

            // Random object used by each thread
            Random random = new Random();

            // create Producer and Consumer objects
            Producer producer = new Producer(holdInteger, random);
            Consumer consumer = new Consumer(holdInteger, random);

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

    public class HoldIntegerUnsynchronized
    {
        // buffer shared by producer and consumer threads
        private int buffer = -1;

        // property Buffer
        public int Buffer
        {
            get
            {
                Console.WriteLine(Thread.CurrentThread.Name +
                    " reads " + buffer);

                return buffer;
            }

            set
            {
                Console.WriteLine(Thread.CurrentThread.Name +
                    " writes " + value);

                buffer = value;
            }
        } // end property Buffer
    }

    class Producer
    {
        private HoldIntegerUnsynchronized sharedLocation;
        private Random randomSleepTime;

        // constructor
        public Producer(HoldIntegerUnsynchronized shared, Random random)
        {
            sharedLocation = shared;
            randomSleepTime = random;
        }

        // store values 1-4 in object sharedLocation
        public void Produce()
        {
            // sleep for a random interval up to 3000 milliseconds
            // then set sharedLocation's Buffer property
            for (int count = 1; count <= 4; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sharedLocation.Buffer = count;
            }

            Console.WriteLine(Thread.CurrentThread.Name +
                " done producing.\nTerminating " +
                Thread.CurrentThread.Name + ".");
        }
    }

    class Consumer
    {
        private HoldIntegerUnsynchronized sharedLocation;
        private Random randomSleepTime;

        // constructor
        public Consumer(HoldIntegerUnsynchronized shared, Random random)
        {
            sharedLocation = shared;
            randomSleepTime = random;
        }

        // read sharedLocation's value four times
        public void Consume()
        {
            int sum = 0;

            // sleep for random interval up to 3000 milliseconds
            // then add sharedLocation's Buffer property value to sum
            for (int count = 1; count <= 4; count++)
            {
                Thread.Sleep(randomSleepTime.Next(1, 3000));
                sum += sharedLocation.Buffer;
            }

            Console.WriteLine(Thread.CurrentThread.Name +
                " done consuming.\nTerminating " +
                Thread.CurrentThread.Name + ".");
            Console.WriteLine(Thread.CurrentThread.Name + " read values totaling: " + sum + ".\nTerminating " + Thread.CurrentThread.Name + ".");
        }
    }
}

    

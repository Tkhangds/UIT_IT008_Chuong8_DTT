namespace Console_ThreadPool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
            Console.WriteLine("Main thread does some work, then sleeps. ");
            // If you comment out the Sleep, the main thread exits before 
            // the thread pool task runs. The thread pool uses background 
            // threads, which do not keep the application running. (This 
            // is a simple example of a race condition.) 
            Thread.Sleep(1000);
            Console.WriteLine("Main thread exits. ");
        }

        static void ThreadProc(Object stateInfo)
        { // No state object was passed to QueueUserWorkItem, so 
          // stateInfo is null. 
            Console.WriteLine("Hello from the thread pool. ");
        }
    }
}
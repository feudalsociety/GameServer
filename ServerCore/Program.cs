using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        static void MainThread(object state)
        {
            for(int i = 0; i < 5; i++)
                Console.WriteLine("Hello Thread!");
        }

        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);

            // threadpool에 들어가지만 별도처리를 한다. 뽑아서 사용하는것이 아님
            for (int i = 0; i < 5; i++)
            {
                Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                t.Start();
            }

            // for (int i = 0; i < 5; i++)
            //     ThreadPool.QueueUserWorkItem((obj) => {while(true) { } });

            ThreadPool.QueueUserWorkItem(MainThread);

            // for (int i = 0; i < 1000; i++)
            // {
            //     Thread t = new Thread(MainThread);
            //     t.Name = "Test Thread";
            //     t.IsBackground = true;
            //     t.Start();
            // }

            // Console.ReadLine("Waiting for Thread!");

            // t.Join();
            // Console.WriteLine("Hello World!");

            while(true)
            {
            }
        }
    }
}

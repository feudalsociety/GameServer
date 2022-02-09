using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // 꼼수 - lock을 class로 wrapping한다.
    class FastLock
    {
        public int id; // id 호출 순서를 추적했다가 graph 상에 cycle 있는지 판별
    }

    class SessionManager
    {
        static object _lock = new object();

        public static void TestSession()
        {
            lock(_lock)
            {

            }
        }

        public static void Test()
        {
            lock(_lock)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        public static void Test()
        {
            // Monitor.TryEnter()
            lock(_lock)
            {
                SessionManager.TestSession();
            }
        }

        public static void TestUser()
        {
            lock(_lock)
            {

            }
        }
    }

    class Program
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for(int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }

        // Deadlock
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();

            Thread.Sleep(100);

            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(number);
        }
        
    }
}

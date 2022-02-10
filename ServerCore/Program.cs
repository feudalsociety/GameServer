using System;
using System.Threading;
using System.Threading.Tasks;

// lock free programming와 유사

namespace ServerCore
{
    class Program
    {
        static volatile int count = 0;
        static Lock _lock = new Lock();

        static void main(string[] args)
        {
            Task t1 = new Task(delegate () 
            {
                for(int i = 0; i < 100000; i++)
                {
                    _lock.WriteLock();
                    _lock.WriteLock(); // 중첩 호출 가능
                    count++;
                    _lock.WriteLock();
                    _lock.WriteUnlock();
                }
            });

            Task t2 = new Task(delegate ()
            {
                for(int i = 0; i< 100000; i++)
                {
                    _lock.WriteLock();
                    count--;
                    _lock.WriteUnlock();
                }
            });

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(count);
        }
    }
}

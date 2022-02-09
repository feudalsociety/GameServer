using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        // 공유 변수 접근에 대한 문제점 실험, race condition
        static int number = 0;

        static void Thread_1()
        {
            // atomic - 원자성
            // 집행검 User2 인벤에 넣어라 - ok
            // 집행검 User1 인벤에서 없애라 - fail

            for(int i = 0; i < 100000; i++)
            {
                // 성능에서 손해를 본다는 단점. memory barrier를 간접적으로 사용, number을 volatile로 할 필요가 없음
                // All or Nothing, 한번에 하나만 작업 가능
                int aftervalue = Interlocked.Increment(ref number); // ref가 꼭 붙어야한다.
                // number++;

                // int temp = number;
                // temp += 1;
                // number = temp;
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                Interlocked.Decrement(ref number);
                // number--;

                // int temp = number;
                // temp -= 1;
                // number = temp;
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(number);
        }
        
    }
}

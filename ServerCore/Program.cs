using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        // C#에서 volitle 사용을 추천하지 않는다. 
        volatile static bool _stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("Thread Start!");

            while (_stop == false)
            {
                // 누군가가 stop 신호를 해주기를 기다린다.
            }

            Console.WriteLine("Thread End!");
        }

        static void main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);

            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기중");

            t.Wait();
            Console.WriteLine("종료 성공");
        }
    }
}

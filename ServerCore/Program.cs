using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;

        public void Aquire()
        {   
            while(true)
            {
                int expected = 0;
                int desired = 1;

                if(Interlocked.CompareExchange(ref _locked, desired, expected) == expected) 
                    break;  

                // 쉬다 올게~ 3가지 방법
                // Thread.Sleep(1); // 무조건 휴식 => 무조건 1ms 쉬고 싶어요
                // Thread.Sleep(0); // 조건부 양보 => 나보다 우선순위가 낮은 애들한테는 양보 불가 => 우선순위가 나보다 높은 쓰레드가 없으면 다시 본인선택
                Thread.Yield();  // 관대한 양보 => 지금 실행 가능한 쓰레드가 있으면 실행하세요 => 없으면 남은시간 본인이 소진
            }
        }

        public void Release()
        {
            _locked = 0;
        }
    }

    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for(int i = 0; i < 100000; i++)
            {
                _lock.Aquire();
                _num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for(int i = 0; i < 100000; i++)
            {
                _lock.Aquire();
                _num--;
                _lock.Release();
            }
        }

        static void Main(string[] args)
        {   
            Thread t1 = new Thread(Thread_1);
            Thread t2 = new Thread(Thread_2);

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
            Console.WriteLine(_num);
        }
        
    }
}

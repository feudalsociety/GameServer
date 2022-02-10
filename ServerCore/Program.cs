using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class SpinLock
    {
        // volatile bool _locked = false;
        volatile int _locked = 0;

        public void Aquire()
        {   
            while(true)
            {
                // int original = Interlocked.Exchange(_locked, 1); // original은 stack에 있는 경합하지 않는 하나의 thread에서만 사용
                // if (original == 0) break;

                // CAS Compare and Swap
                int expected = 0;
                int desired = 1;
                // _locked와 expected을 비교해서 같으면 desired을 넣음
                if(Interlocked.CompareExchange(ref _locked, desired, expected) == expected) 
                    break;  
            }

            // {
            //     int original = _locked;
            //     _locked = 1;
            //     if(original == 0) break;
            // }
            // {
            //     if(_locked == 0) _locked = 1; // 범용적
            // }

            // ============================================================
                    
            // // 문안에 들어가서 잠그는거 까지 하나의 행동으로 이어져야함
            // while(_locked)
            // {
            //     // 잠김이 풀리기를 기다림
            // }

            // // 내꺼
            // _locked = true;
        }

        public void Release()
        {
            // _locked = false;
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

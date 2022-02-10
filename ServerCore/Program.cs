using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Lock
    {
        // bool <= 커널
        // AutoResetEvent _available = AutoResetEvent(true); // true 문이 열린상태, false 닫힌 상태

        ManualResetEvent _available = new ManualResetEvent(true);

        public void Aquire()
        {
            // _available.WaitOne(); // 입장 시도, 문을 자동으로 닫음
            // _available.Reset(); 

            _available.WaitOne();
            _available.Reset(); // 두개로 나뉘면 안됨
        }

        public void Release()
        {
            // _available.Set(); // 문을 열어줌
            _available.Set();
        }
    }

    // 운영체제에게 요청을 하기때문에 큰 부담이 됨
    // 이거 말고도 kernal을 이용해서 순서를 정하는게 있다. 커널 동기화 객체 : mutex
    // 많은 정보를 가지고 있다. 몇번이나 잠궜는지 counting, threadId
    class Program
    {
        static int _num = 0;
        // static SpinLock _lock = new SpinLock();
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for(int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for(int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
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

using System;
using System.Threading;
using System.Threading.Tasks;

// lock free programming와 유사

namespace ServerCore
{
    class Program
    {
        // TLS value setting이 안되어있으면 넘겨준 코드를 실행
        // 반드시 static 상황에서 사용해야하는건 아니지만 대부분의 상황에서는 static을 붙여서 사용하는 경우가 많다.
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => {});

        // TLS에서 사용하는 것은 굳이 lock을 걸지 않고 부담없이 뽑아서 쓸 수 있다. 공용 공간 접근 횟수 줄임

        static void WhoAmI()
        {
            bool repeat = ThreadName.IsValueCreated;
            if(repeat) Console.WriteLine(ThreadName.Value + "(repeat)");
            else Console.WriteLine(ThreadName.Value);

            // 같은 thread가 실행될 때마다 매번 덮어쓰고 있다.
            // ThreadName.Value = $"My Name is {Thread.CurrentThread.ManagedThreadId}"; 
            // Thread.Sleep(1000);
            // Console.WriteLine(ThreadName.Value);
        }

        static void main(string[] args)
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            // 넣어주는 action만큼을 task로 만들어 실행
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            ThreadName.Dispose(); // 필요없으면 날림
        }
    }
}

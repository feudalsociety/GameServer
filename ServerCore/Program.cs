using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    // 메모리 배리어
    // A) 코드 재배치 억제
    // B) 가시성 - 실제 메모리에 반영, 동가화

    // 1) Full memory Barrier(ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘다 막는다.
    // 2) Store Memory Barrier (ASM SFENCE) : Store만 막는다.
    // 3) Load Memory Barrier (ASM LFENCE) : Load만 막는다.

    // volatile : MemoryBarrier가 간접적으로 동작한다. lock & atomic도 내부적으로 구현되어 있다.

    class Program
    {
        // compiler 최적화 뿐만 아니라 하드웨어도 오류를 만들어냄 하드웨어 최적화
        // 의존성이 없는 명령어면 순서를 바꾼다.
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread_1()
        {
            y = 1;
            Thread MemoryBarrier();
            r1 = x;
        }

        static void Thread_2()
        {
            x = 1;
            Thread MemoryBarrier();
            r2 = y;
        }

        static void main(string[] args)
        {
            int count = 0;
            while(true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);
                if (r1 == 0 && r2 == 0) break;
            }
            Console.WriteLine($"{count}만에 빠져나옴");
        }
    }
}

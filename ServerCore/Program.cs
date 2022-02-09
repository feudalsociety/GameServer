using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        // Thread로 A와 B가 동시에 실행된다고 가정
        int _answer;
        bool _complete;

        void A()
        {
            _answer = 123;
            Thread MemoryBarrier(); // Store 후 반영
            _complete = true;
            Thread MemoryBarrier(); // Store 후 반영
        }

        void B()
        {
            Thread MemoryBarrier(); // Read 하기전에 동기화
            if(_complete)
            {
                Thread MemoryBarrier();
                Console.WriteLine(_answer); // Read 하기전에 동기화
            }
        }

        static void main(string[] args)
        {
            
        }
    }
}

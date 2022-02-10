using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    // WriteThreadId를 bool로 하지 않은 이유는 재귀적 lock이 필요하게 되면 누가 잡고있는지 알아야하기 때문에
    // 재귀적 lock 허용 yes WriteLock -> WriteLock OK, WriteLock -> ReadLock OK, ReadLock -> WriteLock NO

    // 재귀적 Lock을 허용할지 (No) - Write lock을 얻은 상태에서 또 다시 재귀적으로 같은 Thread에서 aquire을 허용할 것인지
    // SpinLock (5000번 -> yeild)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)], Write는 한번에 한 thread만 획득 
        int _flag = EMPTY_FLAG;
        int _writeCount = 0; 
        // 재귀적으로 몇개의 write할지 관리, write 자체가 상호배타적이어서 flag에 넣을 필요가 없음
        // 누군가가 _writeLock을 잡으면 개만 여기 접근할 수 있으므로 mutlithread 문제가 없다.

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId) 
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을때 경합해서 소유권을 얻는다.
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while(true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // 시도를 해서 성공하면 return
                    // if (_flag == EMPTY_FLAG) _flag = desired; // 동시다발적으로 성공했다고 착각함
                    if(Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1; // writeLock을 잡는데 성공했으면
                        return;
                    }
                }
                Thread.Yield();
            }
        }

        public void WriteUnlock() // 경합할 필요없이 WriteLock한애만 WriteUnlock을 할 수 있다.
        {
            int lockCount = --_writeCount;
            if(lockCount == 0) Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }


        public void ReadLock()
        {
            // 재귀호출 처리
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId) 
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1늘린다.
            while(true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    // if((_flag & WRITE_MASK) == 0) 
                    // {
                    //     _flag = _flag + 1;
                    //     return;
                    // }

                    // WRITE_MASK 할필요가 없다. _flag가 expected와 같으려면 writeMask 부분이 모두 0이여야함
                    int expected = (_flag & READ_MASK); /// A(0) B(0)
                    if(Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected) return; // A(0->1) 성공 B(0->1) 실패
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
    // WriteLock -> ReadLock 하면 풀어줄 때 반대로 ReadUnlock을 먼저한후 WriteUnlock을 해줘야한다.
}

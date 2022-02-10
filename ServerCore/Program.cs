using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    {
        // 1. 근성, 2. 양보, 3. 갑질

        // 상호 배제 Mutual Exclusion
        // 내부적으로 Monitor class 이용
        static object _lock = new object();
        static SpinLock _lock2 = new SpinLock(); // 이미 구현된 spinlock. 1과 2 혼합
        static Mutex _lock3 = new Mutex(); // 느리다. 별도의 프로그램도 순서를 맞출수 있긴한데 MMO Server에서는 안쓸거임
        // 직접 만든다. 
        // 컨텐츠도 멀티쓰레드로 만들것인가? - 경계가 없는 rpg

        // 다른 형태의 lock
        // [] [] [] + [] [] 바꿀때만 상호 배타적으로 막을 수 있다면
        class Reward
        {

        }
        // RWLock ReaderWriteLock Slim 붙은것이 더 최신버전
        static ReaderWriterLockSlim _lock3 = new ReaderWriterLockSlim();


        // 99.99999 + 0.00001 
        static Reward GetRewardByid(int id)
        {
            // 아무도 writelock을 잡고 있지 않으면 동시다발적으로 들어올 수 있다.
            _lock3.EnterReadLock(); 
            _lock3.ExitReadLock();

            // lock(_lock) 
            // { 
            //     // 그냥 읽을 때는 동시다발적으로 접근할 수 있다가 바꿀때만 배타적으로 막음
            // }
            return null;
        }

        // 0.00001
        static void AddReward(Reward reward)
        {
            _lock3.EnterWriteLock();
            _lock3.ExitWriteLock();

            // lock (_lock)
            // {
               
            // }

            return null;
        }

        static void Main(string[] args)
        {   
            lock(_lock)
            {
                
            }

            // SpinLock interface
            bool lockTaken = false;
            try
            {
                _lock2.Enter(ref lockTaken);
            }
            finally
            {
                if(lockTaken) _lock2.Exit(false);
            }
        }   
    }
}

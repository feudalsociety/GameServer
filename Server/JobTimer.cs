using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    struct JobTimerElem : IComparable<JobTimerElem>
    {
        public int execTick; // 실행시간
        public Action action;

        public int CompareTo(JobTimerElem other)
        {
            // 작은 애가 먼저 튀어나오길 원함
            return other.execTick - execTick;
        }
    }

    // 경우에 따라서 여기서 더 최적하는 경우도 있다. 2개로 분리
    // 시간이 임박하면 리스트로 관리
    // [20ms 이 시점에 실행되어야하는 job들이 여기에 list로 연결됨][20ms][20ms][20ms][][][]...
    // 여유롭게 시간이 남은 job들은 우선순위 큐로 관리
    // 100ms에 실행되야하는거라면 다섯번째 버킷에 넣음
    class JobTimer
    {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();
        public static JobTimer Instance { get; } = new JobTimer();
        
        // 당장에 실행하기 원하면 tickafter 인지를 받지 않음
        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElem job;
            job.execTick = System.Environment.TickCount + tickAfter;
            job.action = action;   

            lock(_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while(true)
            {
                int now = System.Environment.TickCount;

                JobTimerElem job;

                lock(_lock)
                {
                    if (_pq.Count == 0)
                        break;

                    job = _pq.Peek();
                    if (job.execTick > now) break;

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
// background thread에서 unity game에서 관리하는 객체에 접근하거나 코드를 실행하면 crash
// game logic은 mainthread에서만 하도록
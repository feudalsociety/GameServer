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

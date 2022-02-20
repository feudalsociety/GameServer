using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public interface IJobQueue
    {
        void Push(Action job);
    }

    public class JobQueue : IJobQueue
    {
        Queue<Action> _jobQueue = new Queue<Action>();
        object _lock = new object();
        bool _flush = false; 

        // Push는 여러명이서 할 수 있음
        public void Push(Action job)
        {
            bool flush = false; // queue에 쌓인것을 실행할것인지
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
                if (_flush == false)
                {
                    flush = _flush = true;
                }
            }

            if (flush)
                Flush();
        }

        // Flush는 혼자서 하는데 왜 Pop을 할 때 lock을 잡는 이유
        // 하나하나씩 꺼내는 와중에도 다른애가 Push할 수 있어서
        void Flush()
        {
            while(true)
            {
                Action action = Pop();
                if (action == null) return;

                action.Invoke();
            }
        }

        Action Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }

                return _jobQueue.Dequeue();
            }
        }


    }
}

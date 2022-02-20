using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom : IJobQueue
    {
        // 자료구조는 dictionary로 id와 session둘 다 가지고 있어도 된다.
        List<ClientSession> _sessions = new List<ClientSession>();

        // packet이 조립되어 handler로 넘어오는것은 동시다발적으로 여러 thread에서 일어날 수 있어서
        // enter와 leave도 동시다발적으로 실행
        // object _lock = new object();
        // 하나의 Thread만 Flush할 수 있음이 보장되므로 Jobqueue를 사용할때 굳이 lock을 사용할 필요가 없다

        JobQueue _jobQueue = new JobQueue();
        // 1. main thread나 다른애가 순차적으로 queue에 있는걸 실행
        // 2. push를 할때 맨처음으로 일감을 밀어넣으면 실제 실행까지 바로 진행

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            // lock (_lock)
            foreach (ClientSession s in _sessions)
                s.Send(segment);
        }

        public void Enter(ClientSession session)
        {
            // lock (_lock)
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            // lock (_lock)
            _sessions.Remove(session);
        }
    }
}

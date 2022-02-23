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

        JobQueue _jobQueue = new JobQueue();

        // 패킷 모아보내기, 엔진단 or 컨텐츠단에서 모아보내는 방법 2가지
        // 여기서는 컨텐츠 단에서 모아보내기 방법
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        // 마찬가지로 여기서 lock을 걸지 않은 이유는 jobQueue에서 하나의 thread만 일감을 실행한다는것이 보장되기 때문에
        public void Flush()
        {
            foreach (ClientSession s in _sessions)
                s.Send(_pendingList);

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);

            // 이부분이 부하를 많이 줌 n^2를 벗어나기 위해서 패킷 모아보내기. n까지 줄일 수 있음
            //foreach (ClientSession s in _sessions)
            //    s.Send(segment);
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}

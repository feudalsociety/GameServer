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

        // 누가 jobQueue를 들고 있을 것인가? - 게임에 따라서 다르다.
        // seamless가 아니면 맵/zone 마다 task를 실행하는 애들을 하나씩 배치
        // seamless인 경우에는 애매함. 옆에있는 zone과 interaction하는 경우
        // game room이나 zone 단위로 배치를 하는것보다 모든 객체에 집어넣는 방법이 있다.
        // (몬스터, 스킬, 유저 모든 객체에 queue를 하나씩 배치)
        JobQueue _jobQueue = new JobQueue();

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

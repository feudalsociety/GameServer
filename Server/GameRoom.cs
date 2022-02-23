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

            //Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ArraySegment<byte> segment)
        {
            // 부하를 줄이기 위해서 패킷 모아보내기
            _pendingList.Add(segment);
        }

        public void Enter(ClientSession session)
        {
            // 플레이어 추가
            _sessions.Add(session);
            session.Room = this;

            // 실제 mmo라면 여기서 들어왔다는 사실을 알려야함.
            S_PlayerList players = new S_PlayerList();
            foreach(ClientSession s in _sessions)
            {
                players.players.Add(new S_PlayerList.Player()
                {
                    isSelf = (s == session),
                    playerId = s.SessionId,
                    posX = s.PosX,
                    posY = s.PosY,
                    posZ = s.PosZ,
                });
            }
            // 신입생한테 모든 플레이어 목록 전달
            session.Send(players.Write());

            // 신입생 입장을 모든 플레이어에게 알림
            // 다만 방금 들어온 애는 재외하고 보내야함.
            // 딱히 방법이 없으니까 client handler에서 예외처리 EnterGame
            S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
            enter.playerId = session.SessionId;
            enter.posX = 0;
            enter.posY = 0;
            enter.posZ = 0;
            Broadcast(enter.Write());
        }

        public void Leave(ClientSession session)
        {
            // 플레이어 제거
            _sessions.Remove(session);
            // 모두에게 알리는 부분
            S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
            leave.playerId = session.SessionId;
            Broadcast(leave.Write());
        }

        public void Move(ClientSession session, C_Move packet)
        {
            // 좌표 바꿔주고
            session.PosX = packet.posX;
            session.PosY = packet.posY;
            session.PosZ = packet.posZ;

            // 모두에게 알린다.
            S_BroadcastMove move = new S_BroadcastMove();
            move.playerId = session.SessionId;
            move.posX = packet.posX;
            move.posY = packet.posY;
            move.posZ = packet.posZ;
            Broadcast(move.Write());
        }
    }
}

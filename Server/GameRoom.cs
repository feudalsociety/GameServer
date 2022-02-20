using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        // 자료구조는 dictionary로 id와 session둘 다 가지고 있어도 된다.
        List<ClientSession> _sessions = new List<ClientSession>();

        // packet이 조립되어 handler로 넘어오는것은 동시다발적으로 여러 thread에서 일어날 수 있어서
        // enter와 leave도 동시다발적으로 실행
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{chat} I am {packet.playerId}";
            ArraySegment<byte> segment = packet.Write();

            // seamless game의 경우 broadcasting 공간을 잡는게 어렵다.
            // 여기서 계속 시간이 밀려서 회수되지 않은 채로 pool에서 새로 생성한다.
            // lock을 잡고 모든 부분을 실행하면 제대로 돌아갈 확률이 낮음. 패킷이 몰릴 수 있다.
            // lock을 잡아서 실행하는것이 아닌 실질적으로 gameRoom을 담당해서 실행하는 것은 한명만 실행되게끔 만들어줘야함
            // 일감을 그냥 queue에다 넣고 실제로 queue는 한번에 하나씩만 작동하게끔 한다.
            // 일감 job이라고 부르는 경우. task라고 부르는 경우
            // 패킷이 왔다는것을 wrapping해서 나중에 여유가 될 때 처리하게끔 미루는것이 핵심
            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                    s.Send(segment);
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }
    }
}

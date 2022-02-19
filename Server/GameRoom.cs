using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class GameRoom
    {
        // 자료구조는 dictionary로 id와 session둘다 가지고 있어도 된다.
        List<ClientSession> _session = new List<ClientSession>();

        // packet이 조립되어 handler로 넘어오는것은 동시다발적으로 여러 thread에서 일어날 수 있어서
        // enter와 leave도 동시다발적으로 실행
        object _lock = new object();

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _session.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _session.Remove(session);
            }
        }
    }
}

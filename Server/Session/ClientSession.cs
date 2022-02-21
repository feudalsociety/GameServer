using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class ClientSession : PacketSession
    {
        // client session에서 내가 어떤 방에 있는지 궁금할 수 있으니까
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        // 엔진과 컨텐츠 분리
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected : {0}", endPoint);

            // 전역 room이므로 문제 없음
            Program.Room.Push(() => Program.Room.Enter(this));
            // Program.Room.Enter(this); // jobQueue 이용, 직접호출불가능
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if(Room != null)
            {
                // 이렇게 하면 crash나는 상황이 없어짐. 
                GameRoom room = Room;
                // 마찬가지로 room이 null이되면 문제가 됨
                // Room.Push(() => Room.Leave(this));
                room.Push(() => room.Leave(this));
                // Room.Leave(this);
                Room = null;
            }

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            // singleton이 아니라 handler 방식으로 등록해서 호출하는 방식으로해도 상관없다.
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numofBytes)
        {
            // Console.WriteLine($"Transferred bytes : {numofBytes}");
        }
    }
}

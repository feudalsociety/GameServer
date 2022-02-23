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
    // 모든 정보를 clientsession에 넣지 않고, 새로운 class 예를 들면 player class를 판다음에 
    // 거기다 컨텐츠 코드를 넣어넣고 player가 연결된 client sessions을 물고있게끔 만들어줌
    // 일단은 간단하게 하기 위해서 여기다 모든 정보를 넣어둠
    class ClientSession : PacketSession
    {
        // client session에서 내가 어떤 방에 있는지 궁금할 수 있으니까
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }


        // 엔진과 컨텐츠 분리
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected : {0}", endPoint);
            // client가 접속 했을 때 바로 입장시키지 않고 client쪽으로 승인을 보내고
            // client쪽에서 모든 resource를 load 했을 때 ok packet을 보내면 방에 입장
            Program.Room.Push(() => Program.Room.Enter(this));
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

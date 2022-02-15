using ServerCore;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    // client쪽에 server 대리자가 ServerSession이고, 반대로 server에서 client의 대리인의 역할을 하는게 clientSession

    // 헤더
    class Packet
    {
        public ushort size;
        public ushort packetId;
    }

    // client에서 요청
    class PlayerInfoReq : Packet
    {
        public long playerId;

    }

    // Server에서 client로의 답변
    class PlayerInfoOk : Packet
    {
        public int hp;
        public int attack;
    }

    public enum PacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOk = 2,
    }

    class ServerSession : Session
    {
        // 엔진과 컨텐츠 분리
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine("OnConnected : {0}", endPoint);

            PlayerInfoReq packet = new PlayerInfoReq() { size = 4, packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001};

            // for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = SendBufferHelper.Open(4096);

                // s에 집어넣지않고 new byte하고 있어서 좀 비효율적, 다양한 해결방법이 있음
                // 유니티에서 모든 버전을 사용할 수 있는지 확인해야함
                ushort count = 0;
                bool success = true;

                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), packet.size);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.packetId);
                count += 2;
                success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packet.playerId);
                count += 8;

                // Getbyte는 안정성 때문에, 근데 속도가 느림
                byte[] size = BitConverter.GetBytes(packet.size); // 2
                byte[] packetId = BitConverter.GetBytes(packet.packetId); // 2
                byte[] playerId = BitConverter.GetBytes(packet.playerId); // 8

                // 자동화하기 위해서

                Array.Copy(size, 0, s.Array, s.Offset + count, 2);
                count += 2;
                Array.Copy(packetId, 0, s.Array, s.Offset + count, 2);
                count += 2;
                Array.Copy(playerId, 0, s.Array, s.Offset + count, 8);
                count += 8;
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 앞으로 패킷으로 보낼거임, 짤려서 왔으면 실행하면 안될것이다.
        // TCP에서는 100byte를 보낸다고해서 무조건 100byte들 받는다는 보장이 없다.
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numofBytes)
        {
            Console.WriteLine($"Transferred bytes : {numofBytes}");
        }
    }
}

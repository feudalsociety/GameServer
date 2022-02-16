using ServerCore;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DummyClient
{
    public abstract class Packet
    {
        public ushort size;
        public ushort packetId;
        public abstract ArraySegment<byte> Write();
        public abstract void Read(ArraySegment<byte> s);
    }

    // client에서 요청
    class PlayerInfoReq : Packet
    {
        public long playerId;
        public string name;

        public PlayerInfoReq()
        {
            this.packetId = (ushort)PacketID.PlayerInfoReq;
        }

        public override void Read(ArraySegment<byte> segment)
        {
            ushort count = 0;

            ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            count += sizeof(ushort);
            // this.playerId = BitConverter.ToInt64(s.Array, s.Offset + count);
            // 범위를 집어주면서, 만약에 범위를 초과하는 값을 parsing을 하려고하면 자동으로 error
            this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
            count += sizeof(long);

            // string, nameLen이 이상한 값이 들어갔다고 가정
            ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count)); 
            count += sizeof(ushort);
            this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen)); // 문제가 생기면 try catch로 넘어감
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> segment = SendBufferHelper.Open(4096);

            ushort count = 0;
            bool success = true;

            Span<byte> s = new Span<byte>(segment.Array, segment.Offset, segment.Count);

            count += sizeof(ushort);
            // 굳이 packetid를 사용해야할까? (ushort)PacktID.PlayerInfoReq 써도됨. size도 packet 자체에서는 이용하지 않는다.
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.packetId);
            count += sizeof(ushort);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), this.playerId);
            count += sizeof(long);

            // string, c# 끝은 항상 NULL 0x00 00 으로 끝나지 않음, 몇바이트가 와야 성공적으로 조립해야하는지 알기 어려움
            // 2단계로 보냄 string len[2], byte[]
            // ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
            // success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            // count += sizeof(ushort);
            // segment 버퍼에 직접적으로 밀어넣지 않음, 개선할 수 있는 여지
            // Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, segment.Array, count, nameLen);
            // count += nameLen;

            // 몇바이트를 복사했는지 int 형으로 return
            ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, segment.Array, segment.Offset + count + sizeof(ushort));
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
            count += sizeof(ushort);
            count += nameLen;

            // size에 count 값을 복사
            success &= BitConverter.TryWriteBytes(s, count);

            if (success == false) return null;

            return SendBufferHelper.Close(count);
        }
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

            // size는 시작에서는 모르고 마지막에서야 알수 있다.
            PlayerInfoReq packet = new PlayerInfoReq() { playerId = 1001, name = "ABCD" };

            // for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> s = packet.Write();
                if(s != null) Send(s);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 앞으로 패킷으로 보낼거임, 짤려서 왔으면 실행하면 안될 것이다.
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

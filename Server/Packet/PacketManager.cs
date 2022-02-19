using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // PacketHandler를 등록시킨 다음에는 수정할건 없기 때문에 singleton으로 만든다.
    class PacketManager
    {
        #region Singleton
        static PacketManager _instance;
        public static PacketManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PacketManager();
                return _instance;
            }
        }
        #endregion

        Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
        Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();  

        // multithread가 개입되기전에 우선적으로 해줘야함, 나머지 코드는 조회만 하므로 multithread에서 
        public void Register()
        {
            _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
            _handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
        }

        public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
        {
            ushort count = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            count += 2;
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
            count += 2;

            // packet의 종류가 아주 많을 경우 switch문을 이용하는것은 비효율적일 수 있다.
            // swtich 문에서 parsing하는 작업을 자동화, packet의 base class를 만드는 것이 좋겠다.
            //switch ((PacketID)id)
            //{
            //    case PacketID.PlayerInfoReq:
            //        {
            //            PlayerInfoReq p = new PlayerInfoReq();
            //            p.Read(buffer);

            //        }
            //        break;
            //}

            Action<PacketSession, ArraySegment<byte>> action = null;
            if (_onRecv.TryGetValue(id, out action))
                action.Invoke(session, buffer);
        }

        void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
        {
            T pkt = new T();
            pkt.Read(buffer); // deserialize

            Action<PacketSession, IPacket> action = null;
            if (_handler.TryGetValue(pkt.Protocol, out action))
                action.Invoke(session, pkt);
        }
    }
}

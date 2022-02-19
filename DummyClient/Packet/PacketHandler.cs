using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

// 패킷은 한방향으로 쓰고 양방향으로 쓰는 경우는 거의 없을 텐데 
// Client와 Server 둘다 handler 를 가지고 있을 필요는 없을 것이다. 비워놓더라도 선언은 해야됨
// 서버를 분산처리하는 경우 패킷분리를 해야한다. 용도를 잘 구분하여 안전처리함 
// C_ client에서 server쪽으로 보내는 packet, S_ 반대
// PacketManager를 자동화할때 Server쪽에 Register는 C_만 등록, 반대로 Client쪽에는 S_만 등록
class PacketHandler
{
    //public static void PlayerInfoReqHandler(PacketSession session, IPacket packet)
    //{
        
    //}

    public static void S_TestHandler(PacketSession session, IPacket packet)
    {

    }


}


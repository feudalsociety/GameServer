using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

class PacketHandler
{
    // Sever에서 방에 있는 모든 애들에게 S_Chat 답장하는데 이부분을 handling
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        // 다수의 client를 simulation하고 있어서 모든 messeage를 다 출력하면 너무 많아짐
        // if (chatPacket.playerId == 1)
           // Console.WriteLine(chatPacket.chat);
    }
}


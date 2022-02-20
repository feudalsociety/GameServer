using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null) return;

        GameRoom room = clientSession.Room;
        // clientSession에 Room이 null로 바뀌면 lamda가 문제가 될 수 있다.
        // 이전과는 다르게 실행시점이 뒤로 밀림. client 접속이 끊기면 room을 찾지 못해 crash
        //clientSession.Room.Push(() => clientSession.Room.Broadcast(clientSession, chatPacket.chat));
        // clientSession.Room.Broadcast(clientSession, chatPacket.chat);
        room.Push(() => room.Broadcast(clientSession, chatPacket.chat));

    }
}


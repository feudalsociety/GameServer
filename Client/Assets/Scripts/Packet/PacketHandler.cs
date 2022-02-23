using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PacketHandler
{
    // Sever에서 방에 있는 모든 애들에게 S_Chat 답장하는데 이부분을 handling
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        // dummyclient가 보낸 message가 server를 통해서 unity client쪽으로 출력
        // if (chatPacket.playerId == 1)
        {
            Debug.Log(chatPacket.chat);

            GameObject go = GameObject.Find("Payer");
            if (go == null)
                Debug.Log("Player found");
            else
                Debug.Log("Player not found");
        }
    }
}
    

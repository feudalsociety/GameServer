using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    // Unity 같은 경우에는 Session을 하나만 들고 있으면 되겠다.
    ServerSession _session = new ServerSession();

    public void Send(ArraySegment<byte> sendBuff)
    {
        _session.Send(sendBuff);
    }

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        // client 몇개를 simulate 할건지 조절
        connector.Connect(endPoint, () => { return _session; }, 1);
    }

    void Update()
    {
        // 한 프레임동안 packet 하나 처리
        //IPacket packet = PacketQueue.Instance.Pop();
        //if(packet != null)
        //{
        //    PacketManager.Instance.HandlePacket(_session, packet);
        //}

        // 해당 프레임에 들어온 모든 packet을 다 꺼내서 실행
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list)
            PacketManager.Instance.HandlePacket(_session, packet);
    }

    
}

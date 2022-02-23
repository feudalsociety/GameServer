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

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        // client 몇개를 simulate 할건지 조절
        connector.Connect(endPoint, () => { return _session; }, 1);

        StartCoroutine("CoSendPacket");
    }
    // unity를 구동하고 있는 main thread에서 network packet을 실행하는것이 아니라 thread pool에서 하나꺼내와서 실행하고 있어서 문제됨
    // unity에서는 다른 thread에서 실행하는것을 원천적으로 차단함
    // 따라서 handler를 main thread에서 실행하게끔 조작, handler에서 바로 처리하지 않고 queue에 넣고 unity main thread에서 꺼내서 처리

    void Update()
    {
        // 한 프레임동안 packet 하나 처리
        IPacket packet = PacketQueue.Instance.Pop();
        if(packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    IEnumerator CoSendPacket()
    {
        while(true)
        {
            yield return new WaitForSeconds(3.0f);
            C_Chat chatPacket = new C_Chat();
            chatPacket.chat = "Hello Unity!";
            ArraySegment<byte> segment = chatPacket.Write();

            _session.Send(segment);
        }
    }
}

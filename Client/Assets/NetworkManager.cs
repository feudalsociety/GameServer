using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    // Unity ���� ��쿡�� Session�� �ϳ��� ��� ������ �ǰڴ�.
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
        // client ��� simulate �Ұ��� ����
        connector.Connect(endPoint, () => { return _session; }, 1);
    }

    void Update()
    {
        // �� �����ӵ��� packet �ϳ� ó��
        //IPacket packet = PacketQueue.Instance.Pop();
        //if(packet != null)
        //{
        //    PacketManager.Instance.HandlePacket(_session, packet);
        //}

        // �ش� �����ӿ� ���� ��� packet�� �� ������ ����
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach (IPacket packet in list)
            PacketManager.Instance.HandlePacket(_session, packet);
    }

    
}

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

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();
        // client ��� simulate �Ұ��� ����
        connector.Connect(endPoint, () => { return _session; }, 1);

        StartCoroutine("CoSendPacket");
    }
    // unity�� �����ϰ� �ִ� main thread���� network packet�� �����ϴ°��� �ƴ϶� thread pool���� �ϳ������ͼ� �����ϰ� �־ ������
    // unity������ �ٸ� thread���� �����ϴ°��� ��õ������ ������
    // ���� handler�� main thread���� �����ϰԲ� ����, handler���� �ٷ� ó������ �ʰ� queue�� �ְ� unity main thread���� ������ ó��

    void Update()
    {
        // �� �����ӵ��� packet �ϳ� ó��
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

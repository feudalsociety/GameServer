using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> _packetQueue = new Queue<IPacket>();
    object _lock = new object();

    // handler 쪽에서 밀어넣는 작업을 해줄수 있지만 꺼내올때 또 packet id 판별해서 또 함수 호출해줘야될텐데 낭비가될것
    public void Push(IPacket packet)
    {
        lock(_lock)
        {
            _packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock(_lock)
        {
            if (_packetQueue.Count == 0) return null;
            return _packetQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        List<IPacket> list = new List<IPacket>();
        lock (_lock)
        {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }

        return list;
    }
}

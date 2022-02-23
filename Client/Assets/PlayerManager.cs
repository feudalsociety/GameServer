using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; }= new PlayerManager();

    // S_PlayerList�� ���� packet�� PlayerManager���� handle
    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach (S_PlayerList.Player p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            // playerId�� ���� SessionManager���� �߱��� sessionid�� ����.
            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void Move(S_BroadcastMove packet)
    {
        // ���ڽ��� �̵��ϴ��� ����, �̵�����ȭ�� ��� ���� ����� �κ��� �ϳ�
        // 1. Server���� ��� packet�� ���� �� �������� �̵��ϴ� ��� (������ �� ����� ����)
        // 2. Player�� �̵���Ű�� Client���� �����̵����ѳ��� �������� ������ ������ �װſ� ���� �����ϴ� ���
        if (_myPlayer.PlayerId == packet.playerId)
        {
            _myPlayer.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        }
        else
        {
            Player player = null;
            if(_players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
            }
        }
    }

    // ���� �̹� ������ ���¿��� ������ ���� ������ �ߴٴ°�
    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (packet.playerId == _myPlayer.PlayerId) return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        _players.Add(packet.playerId, player);
    }

    public void LeaveGame(S_BroadcastLeaveGame packet)
    {
        // ���� ����� �ڽ��� ���� �����ϱ� ����
        if(_myPlayer.PlayerId == packet.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(packet.playerId);
            }

        }
    }


}

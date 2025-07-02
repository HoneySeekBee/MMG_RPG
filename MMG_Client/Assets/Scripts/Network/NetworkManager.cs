using Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    private ClientSession _session;
    public const string MMG_API_URL = "http://localhost:5070";
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void GetClientSession(ClientSession mySession)
    {
        _session = mySession;
    }
    public void Send_Login(C_LoginToken packet)
    {
        _session.Send(ServerCore.PacketType.C_LoginToken, packet);
    }
    public void Send_CharacterInfo(C_SelectedCharacter packet)
    {
        _session.Send(ServerCore.PacketType.C_SelectCharacter, packet);
    }

    // [1] 캐릭터 생성 요청
    public void Send_EnterGame(C_EnterGameRequest packet)
    {
        _session.Send(ServerCore.PacketType.C_EnterGameRequest, packet);
    }

    // [2] GameRoom 입장 요청

    public void Send_Move(Vector3 pos, float dirY, float speed)
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan elapsed = utcNow - epoch;

        long unixTimeSeconds = (long)elapsed.TotalSeconds;

        C_BroadcastMove packet = new C_BroadcastMove()
        {
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,

            DirY = dirY,
            Speed = speed,

            Timestamp = unixTimeSeconds
        };
        _session.Send(ServerCore.PacketType.C_BroadcastMove, packet);
    }
}

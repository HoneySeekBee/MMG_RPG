using Packet;
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
    public void Send_LoginCheck(C_LoginCheck packet)
    {
        _session.Send(ServerCore.PacketType.C_LoginCheck, packet);
    }
    public void Send_Login(C_LoginRequest packet)
    {
        _session.Send(ServerCore.PacketType.C_LoginRequest, packet);
    }
    public void Send_CharacterList(C_PlayerCharactersRequest packet)
    {
        _session.Send(ServerCore.PacketType.C_UserCharacterListRequest, packet);
    }
}

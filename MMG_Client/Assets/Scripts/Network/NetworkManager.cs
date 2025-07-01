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
    public void Send_Login(C_LoginToken packet)
    {
        _session.Send(ServerCore.PacketType.C_LoginToken, packet);
    }
    public void Send_CharacterInfo(C_SelectedCharacter packet)
    {
        _session.Send(ServerCore.PacketType.C_SelectCharacter, packet);
    }
    public void Send_Move(C_Move packet)
    {
        _session.Send(ServerCore.PacketType.C_Move, packet);
    }
}

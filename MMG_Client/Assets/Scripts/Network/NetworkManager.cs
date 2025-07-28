using Packet;
using GamePacket;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using AttackPacket;
using ServerCore;
using ChatPacket;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

public class NetworkManager : GlobalSingleton<NetworkManager>
{
    private ClientSession _session;
    private ChatSession _chatSession;
    public static string MMG_API_URL { get { return $"https://localhost:{API_PORT_NUMBER}"; } }

    public static int MAIN_PORT_NUMBER { get { return GetPortNumber("Main"); } }
    public static int CHAT_PORT_NUMBER { get { return GetPortNumber("Chat"); } }
    public static int API_PORT_NUMBER { get { return GetPortNumber("API"); } }
    public void GetClientSession(ClientSession mySession)
    {
        _session = mySession;
    }
    public void GetClientSession(ChatSession mySession)
    {
        _chatSession = mySession;
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
            BroadcastMove = new GamePacket.MoveData()
            {
                PosX = pos.x,
                PosY = pos.y,
                PosZ = pos.z,

                DirY = dirY,
                Speed = speed,

                Timestamp = unixTimeSeconds
            }
        };
        _session.Send(ServerCore.PacketType.C_BroadcastMove, packet);
    }

    // [3] 캐릭터 공격 
    public void Send_Attack(C_AttackData packet)
    {
        Debug.Log($"Send AttackData: {packet.ToString()}");

        Debug.Log((ushort)PacketType.C_AttackData);
        _session.Send(ServerCore.PacketType.C_AttackData, packet);
    }

    public void Send_PlayerReviveRequest(PlayerId packet)
    {
        _session.Send(PacketType.C_PlayerReviveRequest, packet);
    }
    #region ChatServer Packet
    public void Send_Chat_EnterGame()
    {
        // 패킷을 만들자. 
        // 패킷 타입을 만들자. 
        EnterChatRoom enterChatRoom = new EnterChatRoom()
        {
            RoomId = MapManager.Instance.MapNumber,
            UserId = PlayerData.Instance.MyCharacterInfo().UserId,
            CharacterId = PlayerData.Instance.MyCharacterInfo().Id,
            NickName = PlayerData.Instance.MyCharacterInfo().CharacterName,
        };
        _chatSession.Send(PacketType.C_EnterChatRoom, enterChatRoom);
    }
    public void Send_RoomChat(string chatText)
    {
        Debug.Log($"[NetworkManager] : Chat {chatText}");
        C_RoomChat chat = new C_RoomChat()
        {
            Message = chatText
        };
        _chatSession.Send(PacketType.C_RoomChat, chat);
    }
    #endregion
    private static int GetPortNumber(string serverName)
    {
        string configPath = @"C:\Users\USER\OneDrive\바탕 화면\MMG\MMG_RPG\MMG_RPG\ServerConfig.json";

        var json = File.ReadAllText(configPath);
        var configs = JsonConvert.DeserializeObject<List<ServerConfig>>(json);
        var myConfig = configs?.FirstOrDefault(c => c.ServerName == serverName);
        if (myConfig == null)
        {
            Console.WriteLine($"[ERROR] {serverName} 설정을 찾을 수 없습니다.");
            return 7132;
        }

        return myConfig.PortNumber;
    }

    public class ServerConfig
    {
        public string ServerName { get; set; }
        public string ExePath { get; set; }
        public int PortNumber { get; set; }
    }
}
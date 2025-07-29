using Google.Protobuf;
using Packet;
using GamePacket;
using MonsterPacket;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackPacket;
using ChatPacket;

public class PacketManager : MonoBehaviour
{
    //private static Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>> _onRecv
    //    = new Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>>();
    private static Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv
    = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    public static void Register()
    {
        Debug.Log("Pakcet Manager - Register");
        RegisterLogin();
        RegisterEnterGame();
        RegisterBroadcast();
        RegisterChat();
    }
    #region
    private static void RegisterLogin()
    {
        _onRecv.Add((ushort)PacketType.S_LoginToken, MakePacket<ClientSession, S_LoginToken>(PacketHandler.S_LoginTokenHandler));
        _onRecv.Add((ushort)PacketType.S_SelectCharacter, MakePacket<ClientSession, S_SelectedCharacter>(PacketHandler.S_SelectedCharacterHandler));
    }
    private static void RegisterEnterGame()
    {
        _onRecv.Add((ushort)PacketType.S_EnterGameResponse, MakePacket<ClientSession, S_EnterGameResponse >(PacketHandler.S_EnterGameResponHandler));
        _onRecv.Add((ushort)PacketType.S_MonsterList, MakePacket<ClientSession, S_MonsterList>(PacketHandler.S_MonsterListHandler));
    }
    private static void RegisterBroadcast()
    {
        _onRecv.Add((ushort)PacketType.S_BroadcastEnter, MakePacket<ClientSession, S_BroadcastEnter>(PacketHandler.S_BroadcastEnterHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastMove, MakePacket<ClientSession, S_BroadcastMove>(PacketHandler.S_BroadcastMovehandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastMonstermove, MakePacket<ClientSession, S_BroadcastMove>(PacketHandler.S_BroadcastMonsterMovehandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastDamage, MakePacket<ClientSession, S_DamageBroadcast>(PacketHandler.S_BroadcastDamageHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastAttack, MakePacket<ClientSession, S_Attack>(PacketHandler.S_BroadcastAttackHandler));

        _onRecv.Add((ushort)PacketType.S_BroadcastDead, MakePacket<ClientSession, S_DeathBroadcast>(PacketHandler.S_DeadHandler));
        _onRecv.Add((ushort)PacketType.S_RespawnMonsterList, MakePacket<ClientSession, S_MonsterList>(PacketHandler.S_MonsterListHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastPlayerDie, MakePacket<ClientSession, PlayerId>(PacketHandler.S_BroadcastPlayerDieHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastPlayerRespawn, MakePacket<ClientSession, S_PlayerRespawn>(PacketHandler.S_BroadcastPlayerReviveHandler));

        _onRecv.Add((ushort)PacketType.S_UpdateStatus, MakePacket<ClientSession, Status>(PacketHandler.S_StatusUpdate));
        _onRecv.Add((ushort)PacketType.S_BroadcastLevelUp, MakePacket<ClientSession, S_BroadcastLevelUp>(PacketHandler.S_BroadcastLevelUp));

    }
    public static void RegisterChat()
    {
        _onRecv.Add((ushort)PacketType.S_EnterChatRoom, MakePacket<ChatSession, EnterChatRoom>(PacketHandler.S_Chat_EnterChatRoomHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastRoomChat, MakePacket<ChatSession, S_BroadcastRoomChat>(PacketHandler.S_Chat_RoomChat));
        _onRecv.Add((ushort)PacketType.S_SystemChat, MakePacket<ChatSession, S_BroadcastRoomChat>(PacketHandler.S_Chat_SystemChat));
        _onRecv.Add((ushort)PacketType.S_AdminChat, MakePacket<ChatSession, S_BroadcastRoomChat>(PacketHandler.S_Chat_AdminChat));
        
    }
    #endregion
    public static void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
        int bodySize = size - 2;

        ArraySegment<byte> bodySegment = new ArraySegment<byte>(
            buffer.Array, buffer.Offset + 4, bodySize);

        if (_onRecv.TryGetValue(id, out var action))
            action.Invoke(session, bodySegment);
    }
    private static Action<PacketSession, ArraySegment<byte>> MakePacket<TSession, T>(Action<TSession, T> handler)
    where TSession : PacketSession
    where T : Google.Protobuf.IMessage<T>, new()
    {
        return (session, buffer) =>
        {
            try
            {
                T pkt = new T();
                pkt.MergeFrom(buffer);
                handler.Invoke((TSession)session, pkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PacketManager Merge Error] {typeof(T).Name} → {ex.Message}");
            }
        };
    }
    //public static void OnRecvPacket(ClientSession session, ArraySegment<byte> buffer)
    //{
    //    ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset); // [0~1]
    //    ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // [2~3]
    //    int bodySize = size - 2; // 전체 크기에서 ID(2바이트) 빼기

    //    ArraySegment<byte> bodySegment = new ArraySegment<byte>(
    //        buffer.Array, buffer.Offset + 4, bodySize); // [4 ~ 4+bodySize]

    //    Console.WriteLine($"[PacketManager] PacketType: {id}, BodySize: {size - 2}");

    //    if (_onRecv.TryGetValue(id, out var action))
    //        action.Invoke(session, bodySegment);
    //}

    //private static Action<ClientSession, ArraySegment<byte>> MakePacket<T>(Action<ClientSession, T> handler)
    //    where T : IMessage<T>, new()
    //{
    //    return (session, buffer) =>
    //    {
    //        try
    //        {
    //            T pkt = new T();
    //            pkt.MergeFrom(buffer);
    //            handler.Invoke(session, pkt);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"[Client PacketManager Merge Error] {typeof(T).Name} → {ex.Message}");
    //        }
    //    };
    //}
}

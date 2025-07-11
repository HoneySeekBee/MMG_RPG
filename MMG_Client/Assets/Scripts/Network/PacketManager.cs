using Google.Protobuf;
using Packet;
using GamePacket;
using MonsterPacket;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketManager : MonoBehaviour
{
    private static Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>> _onRecv
        = new Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>>();

    public static void Register()
    {
        Debug.Log("Pakcet Manager - Register");
        RegisterLogin();
        RegisterEnterGame();
        RegisterBroadcast();
    }
    #region
    private static void RegisterLogin()
    {
        _onRecv.Add((ushort)PacketType.S_LoginToken, MakePacket<S_LoginToken>(PacketHandler.S_LoginTokenHandler));
        _onRecv.Add((ushort)PacketType.S_SelectCharacter, MakePacket<S_SelectedCharacter>(PacketHandler.S_SelectedCharacterHandler));
    }
    private static void RegisterEnterGame()
    {
        _onRecv.Add((ushort)PacketType.S_EnterGameResponse, MakePacket<S_EnterGameResponse>(PacketHandler.S_EnterGameResponHandler));
        _onRecv.Add((ushort)PacketType.S_MonsterList, MakePacket<S_MonsterList>(PacketHandler.S_MonsterListHandler));
    }
    private static void RegisterBroadcast()
    {
        _onRecv.Add((ushort)PacketType.S_BroadcastEnter, MakePacket<S_BroadcastEnter>(PacketHandler.S_BroadcastEnterHandler));
        _onRecv.Add((ushort)PacketType.S_BroadcastMove, MakePacket<S_BroadcastMove>(PacketHandler.S_BroadcastMovehandler));
        _onRecv.Add((ushort)PacketType.S_DamagekResponse, MakePacket<S_DamageBroadcast>(PacketHandler.S_BroadcastDamageHandler));

    }
    #endregion

    public static void OnRecvPacket(ClientSession session, ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset); // [0~1]
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // [2~3]
        int bodySize = size - 2; // 전체 크기에서 ID(2바이트) 빼기

        ArraySegment<byte> bodySegment = new ArraySegment<byte>(
            buffer.Array, buffer.Offset + 4, bodySize); // [4 ~ 4+bodySize]

        Console.WriteLine($"[PacketManager] PacketType: {id}, BodySize: {size - 2}");

        if (_onRecv.TryGetValue(id, out var action))
            action.Invoke(session, bodySegment);
    }

    private static Action<ClientSession, ArraySegment<byte>> MakePacket<T>(Action<ClientSession, T> handler)
        where T : IMessage<T>, new()
    {
        return (session, buffer) =>
        {
            try
            {
                T pkt = new T();
                pkt.MergeFrom(buffer);
                handler.Invoke(session, pkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client PacketManager Merge Error] {typeof(T).Name} → {ex.Message}");
            }
        };
    }
}

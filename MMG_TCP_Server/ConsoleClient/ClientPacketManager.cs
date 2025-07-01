﻿using Google.Protobuf;
using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class ClientPacketManager
    {
        private static Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>> _onRecv
            = new Dictionary<ushort, Action<ClientSession, ArraySegment<byte>>>();

        public static void Register()
        {
            RegisterError();
        }
        #region Register 함수 

        private static void RegisterError()
        {
            _onRecv.Add((ushort)PacketType.S_Error, MakePacket<S_Error>(PacketHandler.S_ErrorHandler));
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
}

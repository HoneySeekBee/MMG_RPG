using Google.Protobuf;
using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameServer.Core
{
    public class ServerPacketManager
    {
        private static Dictionary<ushort, Action<ServerSession, ArraySegment<byte>>> _onRecv =
            new Dictionary<ushort, Action<ServerSession, ArraySegment<byte>>>();

        #region Register 구현 함수 
        public static void Register()
        {
            Register_Set();
            Register_GameRoom();
            Register_Move();
        }
        private static void Register_Set()
        {

            _onRecv.Add((ushort)PacketType.C_LoginToken, MakePacket<C_LoginToken>(PacketHandler.C_LoginTokenHandler));
            _onRecv.Add((ushort)PacketType.C_SelectCharacter, MakePacket<C_SelectedCharacter>(PacketHandler.C_SelectedCharacter));

        }
        private static void Register_GameRoom()
        {
            _onRecv.Add((ushort)PacketType.C_EnterGameRoom, MakePacket<C_EnterGameRoom>(PacketHandler.C_EnterGameRoom));
        }
        private static void Register_Move()
        {
            _onRecv.Add((ushort)PacketType.C_Move, MakePacket<C_Move>(PacketHandler.C_MoveHandler));
        }
        #endregion


        public static void OnRecvPacket(ServerSession session, ArraySegment<byte> buffer)
        {
            if (buffer.Array == null || buffer.Count < 4)
            {
                Console.WriteLine("[PacketManager] 잘못된 패킷 수신");
                return;
            }

            try
            {
                ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset); // [0~1]
                ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2); // [2~3]

                if (!_onRecv.TryGetValue(id, out var action))
                {
                    Console.WriteLine($"[PacketManager] 등록되지 않은 패킷 ID: {id}");
                    return;
                }
                int bodySize = size - 2;
                ArraySegment<byte> bodySegment = new ArraySegment<byte>(
                    buffer.Array, buffer.Offset + 4, bodySize);

                action.Invoke(session, bodySegment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PacketManager Error] {ex.Message}");
            }

        }

        private static Action<ServerSession, ArraySegment<byte>> MakePacket<T>(Action<ServerSession, T> handler)
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
                    Console.WriteLine($"[Server PacketManager Merge Error] {typeof(T).Name} → {ex.Message}");
                }
            };
        }
    }
}

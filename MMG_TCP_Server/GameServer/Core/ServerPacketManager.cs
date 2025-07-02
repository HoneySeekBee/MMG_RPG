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
        private static Dictionary<ushort, Func<ServerSession, ArraySegment<byte>, Task>> _asyncHandlers = new();


        private static Dictionary<ushort, Action<ServerSession, ArraySegment<byte>>> _syncHandlers =
            new Dictionary<ushort, Action<ServerSession, ArraySegment<byte>>>();

        #region Register 구현 함수 
        public static void Register()
        {
            Register_Set();
            Register_Game();
            Register_Move();
        }
        private static void Register_Set()
        {
            _syncHandlers.Add((ushort)PacketType.C_LoginToken, MakePacket<C_LoginToken>(PacketHandler.C_LoginTokenHandler));
            _syncHandlers.Add((ushort)PacketType.C_SelectCharacter, MakePacket<C_SelectedCharacter>(PacketHandler.C_SelectedCharacter));

        }
        private static void Register_Game()
        {
            _asyncHandlers.Add((ushort)PacketType.C_EnterGameRequest, MakeAsyncPacket<C_EnterGameRequest>(PacketHandler.C_EnterGameHandler));
        }


        private static void Register_Move()
        {
            _syncHandlers.Add((ushort)PacketType.C_BroadcastMove, MakePacket<C_BroadcastMove>(PacketHandler.C_MoveHandler));
        }
        #endregion


        public static async Task OnRecvPacket(ServerSession session, ArraySegment<byte> buffer)
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


                int bodySize = size - 2;
                ArraySegment<byte> bodySegment = new ArraySegment<byte>(
                    buffer.Array, buffer.Offset + 4, bodySize);

                if (_syncHandlers.TryGetValue(id, out var syncHandler))
                {
                    syncHandler.Invoke(session, bodySegment);
                }
                else if (_asyncHandlers.TryGetValue(id, out var asyncHandler))
                {
                    await asyncHandler.Invoke(session, bodySegment);
                }
                else
                {
                    Console.WriteLine($"[PacketManager] 등록되지 않은 패킷 ID: {id}");
                }

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
                    Console.WriteLine($"[Merge Error: {typeof(T).Name}] {ex.Message}");
                }
            };
        }
        private static Func<ServerSession, ArraySegment<byte>, Task> MakeAsyncPacket<T>(Func<ServerSession, T, Task> handler)
    where T : IMessage<T>, new()
        {
            return async (session, buffer) =>
            {
                try
                {
                    T pkt = new T();
                    pkt.MergeFrom(buffer);
                    await handler(session, pkt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Async Merge Error: {typeof(T).Name}] {ex.Message}");
                }
            };
        }
    }
}

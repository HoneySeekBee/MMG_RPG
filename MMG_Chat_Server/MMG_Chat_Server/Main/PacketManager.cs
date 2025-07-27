using ChatPacket;
using Google.Protobuf;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMG_Chat_Server.Main
{
    public class PacketManager
    {
        private static Dictionary<ushort, Action<ChatSession, ArraySegment<byte>>> _syncHandlers =
            new Dictionary<ushort, Action<ChatSession, ArraySegment<byte>>>();


        public static void Register()
        {
            Register_ChatRoom();
            Register_Chat();
        }

        public static void Register_ChatRoom()
        {
            _syncHandlers.Add((ushort)PacketType.C_EnterChatRoom, MakePacket<EnterChatRoom>(PacketHandler.C_EnterChatRoom));
        }
        public static void Register_Chat()
        {
            _syncHandlers.Add((ushort)PacketType.C_RoomChat, MakePacket<C_RoomChat>(PacketHandler.C_RoomChat));
        }

        public static async Task OnRecvPacket(ChatSession session, ArraySegment<byte> buffer)
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
        private static Action<ChatSession, ArraySegment<byte>> MakePacket<T>(Action<ChatSession, T> handler)
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

    }
}

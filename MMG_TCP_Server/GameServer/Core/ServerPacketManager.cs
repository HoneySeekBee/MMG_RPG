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

        public static void Register()
        {

            RegisterLogin();
            RegisterGame();
            RegisterPing();
            RegisterMove();
            RegisterVillageDataSave();
            RegisterVillageDataLoad();
        }
        #region Register 구현 함수 
        private static void RegisterLogin()
        {
            _onRecv.Add((ushort)PacketType.C_LoginRequest, MakePacket<C_LoginRequest>(PacketHandler.C_LoginRequestHandler));
            _onRecv.Add((ushort)PacketType.C_LoginCheck, MakePacket<C_LoginCheck>(PacketHandler.C_LoginCheckHandler));
        }
        private static void RegisterGame()
        {
            _onRecv.Add((ushort)PacketType.C_EnterGame, MakePacket<C_EnterGame>(PacketHandler.C_EnterGameHandler));
            _onRecv.Add((ushort)PacketType.C_LeaveGame, MakePacket<C_LeaveGame>(PacketHandler.C_LeaveGameHandler));
        }
        private static void RegisterPing()
        {
            _onRecv.Add((ushort)PacketType.C_Ping, MakePacket<C_Ping>(PacketHandler.C_PingHandler));
        }
        private static void RegisterMove()
        {
            _onRecv.Add((ushort)PacketType.C_Move, MakePacket<C_Move>(PacketHandler.C_MoveHandler));
        }
        private static void RegisterVillageDataSave()
        {
            _onRecv.Add((ushort)PacketType.C_SaveVillageData, MakePacket<C_SaveVillageData>(PacketHandler.C_SaveVillageDataHandler));
        }
        private static void RegisterVillageDataLoad()
        {
            _onRecv.Add((ushort)PacketType.C_LoadVillageDataRequest, MakePacket<C_LoadVillageDataRequest>(PacketHandler.C_LoadVillageDataHandler));
            _onRecv.Add((ushort)PacketType.C_DestroyPlantedCrop, MakePacket<C_DestroyPlantedCrop>(PacketHandler.C_DestroyPlantedCrop));
            _onRecv.Add((ushort)PacketType.C_SavePlantedCrop, MakePacket<C_SavePlantedData>(PacketHandler.C_SavePlantedCropHandler));
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

using GameServer.Data;
using GameServer.Domain;
using Microsoft.IdentityModel.Tokens;
using Packet;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static GameServer.Data.LoginDTO;

namespace GameServer.Core
{
    public static class IdGenerator
    {
        private static int _playerId = 1;

        public static int GeneratePlayerId()
        {
            return _playerId++;
        }
    }
    public class PacketHandler
    {
        #region Set
        public static void C_LoginTokenHandler(ServerSession session, C_LoginToken packet)
        {
            Console.WriteLine($"[C_LoginTokenHandler] 토큰 검사하는 로직 추가 ");
            S_LoginToken LoginToken = new S_LoginToken()
            {
                Result = true
            };
            session.Send(PacketType.S_LoginToken, LoginToken);
        }
        public static void C_SelectedCharacter(ServerSession session, C_SelectedCharacter packet)
        {
            Console.WriteLine($"캐릭터 선택완료 {packet.CharacterInfo.CharacterName}");

            S_SelectedCharacter SelectedCharacter = new S_SelectedCharacter()
            {
                Result = true
            };
            session.Send(PacketType.S_SelectCharacter, SelectedCharacter);
        }
        #endregion
        #region GameRoom
        public static void C_EnterGameRoom(ServerSession session, C_EnterGameRoom packet)
        {
            Player player = session.MyPlayer;
            // [1] 이전 방에서 퇴장 처리

            ExitGameRoom(player);

            // [2] 새 방 찾기 또는 생성
            GameRoom nextRoom = GameRoomManager.Instance.GetRoom(packet.EnterRoomId)
                                ?? GameRoomManager.Instance.CreateRoom(packet.EnterRoomId);

            // [3] 새 방으로 입장
            player.CurrentRoomId = packet.EnterRoomId;
            nextRoom.Push(() => nextRoom.Enter(session, packet));
        }
        #endregion
        #region Game 입력 
        public static void C_MoveHandler(ServerSession session, C_Move packet)
        {
            Player player = session.MyPlayer;

            if (packet.Timestamp < player.LastMoveTimestamp)
            {
                Console.WriteLine($"[Move Reject] 이전보다 오래된 이동 요청입니다. Timestamp: {packet.Timestamp}");
                return; // 오래된 패킷 무시
            }

            player.LastMoveTimestamp = packet.Timestamp;

            S_Move s_Move = new S_Move()
            {
                PlayerId = session.SessionId,
                PosX = packet.PosX,
                PosY = packet.PosY,
                PosZ = packet.PosZ,
                DirX = packet.DirX,
                DirY = packet.DirY,
                DirZ = packet.DirZ,
                Speed = packet.Speed,
                Timestamp = packet.Timestamp,
            };
            // 브로드 캐스트 해주어야함
            SafeBroadcastMove(session, s_Move);
        }
        public static void SafeBroadcastMove(ServerSession session, S_Move s_Move)
        {
            var player = session.MyPlayer;
            int? roomId = player.CurrentRoomId;

            if (roomId == null)
            {
                Console.WriteLine($"[SafeBroadcastMove] 유저 {player.UserId}는 방에 속해 있지 않음");
                return;
            }

            GameRoom room = GameRoomManager.Instance.GetRoom(roomId.Value);

            if (room == null)
            {
                Console.WriteLine($"[SafeBroadcastMove] 방 ID {roomId.Value} 존재하지 않음");
                return;
            }

            room.Push(() => room.BroadcastMove(s_Move, exclude: player));
        }
        #endregion


        private static void ExitGameRoom(Player player)
        {
            // [1] 이전 방에서 퇴장 처리

            if (player.CurrentRoomId.HasValue)
            {
                int prevRoomId = player.CurrentRoomId.Value;
                GameRoom prevRoom = GameRoomManager.Instance.GetRoom(prevRoomId);

                // 그 방이 아직 존재하고, 해당 플레이어가 그 방에 실제로 속해있을 때만 퇴장 처리
                if (prevRoom != null && prevRoom.HasPlayer(player))
                {
                    prevRoom.Push(() => prevRoom.Leave(player));
                }
                else
                {
                    Console.WriteLine($"[경고] player.CurrentRoomId={prevRoomId}인데 실제 방에 없음 → 무시");
                }
            }
        }


    }
}

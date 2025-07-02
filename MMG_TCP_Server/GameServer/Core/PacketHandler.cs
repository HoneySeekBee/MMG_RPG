using GameServer.Domain;
using GameServer.GameRoomFolder;
using Newtonsoft.Json;
using Packet;
using ServerCore;
using System.Net.Http.Headers;
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
            session.jwtToken = packet.JwtToken;
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
        public static async Task C_EnterGameHandler(ServerSession session, C_EnterGameRequest packet)
        {
            if (string.IsNullOrEmpty(session.jwtToken))
            {
                Console.WriteLine("[EnterGameHandler] JWT 토큰 없음");
                return;
            }
            // 캐릭터 아이디와 맵 번호 

            // 1. API 서버에서 캐릭터 정보 가져오기
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", session.jwtToken);

            HttpResponseMessage res;
            try
            {
                res = await client.GetAsync($"https://localhost:7132/api/character/{packet.CharacterId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnterGameHandler] HTTP 요청 실패: {ex.Message}");
                return;
            }

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"[EnterGameHandler] 캐릭터 조회 실패: {res.StatusCode}");
                return;
            }
            var json = await res.Content.ReadAsStringAsync();
            CharacterInfo character = JsonConvert.DeserializeObject<CharacterInfo>(json);


            Console.WriteLine($"Myplayer가 null인가요? {session.MyPlayer == null}");
            if(session.MyPlayer == null)
            {
                session.MyPlayer = new Player()
                {
                    Session = session,
                };
            }
            session.MyPlayer.CharacterInfo = character;
            // 2. GameRoom 찾기 및 입장
            GameRoom room = GameRoomManager.Instance.GetOrCreateRoom(packet.MapId);

            room.Enter(session);
        }
        
        #region Game 입력 
        public static void C_BroadcastMoveHandler(ServerSession session, C_BroadcastMove packet)
        {
            Player player = session.MyPlayer;

            if (packet.Timestamp < player.LastMoveTimestamp)
            {
                Console.WriteLine($"[Move Reject] 이전보다 오래된 이동 요청입니다. Timestamp: {packet.Timestamp}");
                return; // 오래된 패킷 무시
            }

            player.LastMoveTimestamp = packet.Timestamp;

            S_BroadcastMove s_Move = new S_BroadcastMove()
            {
                PlayerId = session.SessionId,
                CharacterId = session.MyPlayer.CharacterInfo.Id,
                PosX = packet.PosX,
                PosY = packet.PosY,
                PosZ = packet.PosZ,
                DirY = packet.DirY,
                Speed = packet.Speed,
                Timestamp = packet.Timestamp,
            };
            // 브로드 캐스트 해주어야함

            SafeBroadcastMove(session, s_Move);
        }
        public static void SafeBroadcastMove(ServerSession session, S_BroadcastMove s_Move)
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

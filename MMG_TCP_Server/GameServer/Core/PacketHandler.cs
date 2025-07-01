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
        // 로그인 로직 수정 
        public static async void C_LoginCheckHandler(ServerSession session, C_LoginCheck packet)
        {
            string token = packet.JwtToken;
            Console.WriteLine($"[JWT 검사] 클라이언트에서 전송한 JWT: {token}");

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync($"http://localhost:5070/api/auth/validate?token={token}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[JWT 검사 실패] 상태코드: {response.StatusCode}");
                    session.Send(PacketType.S_LoginCheckResponse, new S_LoginCheckResponse
                    {
                        IsValid = false,
                        Reason = "invalid"
                    });
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JwtValidationResult>(content);

                if (result == null || !result.IsValid)
                {
                    session.Send(PacketType.S_LoginCheckResponse, new S_LoginCheckResponse
                    {
                        IsValid = false,
                        Reason = result?.Reason ?? "invalid"
                    });
                    return;
                }

                Console.WriteLine($"[JWT 검사 성공] userId: {result.UserId}, email: {result.Email}");

                session.Send(PacketType.S_LoginCheckResponse, new S_LoginCheckResponse
                {
                    IsValid = true,
                    UserId = result.UserId,
                    Email = result.Email,
                    Nickname = result.Nickname
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[예외 발생] JWT 검사 중 오류: {ex.Message}");
                session.Send(PacketType.S_LoginCheckResponse, new S_LoginCheckResponse
                {
                    IsValid = false,
                    Reason = "exception"
                });
            }
        }
        public static void C_LoginRequestHandler(ServerSession session, C_LoginRequest packet)
        {
            // [1] 클라에서 로그인 요청이 왔습니다. 

            // 이것도 암호처리를 해서 확인해야할듯 싶습니다. 
            Console.WriteLine($"[Login] {packet.UserId} / {packet.Password}");

            // 서버는 클라가 보낸 로그인 정보를 체크합니다. 
            var response = new S_LoginResponse();
            if (session.IsLoggedIn)
            {
                response.Success = false;
                response.Message = "이미 로그인 된 세션입니다. ";
                session.Send(PacketType.S_LoginResponse, response);
                return;
            }

            if (SessionManager.IsLoggedIn(packet.UserId))
            {
                ServerPacketUtils.SendError(session, ErrorCode.AlreadyLoggedIn, $"{packet.UserId}는 이미 로그인된 유저입니다.");
                return;
            }

            (bool success, User thisUser) = EFUserDB.TryLogin(packet.UserId, packet.Password);

            response.Success = success;
            response.Message = success ? $"Welcome, {packet.UserId}!" : "Login failed";
            if (success)
            {
                session.MyPlayer = new Player()
                {
                    Name = packet.UserId,
                    PlayerId = thisUser.Id,
                    Session = session
                };
                session.SetLoginStatus(true);
                Console.WriteLine($"LoginRequestHandler : {session.MyPlayer.Name}");
                SessionManager.SetUserLoggedIn(packet.UserId, session);
            }
            // 전송 로직은 PacketUtils 등에서 만들어주면 좋음
            session.Send(PacketType.S_LoginResponse, response);
        }


        public static void WelcomeHandler(ServerSession session, LoginRequest req)
        {
            LoginResponse res = new LoginResponse() { Message = $"Welcome {req.UserId}" };
            session.Send(PacketType.S_LoginResponse, res);
        }
        public static void C_LeaveGameHandler(ServerSession session, C_LeaveGame packet)
        {
            Console.WriteLine($"[LeaveGame] Player {packet.UserId} 나감");
            GameRoom.Instance.Leave(session.MyPlayer);
        }
        public static void C_EnterGameHandler(ServerSession session, C_EnterGame packet)
        {
            // 1. 이름이 비어있으면 무시
            if (string.IsNullOrEmpty(packet.Name))
            {
                ServerPacketUtils.SendError(session, ErrorCode.InvalidName, "이름이 비어있습니다.");
                return;
            }

            // 2. 이름이 너무 길면 무시
            if (packet.Name.Length > 12)
            {
                ServerPacketUtils.SendError(session, ErrorCode.InvalidName, "이름이 너무 깁니다.");
                return;
            }

            // 3. 상태검사 : 로그인 상태가 아니면 무시
            if (!session.IsLoggedIn || session.MyPlayer == null)
            {
                ServerPacketUtils.SendError(session, ErrorCode.NotLoggedIn, "로그인 상태가 아닙니다.");
                return;
            }

            Console.WriteLine($"[EnterGame] 유저 '{packet.Name}' 입장 요청");
            GameRoom.Instance.Push(() =>
            {
                GameRoom.Instance.Enter(session, packet);
            });
        }
        public static void C_PingHandler(ServerSession session, C_Ping packet)
        {
            Console.WriteLine($"[Ping] {packet.Message}");

            S_Pong pong = new S_Pong()
            {
                Message = "Pong From Server"
            };
            session.Send(PacketType.S_Pong, pong);
        }
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
            GameRoom.Instance.BroadcastMove(s_Move, session.MyPlayer);
        }
        public static void C_LoadVillageDataHandler(ServerSession session, C_LoadVillageDataRequest packet)
        {
            // 요청한 유저에게 보내준다. 
            // EF 통해 DB 접근
            Console.WriteLine($"Packet ID : {packet.PlayerId}");
            using (var db = new AppDbContext())
            {
                // 1. 최신 버전 조회
                var latestVersion = db.PlacedObjects
                    .Where(o => o.UserId == packet.PlayerId)
                    .Max(o => (int?)o.Version); // null safety

                if (latestVersion == null)
                {
                    Console.WriteLine("불러올 데이터가 없습니다.");
                    // 필요하면 빈 리스트라도 보내기
                    session.Send(PacketType.S_LoadVillageDataResponse, new S_LoadVillageDataResponse());
                    return;
                }

                // 2. 해당 버전 데이터 불러오기
                var placedObjects = db.PlacedObjects
                    .Where(o => o.UserId == packet.PlayerId && o.Version == latestVersion)
                    .ToList();

                // 3. 응답 패킷 구성
                S_LoadVillageDataResponse response = new S_LoadVillageDataResponse();

                foreach (var obj in placedObjects)
                {
                    PlacedObject placed = new PlacedObject()
                    {
                        Type = obj.ObjectType,
                        PosX = obj.PosX,
                        PosY = obj.PosY,
                        DirY = obj.DirY,
                        Timestamp = new DateTimeOffset(obj.UpdatedAt).ToUnixTimeMilliseconds(),
                        ObjectId = obj.ObjectId,
                    };
                    response.PlaceObjects.Add(placed);

                }
                #region 작물 데이터 보내주기 

                List<PlantedCrop> LoadedPlantedCrops = new List<PlantedCrop>();
                List<PlantedCropDto> plantedCrops = new List<PlantedCropDto>();
                try
                {
                    plantedCrops = EFVillageDB.LoadPlantedCrops(packet.PlayerId);
                    Console.WriteLine($"불러온 작물 수: {plantedCrops?.Count ?? 0}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[예외] LoadPlantedCrops 중 오류: {ex.Message}");
                    return;
                }
                Console.WriteLine("작물 데이터 시작");
                var plantedProtos = plantedCrops.Select(CropPacektConverter.ToProto).ToList();

                response.PlanteCrops.AddRange(plantedProtos);
                #endregion

                session.Send(PacketType.S_LoadVillageDataResponse, response);
            }
        }

        public static void C_SaveVillageDataHandler(ServerSession session, C_SaveVillageData packet)
        {
            int userId = session.MyPlayer.PlayerId;

            // 1. 기존 백업 삭제
            EFVillageDB.DeleteBackup(userId);

            // 2. 최신을 백업으로
            EFVillageDB.BackupCurrent(userId);

            // 3. 새로 받은 오브젝트 저장
            List<PlacedObjectDTO> dtoList = new();

            foreach (var obj in packet.PlaceObjects)
            {
                dtoList.Add(new PlacedObjectDTO
                {
                    UserId = userId,
                    Version = 1,
                    ObjectType = obj.Type,
                    PosX = (int)obj.PosX,
                    PosY = (int)obj.PosY,
                    DirY = obj.DirY,
                    ObjectId = obj.ObjectId,
                    ObjLevel = 1, // or obj.Level
                    UpdatedAt = DateTime.Now
                });
            }

            EFVillageDB.SaveVillageData(session.MyPlayer.PlayerId, dtoList);
        }
        public static void C_SavePlantedCropHandler(ServerSession session, C_SavePlantedData packet)
        {
            // 클라에서 서버에 심어진 식물 저장 
            int userId = session.MyPlayer.PlayerId;
            PlantedCropDto plantedCropDto = new PlantedCropDto()
            {
                OwnerUserId = userId,
                CropId = packet.PlantedData.CropId,
                PosX = packet.PlantedData.PosX,
                PosY = packet.PlantedData.PosY,
                GrowthStage = packet.PlantedData.GrowthStage,
                GrowthTimer = packet.PlantedData.GrowthTimer,
                PlantedAt = DateTime.UnixEpoch.AddSeconds(packet.PlantedData.PlantedAt),
                LastUpdateAt = DateTime.Now,
            };

            EFVillageDB.SavePlantedCrop(session.MyPlayer.PlayerId, plantedCropDto);
        }
        public static void C_DestroyPlantedCrop(ServerSession session, C_DestroyPlantedCrop packet)
        {
            int userId = session.MyPlayer.PlayerId;

            // 여기서 온 id로 테이블에서 찾아 수정
            if (EFVillageDB.CanUpdatePlantedCrop(userId, packet.Id))
            {
                PlantedCropDto plantedCropDto = new PlantedCropDto()
                {
                    IsHarvest = packet.IsHarvest,
                    HarvestTime = DateTime.UnixEpoch.AddSeconds(packet.HarvestTime),
                };
                EFVillageDB.UpdatePlantedCrop(packet.Id, plantedCropDto);
            }
            else
            {
                Console.WriteLine("해당 위치의 수확물을 제거할 수 없습니다.");
            }
        }
    }
}

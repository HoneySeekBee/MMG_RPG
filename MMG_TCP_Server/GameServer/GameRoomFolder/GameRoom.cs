using GameServer.Core;
using GameServer.Data;
using GameServer.Domain;
using Google.Protobuf;
using Packet;
using ServerCore;
using System.Numerics;
using System.Net.Http;
using GameServer.Attack;
using GameServer.Intreface;

namespace GameServer.GameRoomFolder
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }

        //private List<Player> _players = new List<Player>();
        private Dictionary<int, Player> _players = new();
        public GameRoom(int roomId)
        {
            RoomId = roomId;
        }
        private PlayerInfo MakePlayerInfo(Player p)
        {
            return new PlayerInfo
            {
                Name = p.CharacterInfo.CharacterName,
                PlayerId = p.UserId,
            };
        }
        public async Task Enter(ServerSession session)
        {
            if (_players.ContainsKey(session.MyPlayer.CharacterInfo.Id))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: {session.MyPlayer.CharacterInfo.CharacterName}");
                return;
            }
            var api = new API();
            session.Room = this;
            // 여기서 DB에 CharacterStatus 요청하기 
           var status =  await api.GetCharacterStatus(session.MyPlayer.CharacterInfo.Id);

            session.MyPlayer.Status = status;

            Player player = session.MyPlayer;

            Vector3 spawnVec = SpawnZoneLoader.GetRandomSpawnPos(RoomId, 0);
            player.PosX = spawnVec.X;
            player.PosY = spawnVec.Y;
            player.PosZ = spawnVec.Z;
            player.DirY = 0;
            _players.Add(session.MyPlayer.CharacterInfo.Id, player);

            // 해당 유저에게는 응답을 보내주고 나머지 방에 사람들에게는 입장 알려줌

            session.MyPlayer.CurrentRoomId = RoomId;

            var response = new S_EnterGameResponse();
            response.MapId = RoomId;


            foreach (var p in _players)
            {
                response.CharacterList.Add(CreateCharacterList(p.Value, p.Key == player.CharacterInfo.Id));
            }


            session.Send(PacketType.S_EnterGameResponse, response);

            Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 입장  -----> 같은 게임방의 유저들에게 입장 알려줘야함.");

            var broadcastEnter = new S_BroadcastEnter
            {
                EnterCharacter = CreateCharacterList(player, isLocal: false)
            };
            BroadcastEnter(broadcastEnter, player);
        }
        public Player FindPlayerById(int characterId)
        {
            return _players[characterId];
        }
        public List<CharacterStatus> FindPlayersInArea(Vector3 center, float radius)
        {
            List<CharacterStatus> result = new();

            foreach (var player in _players.Values)
            {
                CharacterStatus status = player.Status;
                if (status == null) continue;

                Vector3 position = status.Position;

                float distanceSq = Vector3.DistanceSquared(center, position);
                if (distanceSq <= radius * radius)
                {
                    result.Add(status);
                }
            }

            return result;
        }
        public void HandleAttack(CharacterStatus attacker, Vector3 pos, float rotY, WeaponData weapon)
        {
            Console.WriteLine($"임시 : HitType ( Area )");
            IHitDetector detector = HitDetectorFactory.Create(HitDetectorType.Area, weapon.Range);
            List<CharacterStatus> targets = detector.DetectTargets(this, attacker, pos, rotY, weapon);

            foreach (var target in targets)
            {
                // 1. 데미지 계산 및 적용
                target.OnDamaged(attacker, weapon.Damage);

                // 2. 클라이언트에게 브로드캐스트
                S_DamageBroadcast damagePacket = new S_DamageBroadcast
                {
                    TargetId = target.Id,
                    Damage = weapon.Damage,
                    AttackerId = attacker.Id
                };

                BroadcastDamage(damagePacket);
            }
        }
        private CharacterList CreateCharacterList(Player player, bool isLocal)
        {
            return new CharacterList
            {
                IsLocal = isLocal,
                CharacterInfo = player.CharacterInfo,
                PosX = player.PosX,
                PosY = player.PosY,
                PosZ = player.PosZ,
                DirY = player.DirY
            };
        }
        public bool HasPlayer(Player player)
        {
            return _players.ContainsKey(player.CharacterInfo.Id);
        }
        public void Update()
        {
            Flush(); // JobQueue에 쌓인 작업 처리
        }

        public void Leave(Player player)
        {
            if (_players.Remove(player.CharacterInfo.Id))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 퇴장");
        }
        private void Broadcast(PacketType packetType, IMessage message, Player exclude = null)
        {
            foreach (var player in _players)
            {
                if (exclude != null && player.Key == exclude.CharacterInfo.Id) continue;
                player.Value.Session.Send(packetType, message);
            }
        }
        public void BroadcastMove(S_BroadcastMove message, Player exclude = null)
        {
            if (exclude != null)
                exclude.UpdateMove(message.PosX, message.PosY, message.PosZ, message.DirY);

            Broadcast(PacketType.S_BroadcastMove, message, exclude);
        }

        public void BroadcastEnter(S_BroadcastEnter message, Player exclude = null)
        {
            Console.WriteLine($"캐릭터[{message.EnterCharacter.CharacterInfo.CharacterName}] 입장 - ");
            Broadcast(PacketType.S_BroadcastEnter, message, exclude);
        }
        public void BroadcastDamage(S_DamageBroadcast message)
        {
            Broadcast(PacketType.S_DamagekResponse, message);
        }
    }
}

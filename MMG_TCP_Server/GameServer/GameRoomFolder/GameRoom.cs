using GameServer.Core;
using GameServer.Data;
using GameServer.Domain;
using Google.Protobuf;
using Packet;
using ServerCore;
using System.Numerics;
using GameServer.Attack;
using GameServer.Data.Monster;
using System;
using GameServer.GameRoomFolder.Map;

namespace GameServer.GameRoomFolder
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }
        SpawnZoneManager spawnZoneMgr;

        private BattleSystem _battleSystem;
        private ProjectileManager _projectileManager = new();

        public Dictionary<int, Player> _players = new();
        public Dictionary<int, Monster> _monsters = new();
        private List<BlockArea> _blockAreas = new();

        private int _monsterIdCounter = 1;

        public GameRoom(int roomId)
        {
            Console.WriteLine($"GameRoom 초기화 {roomId}");
            RoomId = roomId;
            _battleSystem = new BattleSystem(this);
            spawnZoneMgr = new SpawnZoneManager(roomId);
            MonsterData();
            BlockData();
        }
        public async Task Enter(ServerSession session)
        {
            int charId = session.MyPlayer.CharacterInfo.Id;

            if (_players.ContainsKey(charId))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: {session.MyPlayer.CharacterInfo.CharacterName}");
                return;
            }

            var api = new API();
            session.Room = this;

            CharacterStatus status = await api.GetCharacterStatus(charId, session.MyPlayer.CharacterInfo.CharacterName, session.Room);
            if (status == null)
            {
                Console.WriteLine($"[Enter] status is NULL!!");
            }
            else
            {
                Console.WriteLine($"[Enter] status = Id:{status.Id}, Name:{status.Name}, Pos:{status.Position}");
            }
            session.MyPlayer.Status = status;

            Player player = session.MyPlayer;
            Vector3 spawnVec = spawnZoneMgr.GetRandomPlayerSpawnPos(0);
            player.PosX = spawnVec.X;
            player.PosY = spawnVec.Y;
            player.PosZ = spawnVec.Z;
            player.DirY = 0;

            _players.Add(charId, player);
            player.CurrentRoomId = RoomId;

            // 유저에게 현재 방 정보 전송
            CharacterSpawn(session, charId);

            MonsterSpawn(session);


            // 다른 유저에게 입장 브로드캐스트
            var broadcastEnter = new S_BroadcastEnter
            {
                EnterCharacter = CreateCharacterList(player, isLocal: false)
            };
            BroadcastEnter(broadcastEnter, player);
        }
        private void CharacterSpawn(ServerSession session, int charId)
        {
            var response = new S_EnterGameResponse { MapId = RoomId };

            foreach (var p in _players)
                response.CharacterList.Add(CreateCharacterList(p.Value, p.Key == charId));

            session.Send(PacketType.S_EnterGameResponse, response);
        }
        private void MonsterSpawn(ServerSession session)
        {
            var monsterListPacket = new S_MonsterList();

            foreach (var monster in _monsters.Values)
            {
                var original = monster.Status;

                var status = new MonsterStatus
                {
                    ID = monster.Id,
                    MonsterId = original.MonsterId,
                    MonsterName = original.MonsterName,
                    HP = original.HP,
                    MaxHP = original.MaxHP,
                    MoveSpeed = original.MoveSpeed,
                    ChaseRange = original.ChaseRange,
                    AttackRange = original.AttackRange,
                    MoveData = monster.MonsterMove, 

                    // 나머지 필드도 복사 필요시 추가
                };

                monsterListPacket.MonsterDataList.Add(status);
            }

            session.Send(PacketType.S_MonsterList, monsterListPacket);
        }
        private void MonsterData()
        {
            foreach (var zone in spawnZoneMgr.GetAllMonsterZones())
            {
                foreach (var info in zone.Monsters)
                {
                    for (int i = 0; i < info.SpawnCount; i++)
                    {
                        Vector3 spawnPos = spawnZoneMgr.GetRandomPosition(zone.Min, zone.Max);

                        MonsterStatus monsterStatus = MonsterDataManager.Get(info.MonsterId);

                        int id = GetNextMonsterId();

                        MonsterMoveData _moveData = new MonsterMoveData()
                        {
                            MonsterId = info.MonsterId,
                            MonsterMove = new MoveData()
                            {
                                PosX = spawnPos.X,
                                PosY = spawnPos.Y,
                                PosZ = spawnPos.Z,
                                DirY = 0,
                                Speed = 0,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }
                        };
                        Monster monster = new Monster(id, monsterStatus, _moveData, this);


                        _monsters.Add(monster.Id, monster);
                    }
                }
            }
        }
        public int GetNextMonsterId()
        {
            return _monsterIdCounter++;
        }

        public void Leave(Player player)
        {
            if (_players.Remove(player.CharacterInfo.Id))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 퇴장");
        }
        public Player FindPlayerById(int characterId) => _players[characterId];
        public List<CharacterStatus> FindPlayersInArea(Vector3 center, float radius)
        {
            List<CharacterStatus> result = new();

            foreach (var player in _players.Values)
            {
                CharacterStatus status = player.Status;
                if (status == null) continue;

                float distanceSq = Vector3.DistanceSquared(center, status.Position);
                if (distanceSq <= radius * radius)
                    result.Add(status);
            }

            return result;
        }
        public void HandleAttack(CharacterStatus attacker, Vector3 pos, float rotY, AttackData attackData)
        {
            _battleSystem.HandleAttack(attacker, pos, rotY, attackData);
        }
        public void LaunchProjectile(CharacterStatus attacker, Vector3 pos, float rotY, AttackData data)
        {
            float rad = rotY * (float)Math.PI / 180f;
            Vector3 forward = new((float)Math.Sin(rad), 0, (float)Math.Cos(rad));
            Console.WriteLine("[GameRoom] [LaunchProjectile]");
            Projectile p = new Projectile
            {
                OwnerId = attacker.Id,
                Position = pos,
                Direction = forward,
                Speed = 10f,
                Radius = 0.3f,
                MaxDistance = data.Range,
                AttackData = data
            };

            _projectileManager.AddProjectile(p);
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
        }  // 게임 루프 Tick 처리
        public void Update(float deltaTime)
        {
            _projectileManager.Update(deltaTime, this);
            // 이후: 플레이어 이동, AI 등도 여기서 처리 가능
        }

        public void Update()
        {
            Flush(); // JobQueue 처리
        }
        public void OnPlayerHit(int attackerId, int targetId, AttackData data)
        {
            Console.WriteLine($"[Hit] {targetId} hit by {attackerId} using {data.AttackType}");

            FindPlayerById(targetId).Status.OnDamaged(FindPlayerById(attackerId).Status, data.Damage);
            BroadcastDamage(new S_DamageBroadcast
            {
                AttackerId = attackerId,
                TargetId = targetId,
                Damage = data.Damage
            });
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

            Broadcast(PacketType.S_BroadcastMove, message);
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
        private void BlockData()
        {
            foreach (var blockPoint in spawnZoneMgr.GetAllBlockPoints())
            {
                BlockArea area = new BlockArea
                {
                    Id = blockPoint.Id,
                    Description = blockPoint.Description,
                    Min = new Vector3(blockPoint.Min.x, blockPoint.Min.y, blockPoint.Min.z),
                    Max = new Vector3(blockPoint.Max.x, blockPoint.Max.y, blockPoint.Max.z)
                };

                _blockAreas.Add(area);
                Console.WriteLine($"[BlockData] ID: {area.Id}, Desc: {area.Description}, Min: {area.Min}, Max: {area.Max}");
            }
        }
        public bool IsWalkable(Vector3 position)
        {
            foreach (var block in _blockAreas)
            {
                if (block.Contains(position))
                    return false;
            }
            return true;
        }

    }
}

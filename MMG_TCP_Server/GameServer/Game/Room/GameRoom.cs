using GameServer.Core;
using GameServer.Data;
using Google.Protobuf;
using Packet;
using ServerCore;
using System.Numerics;
using GameServer.Attack;
using GameServer.Data.Monster;
using System;
using GameServer.Game.Room.Map;
using GamePacket;
using MonsterPacket;
using GameServer.Game.Object;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;
using AttackPacket;
using Newtonsoft.Json;
using System.Net.Http;

namespace GameServer.Game.Room
{
    public class GameRoom : JobQueue
    {
        public int RoomId { get; private set; }
        SpawnZoneManager spawnZoneMgr;

        private BattleSystem _battleSystem;
        private ProjectileManager _projectileManager = new();

        public Dictionary<int, CharacterObject> _players = new();
        public Dictionary<int, MonsterObject> _monsters = new();
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
        #region Logic
        public async Task Enter(ServerSession session, CharacterInfo _characterInfo)
        {
            int charId = _characterInfo.Id;

            if (_players.ContainsKey(charId))
            {
                Console.WriteLine($"[GameRoom:{RoomId}] 이미 입장된 유저: ");
                return;
            }

            session.Room = this;

            Console.WriteLine($"[Enter] {_characterInfo.CharacterName}");

            // 유저에게 현재 방 정보 전송
            CharacterSpawn(session, charId, _characterInfo);

            MonsterSpawn(session);

        }

        public void Leave(CharacterObject player)
        {
            if (_players.Remove(player.CharacterInfo.Id))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 퇴장");
        }
        #endregion
        #region Player
        private async void CharacterSpawn(ServerSession session, int charId, CharacterInfo _characterInfo)
        {
            var api = new API();
            try
            {
                CharacterObject characterObj = await api.GetCharacterStatus(charId, _characterInfo.CharacterName, session.Room);

                if (characterObj == null)
                {
                    Console.WriteLine("[Enter] characterObj is null. 생성 실패");
                    return;
                }

                Vector3 spawnVec = spawnZoneMgr.GetRandomPlayerSpawnPos(0);

                MoveData moveData = new MoveData();

                moveData.PosX = spawnVec.X;
                moveData.PosY = spawnVec.Y;
                moveData.PosZ = spawnVec.Z;
                moveData.DirY = 0;

                characterObj.moveData = moveData;
                characterObj.CharacterInfo = _characterInfo;
                characterObj.Session = session;
                characterObj.SkillInfo = await SkillDataManager.GetCharacterSkill(charId);

                session.MyPlayer = characterObj;
                if (_players.ContainsKey(charId))
                {
                    Console.WriteLine($"[Enter] 이미 존재하는 charId: {charId}");
                    return;
                }
                _players.Add(charId, characterObj);
                characterObj.CurrentRoomId = RoomId;

                var response = new S_EnterGameResponse { MapId = RoomId };

                foreach (var p in _players)
                    response.CharacterList.Add(CreateCharacterList(p.Value, p.Key == charId));

                session.Send(PacketType.S_EnterGameResponse, response);

                // 다른 유저에게 입장 브로드캐스트
                var broadcastEnter = new S_BroadcastEnter
                {
                    EnterCharacter = CreateCharacterList(characterObj, isLocal: false)
                };
                BroadcastEnter(broadcastEnter, characterObj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Enter] 예외 발생! Message: {ex.Message}");
                // 필요하다면 ex.StackTrace나 ex.InnerException도 출력 가능
            }
        }

        public CharacterObject FindPlayerById(int characterId) => _players[characterId];
        public List<CharacterObject> FindPlayersInArea(Vector3 center, float radius)
        {
            List<CharacterObject> result = new();

            foreach (var player in _players.Values)
            {
                CharacterObject status = player;
                if (status == null) continue;

                float distanceSq = Vector3.DistanceSquared(center, status.Position);
                if (distanceSq <= radius * radius)
                    result.Add(status);
            }

            return result;
        }
        #endregion

        #region Monster

        private void MonsterSpawn(ServerSession session)
        {
            var monsterListPacket = new S_MonsterList();

            foreach (var monster in _monsters.Values)
            {
                var original = monster.Status;

                var status = new MonsterStatus
                {
                    ID = monster.objectInfo.Id,
                    HP = original.HP,
                    MonsterData = MonsterDataManager.Get(original.ID),
                    MoveData = monster.MonsterSpawnpoint,
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



                        MonsterStatus monsterStatus = new MonsterStatus()
                        {
                            ID = info.MonsterId,
                            MonsterData = MonsterDataManager.Get(info.MonsterId),
                        };

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
                        monsterStatus.MoveData = _moveData;


                        MonsterObject monster = new MonsterObject(id, monsterStatus, _moveData, this);

                        List<Vector3> patrolRoute = spawnZoneMgr.GeneratePatrolPoints(spawnPos, zone.Min, zone.Max, 2f, 5);
                        monster.SetPatrolRoute(patrolRoute);

                        _monsters.Add(monster.objectInfo.Id, monster);
                    }
                }
            }
        }
        public int GetNextMonsterId()
        {
            return _monsterIdCounter++;
        }
        #endregion

        public void HandleAttack(GameObject attacker, Vector3 pos, float rotY, Skill attackData)
        {
            // BroadCast 해준다.
            S_Attack s_Attack = new S_Attack()
            {
                AttackerId = attacker.ObjectId,
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z,
                DirY = rotY, 
                AttackId = attackData.AttackId,
            };
            BroadcastAttack(s_Attack, attacker);
            _battleSystem.HandleAttack(attacker, pos, rotY, attackData);
        }
        public void LaunchProjectile(GameObject attacker, Vector3 pos, float rotY, Skill data)
        {
            float rad = rotY * (float)Math.PI / 180f;
            Vector3 forward = new((float)Math.Sin(rad), 0, (float)Math.Cos(rad));

            Projectile p = new Projectile
            {
                OwnerId = attacker.ObjectId,
                Position = pos,
                Direction = forward,
                Speed = 10f,
                Radius = 0.3f,
                MaxDistance = data.Range,
                AttackData = data
            };

            _projectileManager.AddProjectile(p);
        }

        private CharacterList CreateCharacterList(CharacterObject player, bool isLocal)
        {
            return new CharacterList
            {
                IsLocal = isLocal,
                CharacterInfo = player.CharacterInfo,
                StatInfo = player.objectStatus,
                MoveInfo = player.moveData,
                SkillInfo = player.SkillInfo,
            };
        }
        public bool HasPlayer(CharacterObject player)
        {
            return _players.ContainsKey(player.CharacterInfo.Id);
        }  // 게임 루프 Tick 처리
        public void Update(float deltaTime)
        {
            _projectileManager.Update(deltaTime, this);
            // 이후: 플레이어 이동, AI 등도 여기서 처리 가능

            foreach (MonsterObject monster in _monsters.Values)
            {
                monster.Update(deltaTime);
            }
        }

        public void Update()
        {
            Flush(); // JobQueue 처리
        }
        public void OnPlayerHit(int attackerId, int targetId, Skill data)
        {
            Console.WriteLine($"[Hit] {targetId} hit by {attackerId} using {data.AttackType}");

            FindPlayerById(targetId).OnDamaged(FindPlayerById(attackerId), data.Damage);
            S_DamageBroadcast Damage = new S_DamageBroadcast()
            {
                Damage = new DamageInfo()
                {
                    AttackerId = attackerId,
                    TargetId = targetId,
                    Damage = data.Damage,

                    IsMonster = false
                }
            };
            
            BroadcastDamage(Damage);
        }

        #region Broadcast

        private void Broadcast(PacketType packetType, IMessage message, CharacterObject exclude = null)
        {

            foreach (var player in _players)
            {
                if (exclude != null && player.Key == exclude.CharacterInfo.Id) continue;
                player.Value.Session.Send(packetType, message);
            }
        }
        public void BroadcastMove(S_BroadcastMove message, CharacterObject exclude = null)
        {
            if (exclude != null)
                exclude.UpdateMove(message.BroadcastMove.PosX, message.BroadcastMove.PosY, message.BroadcastMove.PosZ, message.BroadcastMove.DirY);

            Broadcast(PacketType.S_BroadcastMove, message);
        }
        public void BroadcastMonsterMove(S_BroadcastMove message)
        {
            Broadcast(PacketType.S_BroadcastMonstermove, message);
        }
        public void BroadcastEnter(S_BroadcastEnter message, CharacterObject exclude = null)
        {
            Console.WriteLine($"캐릭터[{message.EnterCharacter.CharacterInfo.CharacterName}] 입장 - ");
            Broadcast(PacketType.S_BroadcastEnter, message, exclude);
        }
        public void BroadcastDamage(S_DamageBroadcast message)
        {
            Broadcast(PacketType.S_BroadcastDamage, message);
        }
        public void BroadcastAttack(S_Attack message, GameObject exclude = null)
        {
            CharacterObject characterObject = null;
            if (exclude.Type == ObjectType.Character)
                characterObject = exclude as CharacterObject;

            Broadcast(PacketType.S_BroadcastAttack, message, characterObject);
        }
        public void BroadcastAttackCast(S_CastAttack message)
        {
            Broadcast(PacketType.S_BroadcastCastAttack, message);
        }
        #endregion
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

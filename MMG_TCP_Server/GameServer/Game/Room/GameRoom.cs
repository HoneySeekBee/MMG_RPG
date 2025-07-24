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

        public GameRoomObject GameRoomObjectManager;
        private float countTime = 0;

        public GameRoom(int roomId)
        {
            Console.WriteLine($"GameRoom 초기화 {roomId}");
            RoomId = roomId;
            _battleSystem = new BattleSystem(this);
            spawnZoneMgr = new SpawnZoneManager(roomId);
            BlockData();
            GameRoomObjectManager = new GameRoomObject(this);
            GameRoomObjectManager.Init_MonsterData(spawnZoneMgr);
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
            GameRoomObjectManager.LocalCharacterSpawn(session, charId, _characterInfo);
            GameRoomObjectManager.Send_MonsterList(session);
        }

        public void Leave(CharacterObject player)
        {
            if (_players.Remove(player.CharacterInfo.Id))
                Console.WriteLine($"[GameRoom:{RoomId}] {player.CharacterInfo.CharacterName} 퇴장");
        }
        #endregion
        #region Player
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

        public void HandleAttack(GameObject attacker, Vector3 pos, float rotY, Skill attackData, bool isMonster)
        {
            // BroadCast 해준다.
            S_Attack s_Attack = new S_Attack()
            {
                AttackerId = attacker.objectInfo.Id,
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z,
                DirY = rotY,
                AttackId = attackData.AttackId,
                IsMonster = isMonster
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
            countTime += deltaTime;
            if(countTime > 10)
            {
                GameRoomObjectManager.MonsterRespawn();
                countTime = 0;
            }
        }

        public void Update()
        {
            Flush(); // JobQueue 처리
        }
        public void OnPlayerHit(int attackerId, int targetId, Skill data)
        {
            Console.WriteLine($"[Hit] {targetId} hit by {attackerId} using {data.AttackType}");

            if(FindPlayerById(targetId).OnDamaged(FindPlayerById(attackerId), data.Damage))
            {
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
        public void BroadcastDead(S_DeathBroadcast message)
        {
            Broadcast(PacketType.S_BroadcastDead, message);
        }
        public void BroadcastRespawnMonster(S_MonsterList message)
        {
            Broadcast(PacketType.S_RespawnMonsterList, message);
        }
        public void BroadcastPlayerDie(PlayerId message)
        {
            Broadcast(PacketType.S_BroadcastPlayerDie, message);
        }
        public void BroadcastPlayerRevive(S_PlayerRespawn message)
        {
            Broadcast(PacketType.S_BroadcastPlayerRespawn, message);
        }
        public void BroadcastLevelUp(S_BroadcastLevelUp message)
        {
            Broadcast(PacketType.S_BroadcastLevelUp, message);
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

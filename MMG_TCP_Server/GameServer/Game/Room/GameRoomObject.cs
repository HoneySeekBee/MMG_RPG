using AttackPacket;
using GamePacket;
using GameServer.Core;
using GameServer.Data;
using GameServer.Data.DataManager;
using GameServer.Data.Monster;
using GameServer.Game.Object;
using Google.Protobuf.WellKnownTypes;
using Microsoft.IdentityModel.Tokens;
using MonsterPacket;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Room
{
    // 이 클래스가 담당할 일 

    // 게임룸 내부
    // [1] 몬스터의 스폰과 리스폰 
    // [2] 유저의 스폰과 리스폰 


    public class GameRoomObject
    {
        public GameRoom Room { get; set; } // Room
        private SpawnZoneManager SpawnZoneMgr { get; set; }

        private int _monsterIdCounter = 1;
        public GameRoomObject(GameRoom room)
        {
            Room = room;
        }
        #region Sender
        public void Send_MonsterList(ServerSession session)
        {
            var monsterListPacket = new S_MonsterList();

            foreach (var monster in Room._monsters.Values)
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
        #endregion

        #region Monster 관련 

        // <zoneNumber, > 
        private Dictionary<int, MonsterSpawnZone> SpawnZone = new Dictionary<int, MonsterSpawnZone>();

        // < originalId, (zoneNumber, monsterId) 
        private Dictionary<int, (int, int)> ZoneMonster = new Dictionary<int, (int, int)>();

        public void Init_MonsterData(SpawnZoneManager spawnZoneMgr)
        {
            SpawnZoneMgr = spawnZoneMgr;

            foreach (var zone in SpawnZoneMgr.GetAllMonsterZones())
            {
                SpawnZone.Add(zone.Id, zone);
                foreach (var info in zone.Monsters)
                {
                    zone.SpawnedMonsterDic.Add(info.MonsterId, new List<int>());
                    for (int i = 0; i < info.SpawnCount; i++)
                    {
                        SpawnMonster(info.MonsterId, zone);
                    }
                }
            }
        }
        private int SpawnMonster(int MonsterId, MonsterSpawnZone zone)
        {
            Vector3 spawnPos = SpawnZoneMgr.GetRandomPosition(zone.Min, zone.Max);
            List<Vector3> patrolRoute = SpawnZoneMgr.GeneratePatrolPoints(spawnPos, zone.Min, zone.Max, 2f, 5);

            MonsterData monsterData = MonsterDataManager.Get(MonsterId);
            MonsterStatus monsterStatus = new MonsterStatus()
            {
                ID = MonsterId,
                MonsterData = monsterData,
                HP = monsterData.MaxHP,
            };
            int id = GetNextMonsterId();
            MonsterMoveData _moveData = new MonsterMoveData()
            {
                MonsterId = MonsterId,
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
            zone.SpawnedMonsterDic[MonsterId].Add(id);
            ZoneMonster.Add(id, (zone.Id, MonsterId));
            monsterStatus.MoveData = _moveData;
            MonsterObject monster = new MonsterObject(id, monsterStatus, _moveData, Room);
            monster.SetPatrolRoute(patrolRoute);

            Room._monsters.Add(monster.objectInfo.Id, monster);
            return id;
        }
        public void MonsterDeadProcess(int originalId)
        {
            // [1] Monseter 목록에서 제거
            int zoneId = ZoneMonster[originalId].Item1;
            int monsterId = ZoneMonster[originalId].Item2;
            SpawnZone[zoneId].SpawnedMonsterDic[monsterId].Remove(originalId);
            ZoneMonster.Remove(originalId);
            Room._monsters.Remove(originalId);

            S_DeathBroadcast s_DeathBroadcast = new S_DeathBroadcast()
            {
                IsMonster = true,
                ObjectId = originalId,
            };
            Room.BroadcastDead(s_DeathBroadcast);
        }
        public void MonsterRespawn()
        {
            List<int> RespawnMonster = new List<int>();
            foreach (var zone in SpawnZone.Values)
            {
                foreach (var info in zone.Monsters)
                {
                    int spawnedCount = zone.SpawnedMonsterDic[info.MonsterId].Count;
                    if (spawnedCount < info.SpawnCount)
                    {
                        for (int i = 0; i < info.SpawnCount - spawnedCount; i++)
                        {
                            RespawnMonster.Add(SpawnMonster(info.MonsterId, zone));
                        }
                    }
                }
            }
            if (RespawnMonster.Count == 0)
                return;

            var monsterListPacket = new S_MonsterList();

            for (int i = 0; i < RespawnMonster.Count; i++)
            {
                MonsterObject monster = Room._monsters[RespawnMonster[i]];

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

            Room.BroadcastRespawnMonster(monsterListPacket);
        }

        private int GetNextMonsterId()
        {
            return _monsterIdCounter++;
        }
        #endregion

        #region Player 관련
        public CharacterList CreateCharacter(CharacterObject player, bool isLocal)
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
        private MoveData SpawnArea()
        {
            Vector3 spawnVec = SpawnZoneMgr.GetRandomPlayerSpawnPos(0);

            MoveData moveData = new MoveData()
            {
                PosX = spawnVec.X,
                PosY = spawnVec.Y,
                PosZ = spawnVec.Z,
                DirY = 0
            };
            return moveData;
        }
        public async void LocalCharacterSpawn(ServerSession session, int charId, CharacterInfo _charInfo)
        {
            var api = new API();
            try
            {
                CharacterObject characterObj = await api.GetCharacterStatus(charId, _charInfo.CharacterName, Room);

                if (characterObj == null)
                {
                    Console.WriteLine("[Enter] characterObj is null. 생성 실패");
                    return;
                }
                MoveData moveData = SpawnArea();

                characterObj.moveData = moveData;
                characterObj.CharacterInfo = _charInfo;
                characterObj.Session = session;
                characterObj.SkillInfo = await SkillDataManager.GetCharacterSkill(charId);

                session.MyPlayer = characterObj;
                if (Room._players.ContainsKey(charId))
                {
                    Console.WriteLine($"[Enter] 이미 존재하는 charId: {charId}");
                    return;
                }

                Room._players.Add(charId, characterObj);
                characterObj.CurrentRoomId = Room.RoomId;

                var response = new S_EnterGameResponse { MapId = Room.RoomId };

                foreach (var p in Room._players)
                {

                    response.CharacterList.Add(CreateCharacter(p.Value, p.Key == charId));
                }
                session.Send(PacketType.S_EnterGameResponse, response);

                var broadcastEnter = new S_BroadcastEnter
                {
                    EnterCharacter = CreateCharacter(characterObj, isLocal: false)
                };
                Room.BroadcastEnter(broadcastEnter, characterObj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Enter] 예외 발생! Message: {ex.Message}");
            }
        }
        public void RespawnPlayer(CharacterObject deadPlayer)
        {
            MoveData moveData = SpawnArea();
            deadPlayer.UpdateMove(moveData.PosX, moveData.PosY, moveData.PosZ, moveData.DirY);
            deadPlayer.objectInfo.MoveInfo = moveData;
            deadPlayer.objectInfo.StatInfo.NowHP = deadPlayer.objectStatus.MaxHP;

        }
        // 캐릭터가 죽으면? 
        // 일단 정보들을 DB에 보내준다. 
        // HP가 MaxHP인 상태로 생성한다. 

        #endregion
    }
}

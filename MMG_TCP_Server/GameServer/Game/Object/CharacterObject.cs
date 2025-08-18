using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GamePacket;
using GameServer.Core;
using GameServer.Data;
using GameServer.Data.DataManager;
using GameServer.Game.Quest;
using GameServer.Game.Room;
using Google.Protobuf.WellKnownTypes;
using Packet;
using QuestPacket;
using static GameServer.Data.API;

namespace GameServer.Game.Object
{
    public class CharacterObject : GameObject
    {
        public ServerSession Session { get; set; }
        public CharacterQuest MyQuest { get; set; }
        public int? CurrentRoomId { get; set; }
        public CharacterInfo CharacterInfo { get; set; }
        public CharacterSkillInfo SkillInfo = new CharacterSkillInfo();

        private float MaxExp { get { return objectInfo.StatInfo.Level * 10 * (objectInfo.StatInfo.Level / 10 + 1); }}
        private float MaxHp {get{ return objectInfo.StatInfo.Level * 10; } }
        private float MaxMp {get{ return objectInfo.StatInfo.Level * 10; } }
        public long LastMoveTimestamp { get { return moveData.Timestamp; } set { moveData.Timestamp = value; } }

        public void UpdateMove(float posX, float posY, float posZ, float dirY)
        {
            moveData.PosX = posX;
            moveData.PosY = posY;
            moveData.PosZ = posZ;
            moveData.DirY = dirY;
        }

        public void OnDisconnected()
        {
            //Console.WriteLine($"[Player] {Character.CharacterName} disconnected.");

            // GameRoom에서 제거하거나 기타 정리 작업을 이곳에 작성

        }
        public CharacterObject(Status _status, string _name, int _id, GameRoom _room)
        {
            Type = ObjectType.Character;
            objectInfo = new ObjectInfo
            {
                Name = _name,
                Id = _id,
            };
            objectStatus = _status;
            objectInfo.StatInfo = objectStatus;
            objectInfo.MoveInfo = moveData;
            Room = _room;
        }
        public async Task GetCharacterQuest()
        {
            MyQuest = new CharacterQuest(this);
            await MyQuest.InitAsync();
        }
        public static CharacterObject ConvertToRuntimeStatus(CharacterStatusDTO dto, string name, GameRoom room)
        {
            // 여기서는 Status만 해주면 될것같다. 
            Status status = new Status()
            {
                Level = dto.level,
                Exp = dto.exp,
                MaxExp = dto.maxExp,
                Gold = dto.gold,
                MaxHP = dto.maxHp,
                NowHP = dto.nowHp,
                MaxMP = dto.maxMp,
                NowMP = dto.nowMp,
                Timestamp = new DateTimeOffset(dto.lastUpdated).ToUnixTimeSeconds()
            };

            return new CharacterObject(status, name, dto.characterId, room);
        }


        public override void OnDeath()
        {
            Console.Write("사망");
            PlayerId playerId = new PlayerId()
            {
                PlayerId_ = objectInfo.Id
            };
            Room.BroadcastPlayerDie(playerId);
        }

        public void GetReward(float _exp, int _gold)
        {
            objectInfo.StatInfo.Exp += _exp;
            objectInfo.StatInfo.Gold += _gold;
            if (objectInfo.StatInfo.Exp > objectInfo.StatInfo.MaxExp)
            {
                LevelUp();
                UpdateStatus();

                S_BroadcastLevelUp s_BroadcastLevelUp = new S_BroadcastLevelUp()
                {
                    CharacterId = objectInfo.Id,
                    Status = objectInfo.StatInfo,
                };
                Room.BroadcastLevelUp(s_BroadcastLevelUp);
            }
            else
            {
                Status packet = objectInfo.StatInfo;
                Session.Send(ServerCore.PacketType.S_UpdateStatus, packet);
            }
        }
        private async void UpdateStatus()
        {
            S_BroadcastLevelUp s_BroadcastLevelUp = new S_BroadcastLevelUp()
            {
                CharacterId = objectInfo.Id,
                Status = objectInfo.StatInfo,
            };
            Room.BroadcastLevelUp(s_BroadcastLevelUp);

            var api = new API();
            UpdateCharacterStatusRequest updateStatus = new UpdateCharacterStatusRequest()
            {
                CharacterId = objectInfo.Id,
                CharacterLevel = objectInfo.StatInfo.Level,
                MaxExp = objectInfo.StatInfo.MaxExp,
                Exp = objectInfo.StatInfo.Exp,
                NowHP = objectInfo.StatInfo.NowHP,
                HP = objectInfo.StatInfo.MaxHP,
                NowMP = objectInfo.StatInfo.NowMP,
                MP = objectInfo.StatInfo.MaxMP,
                Gold = objectInfo.StatInfo.Gold
            };
            await api.UpdateCharacterStatus(updateStatus);
        }

        public void LevelUp()
        {
            objectInfo.StatInfo.Exp -= MaxExp;
            objectInfo.StatInfo.Level++;
            objectInfo.StatInfo.MaxExp = MaxExp;
            objectInfo.StatInfo.MaxHP = MaxHp;
            objectInfo.StatInfo.NowHP = objectInfo.StatInfo.MaxHP;
            objectInfo.StatInfo.MaxMP = MaxMp;
            objectInfo.StatInfo.NowMP = objectInfo.StatInfo.MaxMP;

        }
    }
}

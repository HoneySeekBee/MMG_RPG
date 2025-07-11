using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GamePacket;
using GameServer.Core;
using GameServer.Data;
using GameServer.Game.Room;
using Packet;

namespace GameServer.Game.Object
{
    public class CharacterObject : GameObject
    {
        public ServerSession Session { get; set; }
        public int? CurrentRoomId { get; set; }
        public CharacterInfo CharacterInfo { get; set; }
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
            objectInfo = new ObjectInfo
            {
                Name = _name,
                Id = _id,
            };
            objectStatus = _status;
            Room = _room;
        }
        public static CharacterObject ConvertToRuntimeStatus(CharacterStatusDTO dto, string name, GameRoom room)
        {
            // 여기서는 Status만 해주면 될것같다. 
            Status status = new Status()
            {
                Level = dto.level,
                Exp = dto.exp,
                Gold = dto.gold,
                MaxHP = dto.maxHp,
                NowHP = dto.nowHp,
                MaxMP = dto.maxMp,
                NowMP = dto.nowMp,
                Timestamp = new DateTimeOffset(dto.lastUpdated).ToUnixTimeSeconds()
            };
            Console.WriteLine($"[ConvertToRuntimeStatus] #1 : {status}");
            Console.WriteLine($"[ConvertToRuntimeStatus] #2 : {name}");
            Console.WriteLine($"[ConvertToRuntimeStatus] #3 : {room == null}");
            Console.WriteLine($"[ConvertToRuntimeStatus] #4 : {dto.characterId}");

            return new CharacterObject(status, name, dto.characterId, room);
        }
    }
}

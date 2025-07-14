using GamePacket;
using GameServer.Game.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Object
{
    public enum ObjectType
    {
        Character,
        Monster,
        Projectile
    }
    // 캐릭터, 몬스터, 아이템 등을 여기서 관리해야겠다. 
    public class GameObject
    {
        public ObjectType Type; // 오브젝트의 타입
        public int ObjectId; // 생성자 번호 

        public GameRoom Room { get; set; } // Room

        public ObjectInfo objectInfo = new ObjectInfo();  // ObjectInfo
        public MoveData moveData = new MoveData(); // PositionInfo
        public Status objectStatus = new Status(); // Statinfo

        public Quaternion Rotation; // Dir

        private Vector3 _dir;
        public bool IsDead => objectInfo.StatInfo.NowHP <= 0;
        public Vector3 Dir
        {
            get { _dir.Y = moveData.DirY; return _dir; }
            set { _dir = value; moveData.DirY = _dir.Y; }
        }
        private Vector3 _position;
        public Vector3 Position
        {
            get => _position;
            set => _position = value;
        }
        public void AddPosition(Vector3 delta)
        {
            Position += delta;

            moveData.PosX = Position.X;
            moveData.PosY = Position.Y;
            moveData.PosZ = Position.Z;

            objectInfo.MoveInfo = moveData;
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;

            moveData.PosX = pos.X;
            moveData.PosY = pos.Y;
            moveData.PosZ = pos.Z;
            moveData.Speed = 0f;

            objectInfo.MoveInfo = moveData;
        }

        public void SetObjectId(int id)
        {
            ObjectId = id;
        }
        public void OnDamaged(GameObject attacker, float damage)
        {
            Console.WriteLine($"{objectInfo.Name}이 {attacker.objectInfo.Name}으로 부터 공격당함");
            objectInfo.StatInfo.NowHP -= damage;
            Console.WriteLine($"HP : {objectInfo.StatInfo.NowHP}/{objectInfo.StatInfo.MaxHP} | 데미지 {damage}");
        }

    }
}

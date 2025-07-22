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
    public abstract class GameObject
    {
        public ObjectType Type; // 오브젝트의 타입
        public int ObjectId; // 생성자 번호 

        public GameRoom Room { get; set; } // Room

        public ObjectInfo objectInfo = new ObjectInfo();  // ObjectInfo
        public MoveData moveData = new MoveData(); // PositionInfo
        public Status objectStatus = new Status(); // Statinfo

        protected DateTime RecentDamagedTime;

        private Vector3 _dir;
        public bool IsDead => objectInfo.StatInfo.NowHP <= 0;
        protected Dictionary<int, (float, DateTime)> RecentlyAttacker = new Dictionary<int, (float, DateTime)>();

        public Vector3 Dir
        {
            get { _dir.Y = moveData.DirY; return _dir; }
            set { _dir = value; moveData.DirY = _dir.Y; }
        }
        private Vector3 _position;
        public Vector3 Position
        {
            get
            {
                _position.X = moveData.PosX;
                _position.Y = moveData.PosY;
                _position.Z = moveData.PosZ;
                return _position;
            }
            set
            {
                _position = value;
            }
        }
        public void AddPosition(Vector3 delta)
        {
            moveData.PosX += delta.X;
            moveData.PosY += delta.Y;
            moveData.PosZ += delta.Z;

            objectInfo.MoveInfo = moveData;
        }

        public void SetPosition(Vector3 pos)
        {
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
        public bool CheckDamagedDelay()
        {
            TimeSpan diff = DateTime.UtcNow - RecentDamagedTime;
            return (diff.TotalSeconds <= 0.1f);
        }
        public abstract void OnDeath();
        public bool OnDamaged(GameObject attacker, float damage)
        {
            if (CheckDamagedDelay())
            {
                Console.WriteLine($"[GameObject] OnDamaged {RecentDamagedTime} / {DateTime.UtcNow - RecentDamagedTime} ");
                return false;
            }
            if (IsDead)
                return false;

            Console.WriteLine($"{objectInfo.Name}이 {attacker.objectInfo.Name}으로 부터 공격당함");
            objectInfo.StatInfo.NowHP -= damage;
            Console.WriteLine($"HP : {objectInfo.StatInfo.NowHP}/{objectInfo.StatInfo.MaxHP} | 데미지 {damage}");

            RecentDamagedTime = DateTime.UtcNow;

            if (RecentlyAttacker.ContainsKey(attacker.objectInfo.Id) == false)
            {
                RecentlyAttacker.Add(attacker.objectInfo.Id, (damage, RecentDamagedTime));
            }
            else
            {
                var record = RecentlyAttacker[attacker.objectInfo.Id];
                TimeSpan timeSinceLastHit = RecentDamagedTime - record.Item2;
                if (timeSinceLastHit.TotalSeconds <= 15)
                {
                    RecentlyAttacker[attacker.objectInfo.Id] = (record.Item1 + damage, RecentDamagedTime);
                }
                else
                {
                    RecentlyAttacker[attacker.objectInfo.Id] = (damage, RecentDamagedTime);
                }
            }

            if (IsDead)
            {
                OnDeath();
            }

            return true;
        }
        public List<(int id, float damageRatio)> GetRecentDamageRatiosSorted(DateTime deadTime, float thresholdSeconds = 15f)
        {
            // 1. 15초 이내 공격 기록만 필터링
            var recentAttackers = RecentlyAttacker
                .Where(x => (deadTime - x.Value.Item2).TotalSeconds <= thresholdSeconds)
                .ToList();

            // 2. 총 데미지 합
            float totalDamage = recentAttackers.Sum(x => x.Value.Item1);
            if (totalDamage <= 0)
                return new List<(int, float)>(); // 데미지가 없으면 빈 리스트

            // 3. (id, 비율) 리스트로 변환 후 정렬
            var result = recentAttackers
                .Select(x => (x.Key, x.Value.Item1 / totalDamage))
                .OrderByDescending(x => x.Item2) // damageRatio 내림차순 정렬
                .ToList();

            return result;
        }

    }
}

using AttackPacket;
using GamePacket;
using GameServer.Game.Object;
using GameServer.Game.Room;
using MonsterPacket;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class MonsterObject : GameObject
    {
        public MonsterStatus Status { get; private set; }
        public CharacterObject Target { get; private set; }
        public bool HasTarget => Target != null;

        public MonsterStateMachine StateMachine { get; }
        public MonsterMover Mover { get; }
        public MonsterSensor Sensor { get; }
        public MonsterMoveData MonsterSpawnpoint;

        private List<Vector3> _patrolPoints = new();
        private int _currentPatrolIndex = 0;
        public Vector3 CurrentPatrolPoint => _patrolPoints.Count > 0 ? _patrolPoints[_currentPatrolIndex] : Position;
        public List<MonsterSkill> monsterSkills = new();

        public MonsterObject(int id, MonsterStatus status, MonsterMoveData monsterMove, GameRoom room)
        {
            // 부모 객체의 필드들 설정
            Type = ObjectType.Monster;
            ObjectId = id;
            Room = room;

            objectStatus = new Status
            {
                MaxHP = status.MonsterData.MaxHP,
                NowHP = status.MonsterData.MaxHP,
                MaxMP = 0,
                NowMP = 0,
                Level = 2,
                Gold = 0,
                Exp = 0
            };
            MonsterSpawnpoint = monsterMove;
            MoveData _moveData = monsterMove.MonsterMove;
            objectInfo = new ObjectInfo
            {
                Id = id,
                Name = status.MonsterData.MonsterName,
                MoveInfo = _moveData,
                StatInfo = objectStatus
            };
            Console.WriteLine($"해당 몬스터의 체력은? {objectInfo.StatInfo.NowHP}");
            moveData = _moveData;
            Position = new Vector3(moveData.PosX, moveData.PosY, moveData.PosZ);

            Status = status;

            MonsterSkillInfo monsterSkillInfo = Status.MonsterData.SkillInfo;
            monsterSkills = monsterSkillInfo.SkillInfo
   .Select(skill => new MonsterSkill
   {
       MonsterAttack = skill.MonsterAttack,
       Skill = skill.Skill
   })
   .ToList();

            Mover = new MonsterMover(this);
            Sensor = new MonsterSensor(this, room);
            StateMachine = new MonsterStateMachine();

            StateMachine.ChangeState(new IdleState(), this);
        }

        public void Update(float deltaTime)
        {
            StateMachine.Update(this, deltaTime);
        }

        public void SetTarget(CharacterObject target) => Target = target;
        public void ClearTarget() => Target = null;
        public bool IsInChaseRange()
        {
            return HasTarget && Vector3.Distance(Position, Target.Position) <= Status.MonsterData.ChaseRange;
        }
        #region 공격 관련
        public bool IsInAttackRange(float range = -1)
        {
            if (Target == null)
                return false;
            if (range == -1)
                range = Status.MonsterData.AttackRange;

            if (Target.objectStatus.NowHP <= 0)
            {
                ClearTarget();
                return false;
            }
            return HasTarget && Vector3.Distance(Position, Target.Position) <= range;
        }
        public void Attack(Skill skill)
        {
            Room.HandleAttack(this, this.Position, this.moveData.DirY, skill);
        }
        public void AttackCast(bool isMonster, int CasterId, int Attackid, float CastTime)
        {
            // 캐스트 패킷 보내기 
            S_CastAttack CastAttack = new S_CastAttack()
            {
                IsMonster = isMonster,
                CasterId = CasterId,
                AttackId = Attackid,
                CastTime = CastTime
            };
            Room.BroadcastAttackCast(CastAttack);
        }
        #endregion

        #region 사망 처리
        public override void OnDeath()
        {
            Console.WriteLine($"[Monster] {objectInfo.Name} 사망");
            // TODO: Drop, Remove 등
            
            Room.GameRoomObjectManager.MonsterDeadProcess(objectInfo.Id);
        }
        #endregion

        #region 순찰 관련
        public void SetPatrolRoute(List<Vector3> patrolPoints)
        {
            _patrolPoints = patrolPoints;
            _currentPatrolIndex = 0;
        }

        public void MoveToNextPatrolPoint()
        {
            if (_patrolPoints.Count == 0) return;
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        }
        #endregion

        public MonsterSkill SelectAttack()
        {
            int total = monsterSkills.Sum(a => a.MonsterAttack.Frequency);
            Random rand = new Random();
            int choice = rand.Next(0, total);

            int cumulative = 0;
            for (int i = 0; i < monsterSkills.Count; i++)
            {
                cumulative += monsterSkills[i].MonsterAttack.Frequency;
                if (choice < cumulative)
                {
                    // 이 인덱스가 선택된 것
                    return monsterSkills[i];
                }
            }

            Console.WriteLine("[MonsterObject] SelectAttack 오류 ");
            return null;
        }

        #region 위치 동기화

        public void SetDirectionY(float angleY)
        {
            moveData.DirY = angleY;
        }
        #endregion
    }
}

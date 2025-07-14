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

        public MonsterObject(int id, MonsterStatus status, MonsterMoveData monsterMove, GameRoom room)
        {
            // 부모 객체의 필드들 설정
            Type = ObjectType.Monster;
            ObjectId = id;
            Room = room;

            objectStatus = new Status
            {
                MaxHP = status.MaxHP,
                NowHP = status.HP,
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
                Name = status.MonsterName,
                MoveInfo = _moveData,
                StatInfo = objectStatus
            }; 
            Console.WriteLine($"해당 몬스터의 체력은? {objectInfo.StatInfo.NowHP}");
            moveData = _moveData;
            Position = new Vector3(moveData.PosX, moveData.PosY, moveData.PosZ);

            Status = status;

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
            return HasTarget && Vector3.Distance(Position, Target.Position) <= Status.ChaseRange;
        }
        #region 공격 관련
        public bool IsInAttackRange()
        {
            return HasTarget && Vector3.Distance(Position, Target.Position) <= Status.AttackRange;
        }

        public void AttackTarget()
        {
            if (Target == null) return;
            Target.OnDamaged(this, 2); // null = monster 공격자 미지정
            Console.WriteLine("[Monster] AttackDamage는 나중에 ID로 찾아서 지정하기");
        }
        #endregion

        #region 사망 처리
        public void OnDeath()
        {
            Console.WriteLine($"[Monster] {objectInfo.Name} 사망");
            // TODO: Drop, Remove 등
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

        #region 위치 동기화
       
        public void SetDirectionY(float angleY)
        {
            moveData.DirY = angleY;
        }
        #endregion
    }
}

using GameServer.Domain;
using GameServer.GameRoomFolder;
using Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class Monster
    {
        #region Monster Data
        public int Id { get; }
        public string Name { get; }
        public MonsterStatus Status { get; }

        private Vector3 _position;
        public Vector3 Position => _position;
        #endregion

        #region AI 및 구성요소
        public MonsterMoveData MonsterMove { get; private set; }

        public CharacterStatus Target { get; private set; }
        public bool HasTarget => Target != null;
        public bool IsDead => Status.HP <= 0;

        public MonsterStateMachine StateMachine { get; }
        public MonsterMover Mover { get; }
        public MonsterSensor Sensor { get; }

        private List<Vector3> _patrolPoints = new();
        private int _currentPatrolIndex = 0;
        public Vector3 CurrentPatrolPoint => _patrolPoints.Count > 0 ? _patrolPoints[_currentPatrolIndex] : _position;
        #endregion

        public Monster(int id, string name, MonsterStatus status, GameRoom room)
        {
            Id = id;
            Name = name;
            Status = status;

            MonsterMove = new MonsterMoveData
            {
                MonsterId = id,
                MoveData = new MoveData()
            };

            Mover = new MonsterMover(this);
            Sensor = new MonsterSensor(this, room);
            StateMachine = new MonsterStateMachine();

            _position = Vector3.Zero;

            StateMachine.ChangeState(new IdleState(), this);
        }

        public void Update(float deltaTime)
        {
            StateMachine.Update(this, deltaTime);
        }

        #region 타겟 관련
        public void SetTarget(CharacterStatus target) => Target = target;
        public void ClearTarget() => Target = null;
        #endregion

        #region 공격 관련
        public bool IsInAttackRange()
        {
            return HasTarget && Vector3.Distance(Position, Target.Position) <= Status.AttackRange;
        }

        public void AttackTarget()
        {
            if (Target == null) return;
            Target.OnDamaged(null, 2); // null = monster 공격자 미지정
            Console.WriteLine("[Monster] AttackDamage는 나중에 ID로 찾아서 지정하기");
        }
        #endregion

        #region 사망 처리
        public void OnDeath()
        {
            Console.WriteLine($"[Monster] {Name} 사망");
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
        public void AddPosition(Vector3 delta)
        {
            _position += delta;

            MonsterMove.MoveData.PosX = _position.X;
            MonsterMove.MoveData.PosY = _position.Y;
            MonsterMove.MoveData.PosZ = _position.Z;
            MonsterMove.MoveData.Speed = Status.MoveSpeed;
        }

        public void SetPosition(Vector3 pos)
        {
            _position = pos;

            MonsterMove.MoveData.PosX = pos.X;
            MonsterMove.MoveData.PosY = pos.Y;
            MonsterMove.MoveData.PosZ = pos.Z;
            MonsterMove.MoveData.Speed = 0f;
        }

        public void SetDirectionY(float angleY)
        {
            MonsterMove.MoveData.DirY = angleY;
        }
        #endregion
    }
}

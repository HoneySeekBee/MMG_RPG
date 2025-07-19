using GamePacket;
using MonsterPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Data.Monster
{
    public class AttackState : IMonsterState
    {
        private MonsterSkill _currentSkill;
        private float _castTimer;
        private float _delayTimer;
        private bool _isCasting;
        private bool _isDelaying;
        private Dictionary<int, float> _cooldowns = new();
        public void Enter(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Attack 상태 진입");
            _isCasting = false;
            _isDelaying = false;
            _currentSkill = null;
            foreach (var skill in monster.monsterSkills)
            {
                _cooldowns[skill.Skill.AttackId] = 0f;
            }
        }

        public void Update(MonsterObject monster, float deltaTime)
        {
            foreach (var key in _cooldowns.Keys.ToList())
            {
                _cooldowns[key] = Math.Max(0f, _cooldowns[key] - deltaTime);
            }
            if (monster.Target == null || monster.Target.IsDead)
            {
                monster.StateMachine.ChangeState(new PatrolState(), monster);
                return;
            }
            float attackRange = -1;
            if (_isCasting)
                attackRange = _currentSkill.Skill.Range;
            if (!monster.IsInAttackRange(attackRange))
            {
                monster.StateMachine.ChangeState(new ChaseState(), monster);
                return;
            }

            // 시전 중 
            if (_isCasting)
            {
                _castTimer -= deltaTime;
                if (_castTimer <= 0f)
                {
                    // 시전 끝났으니 공격 실행
                    monster.Attack(_currentSkill.Skill); // 여기에 Skill 정보 넘김


                    _isCasting = false;
                    _isDelaying = true;
                    _delayTimer = _currentSkill.Skill.DelayAfter;

                    // 쿨다운 설정
                    _cooldowns[_currentSkill.Skill.AttackId] = _currentSkill.Skill.Cooldown;
                }
                return;
            }

            // 딜레이 
            if (_isDelaying)
            {
                _delayTimer -= deltaTime;
                if (_delayTimer <= 0f)
                {
                    _isDelaying = false;
                }
                return;
            }
            var availableSkills = monster.monsterSkills
            .Where(s => _cooldowns[s.Skill.AttackId] <= 0f)
            .ToList();

            if (availableSkills.Count == 0)
                return;

            _currentSkill = SelectSkillByFrequency(availableSkills);
            _isCasting = true;

            _castTimer = _currentSkill.Skill.CastTime;
            if (_castTimer != 0)
            {
                Console.WriteLine($"[AttackState] {_currentSkill.Skill.AttackName} 시전 시작 (CastTime: {_castTimer})");
                monster.AttackCast(true, monster.objectInfo.Id, _currentSkill.Skill.AttackId, _castTimer); // 캐스트 패킷 보내기 
            }
        }

        public void Exit(MonsterObject monster)
        {
            Console.WriteLine($"{monster.objectInfo.Name} → Attack 상태 종료");
        }

        private MonsterSkill SelectSkillByFrequency(List<MonsterSkill> skills)
        {
            int total = skills.Sum(s => s.MonsterAttack.Frequency);
            int randValue = new Random().Next(0, total);

            int cumulative = 0;
            foreach (var skill in skills)
            {
                cumulative += skill.MonsterAttack.Frequency;
                if (randValue < cumulative)
                    return skill;
            }

            return skills[0]; // fallback
        }
    }
}

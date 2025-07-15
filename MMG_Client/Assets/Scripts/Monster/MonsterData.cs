using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MMG
{
    public class MonsterData : ScriptableObject
    {
        public int MonsterId;
        public string MonsterName;

        public float _MaxHP;
        public float _MoveSpeed;

        public float _ChaseRange;
        public float _AttackRange;

        [System.Serializable]
        public class MonsterAttackData
        {
            public AttackInputType inputType;
            public int AttackAppearanceAmount;
            public AttackData attackData;

            public int InputTypeInt => (int)inputType;
        }

        public List<MonsterAttackData> _AttackData = new List<MonsterAttackData>();

        // Monster 기본 정보 DTO로 변환
        public MonsterDto ToMonsterDto()
        {
            return new MonsterDto
            {
                Id = MonsterId,
                Name = MonsterName,
                Speed = _MoveSpeed,
                ChaseRange = _ChaseRange,
                AttackRange = _AttackRange
            };
        }

        // MonsterSkill 목록 DTO로 변환
        public List<MonsterSkillDto> ToMonsterSkillDtos()
        {
            List<MonsterSkillDto> list = new List<MonsterSkillDto>();
            foreach (var a in _AttackData)
            {
                list.Add(new MonsterSkillDto
                {
                    MonsterId = this.MonsterId,
                    SkillId = a.attackData.AttackId,
                    Frequency = a.AttackAppearanceAmount,
                    InputType = a.InputTypeInt
                });
            }
            return list;
        }

        public MonsterStatus Status
        {
            get
            {
                MonsterPacket.MonsterData monsterData = new MonsterPacket.MonsterData()
                {
                    MonsterId = MonsterId,
                    MonsterName = MonsterName,
                    MaxHP = _MaxHP,
                    MoveSpeed = _MoveSpeed,
                };
                return new MonsterStatus
                {
                    HP = _MaxHP,
                    MonsterData = monsterData,
                };
            }
        }
    }

    // DTO 정의
    [System.Serializable]
    public class MonsterDto
    {
        public int Id;
        public string Name;
        public float Speed;
        public float ChaseRange;
        public float AttackRange;
    }

    [System.Serializable]
    public class MonsterSkillDto
    {
        public int MonsterId;
        public int SkillId;
        public int Frequency;
        public int InputType;
    }
}

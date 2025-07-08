using Packet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MMG
{

    [CreateAssetMenu(menuName = "MMG/_MonsterData")]
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
        }
        public List<MonsterAttackData> _AttackData = new List<MonsterAttackData>();
        public Dictionary<int, (int, int)> monsterAttackDictionary = new Dictionary<int, (int, int)>();

        public void Save()
        {
            monsterAttackDictionary = new Dictionary<int, (int, int)>();
            foreach (MonsterAttackData data in _AttackData)
            {
                monsterAttackDictionary.Add((int)data.inputType, (data.AttackAppearanceAmount, (int)data.attackData.AttackId));
            }
        }
        public List<(int, (int, int))> AttackDataList
        {
            get
            {
                return monsterAttackDictionary
                    .Select(kv => (kv.Key, kv.Value))
                    .ToList();
            }
        }
        public MonsterStatus Status
        {
            get
            {
                return new MonsterStatus
                {
                    ID = MonsterId,
                    HP = _MaxHP,
                    MaxHP = _MaxHP,
                    MoveSpeed = _MoveSpeed,
                };
            }
        }
    }

}
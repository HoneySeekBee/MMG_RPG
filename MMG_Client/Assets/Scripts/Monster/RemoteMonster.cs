using MMG;
using MonsterPacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MMG.BattleAction;
using static MMG.MonsterData;

public class RemoteMonster : MonoBehaviour
{
    public int Id;
    public float HP;
    public MMG.MonsterData RemoteMonsterData;
    private MonsterController _controller;

    public void Init(MonsterStatus data, MonsterController controller)
    {
        Debug.Log("Monster");
        Id = data.ID;
        HP = data.HP;

        RemoteMonsterData = new MMG.MonsterData()
        {
            MonsterId = data.MonsterData.MonsterId,
            MonsterName = data.MonsterData.MonsterName,
            _MaxHP = data.MonsterData.MaxHP,
            _MoveSpeed = data.MonsterData.MoveSpeed,

            _ChaseRange = data.MonsterData.ChaseRange,
            _AttackRange = data.MonsterData.AttackRange,
        };

        MonsterSkillInfo skillInfo = data.MonsterData.SkillInfo;
        foreach (var skillData in skillInfo.SkillInfo)
        {
            RemoteMonsterData._AttackData.Add(new SaveKeyWithAttackData()
            {
                InputType = (AttackInputType)skillData.MonsterAttack.InputType,
                 
                attackData = new AttackData()
                {
                    AttackId = skillData.Skill.AttackId,
                    AttackName = skillData.Skill.AttackName,
                    OwnerType = skillData.Skill.OwnerType,
                    WeaponType = skillData.Skill.WeaponType,
                    AttackType = skillData.Skill.AttackType,

                    SkillLevel = 1,
                    SlotIndex = 1,
                    Appearance = skillData.MonsterAttack.Frequency,

                    Range = skillData.Skill.Range,
                    Angle = skillData.Skill.Angle,
                    Damage = skillData.Skill.Damage,
                    Cooldown = skillData.Skill.Cooldown,
                    DelayAfter = skillData.Skill.DelayAfter,
                    CastTime = skillData.Skill.CastTime,
                }
            });
        }

        _controller = controller;

        Vector3 spawnPoint = new Vector3(data.MoveData.MonsterMove.PosX, data.MoveData.MonsterMove.PosY, data.MoveData.MonsterMove.PosZ);
        transform.position = spawnPoint;
        transform.rotation = Quaternion.Euler(0, data.MoveData.MonsterMove.DirY, 0);

        _controller.Init_Position(spawnPoint, data.MoveData.MonsterMove.DirY);
        _controller.Init_AttackData(RemoteMonsterData._AttackData);
    }
    public void MoveTo(Vector3 targetPos, float dirY, float speed)
    {
        _controller.SetMove(targetPos, dirY, speed);
    }
    public void AttackHandle(BattleData battleData)
    {
        _controller.SetAttack(battleData);
    }
    public void GetDamage(float damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            Debug.Log("»ç¸Á~");
        }
    }
}

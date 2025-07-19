using MMG;
using GamePacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MMG.BattleAction;
using MMG.UI;

public class RemotePlayer : MonoBehaviour
{
    public CharacterData RemoteCharaceterData;
    public Status StatInfo;
    public CharacterSkillInfo SkillInfo;
    private PlayerController _controller; 
    public List<SaveKeyWithAttackData> attackDatas = new List<SaveKeyWithAttackData>();
    public void Init(CharacterList data, PlayerController controller)
    {
        RemoteCharaceterData = new CharacterData(){
            id = data.CharacterInfo.Id,
            userId = data.CharacterInfo.UserId,
            Gender = (Gender)data.CharacterInfo.Gender,
            characterName = data.CharacterInfo.CharacterName,
            @class = (ClassType)data.CharacterInfo.Class,
            appearanceCode = data.CharacterInfo.AppearanceCode,
        };
        _controller = controller;
        StatInfo = data.StatInfo;
        transform.position = new Vector3(data.MoveInfo.PosX, data.MoveInfo.PosY, data.MoveInfo.PosZ);
        // ¿ÜÇü ±¸¼º µîµî
        SkillInfo = data.SkillInfo;
    //        public AttackInputType InputType;
    //public AttackData attackData;
        foreach(var skill in SkillInfo.SkillInfo)
        {
            attackDatas.Add(new SaveKeyWithAttackData()
            {
                InputType = (AttackInputType)skill.CharacterSkill.InputType,
                attackData = new AttackData()
                {
                    AttackId = skill.Skill.AttackId,
                    AttackName = skill.Skill.AttackName,
                    OwnerType = skill.Skill.OwnerType,
                    WeaponType = skill.Skill.WeaponType,
                    AttackType = skill.Skill.AttackType,

                    SkillLevel = skill.CharacterSkill.SkillLevel,
                    SlotIndex = skill.CharacterSkill.SlotIndex,

                    Range = skill.Skill.Range,
                    Angle = skill.Skill.Angle,
                    Damage = skill.Skill.Damage,
                    Cooldown = skill.Skill.Cooldown,
                    DelayAfter = skill.Skill.DelayAfter,
                    CastTime = skill.Skill.CastTime,
                },
            });
        }
        Debug.Log($"[RemotePlayer] AttackDatas °¹¼ö {attackDatas.Count}");
        _controller.Init_AttackData(attackDatas);

        // ÆË¾÷ ¿­±â 
        if (_controller.isLocalPlayer)
        {
            PopupManager.Instance.Show<InGamePopup>((popup) => popup.Set(attackDatas));
        }
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
        StatInfo.NowHP -= damage;
        if (StatInfo.NowHP <= 0)
        {
            Debug.Log("»ç¸Á~");
        }
    }
}

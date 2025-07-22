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
    public InGamePopup InGameUI;
    public bool isDead { get{ return StatInfo.NowHP <= 0; } }

    public void Init(CharacterList data, PlayerController controller)
    {
        RemoteCharaceterData = new CharacterData()
        {
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
        Vector3 initPos = new Vector3(data.MoveInfo.PosX, data.MoveInfo.PosY, data.MoveInfo.PosZ);
        _controller.Init_Position(initPos, data.MoveInfo.DirY);

        // ¿ÜÇü ±¸¼º µîµî
        SkillInfo = data.SkillInfo;
        //        public AttackInputType InputType;
        //public AttackData attackData;
        foreach (var skill in SkillInfo.SkillInfo)
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
            PopupManager.Instance.Show<InGamePopup>((popup) =>
            {
                popup.Set(attackDatas);
                InGameUI = popup;
            });
        }
        _controller.animator.GetRemotePlayer(this);
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
    public void UpdateStatus(Status status)
    {
        StatInfo.Gold = status.Gold;
        StatInfo.Exp = status.Exp;
        Debug.Log($"º¸»ó È¹µæ {StatInfo.Gold}G / {StatInfo.Exp} ");
    }
    public void LevelUp(Status status)
    {
        StatInfo = status;
        Debug.Log("·¹º§¾÷~~ ");
    }
    public void PlayerDie()
    {
        // [1] »ç¸Á ¸ð¼Ç º¸¿©ÁÖ±â 
        _controller.animator.DeadAnimation();
        if (_controller.isLocalPlayer)
        {
            // ºÎÈ° ÆË¾÷ º¸¿©ÁÖ±â 

            PopupManager.Instance.Show<RevivePopup>((popup) =>
            {
                popup.Open();
                popup.SetPlayerId(RemoteCharaceterData.id);
            });
        }
    }
    public void PlayerRespawn(S_PlayerRespawn packet)
    {
        StatInfo = packet.StatInfo;
        _controller.animator.IdleAnimation();

        Vector3 respawnPosition = new Vector3(packet.MoveData.PosX, packet.MoveData.PosY, packet.MoveData.PosZ);
        _controller.Init_Position(respawnPosition, packet.MoveData.DirY);

        Debug.Log($"[RemotePlayer] PlayerRespawn : NowHp {packet.StatInfo.NowHP} / {StatInfo.NowHP}");
    }
}

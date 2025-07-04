using DevionGames.InventorySystem;
using Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public struct BattleInputData
    {
        public bool isLeftClick;       // 마우스 좌클릭
        public BattleSkill skillType;       // 사용 중인 스킬 번호 (예: 1번 스킬, 2번 스킬)
    }
    public enum BattleSkill { None,  Skill_1, Skill_2, Skill_3, Skill_4 }

    public class BattleAction : ActionBase<BattleInputData>
    {
        private PlayerAnimator playerAnimator;

        public override void Initialize(bool isLocal, IInputBase input)
        {
            base.Initialize(isLocal, input);

            Debug.Log("[BattleAction] 임시로 여기서 전투모드 설정하기 ");
            GameContextManager.SetContext(GameContext.Battle);

            playerAnimator = GetComponent<PlayerAnimator>();
        }

        protected override void Action(BattleInputData value)
        {
            string clickType = value.isLeftClick ? "좌클릭" : "우클릭";
            string skillName = value.skillType.ToString();

            Debug.Log($"{skillName}을 {clickType} 했습니다.");
            if(value.skillType == BattleSkill.None && value.isLeftClick)
            {
                Nomal_Attack();
            }
            else if (value.skillType == BattleSkill.None && value.isLeftClick == false)
            {
                Critical_Attack();
            }
            else if (value.skillType != BattleSkill.None && value.isLeftClick == false)
            {
                Debug.Log("스킬 취소");
            }
            else if (value.skillType == BattleSkill.Skill_1 && value.isLeftClick)
            {
                Debug.Log("스킬 1 발동");
            }
            else if (value.skillType == BattleSkill.Skill_2 && value.isLeftClick)
            {
                Debug.Log("스킬 2 발동");
            }
            else if (value.skillType == BattleSkill.Skill_3 && value.isLeftClick)
            {
                Debug.Log("스킬 3 발동");
            }
            else if (value.skillType == BattleSkill.Skill_4 && value.isLeftClick)
            {
                Debug.Log("스킬 4 발동");
            }
        }
        private void Nomal_Attack()
        {
            // 약공격을 한다. 
            playerAnimator.PlayAttack(true);

            Vector3 pos = transform.position;
            float dirY = transform.eulerAngles.y;

            C_AttackRequest attackRequest = new C_AttackRequest()
            {
                AttackerId = PlayerData.Instance.MyCharaceterData.id,
                PosX = pos.x,
                PosY = pos.y,
                PosZ = pos.z,
                DirY = dirY,
                TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                WeaponData = new WeaponData()
                {
                    Type = WeaponType.NoHaveWepon,
                    AttackType = AttackType.MeleeFist,
                    Damage = 1,
                    Range = 5,
                    Cooldown = 0.25f,
                }
            };

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Critical_Attack()
        {
            // 약공격을 한다. 
            playerAnimator.PlayAttack(false);
        }

    }

}
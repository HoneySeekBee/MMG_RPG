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
        public bool isLeftClick;       // ���콺 ��Ŭ��
        public BattleSkill skillType;       // ��� ���� ��ų ��ȣ (��: 1�� ��ų, 2�� ��ų)
    }
    public enum BattleSkill { None,  Skill_1, Skill_2, Skill_3, Skill_4 }

    public class BattleAction : ActionBase<BattleInputData>
    {
        private PlayerAnimator playerAnimator;

        public override void Initialize(bool isLocal, IInputBase input)
        {
            base.Initialize(isLocal, input);

            Debug.Log("[BattleAction] �ӽ÷� ���⼭ ������� �����ϱ� ");
            GameContextManager.SetContext(GameContext.Battle);

            playerAnimator = GetComponent<PlayerAnimator>();
        }

        protected override void Action(BattleInputData value)
        {
            string clickType = value.isLeftClick ? "��Ŭ��" : "��Ŭ��";
            string skillName = value.skillType.ToString();

            Debug.Log($"{skillName}�� {clickType} �߽��ϴ�.");
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
                Debug.Log("��ų ���");
            }
            else if (value.skillType == BattleSkill.Skill_1 && value.isLeftClick)
            {
                Debug.Log("��ų 1 �ߵ�");
            }
            else if (value.skillType == BattleSkill.Skill_2 && value.isLeftClick)
            {
                Debug.Log("��ų 2 �ߵ�");
            }
            else if (value.skillType == BattleSkill.Skill_3 && value.isLeftClick)
            {
                Debug.Log("��ų 3 �ߵ�");
            }
            else if (value.skillType == BattleSkill.Skill_4 && value.isLeftClick)
            {
                Debug.Log("��ų 4 �ߵ�");
            }
        }
        private void Nomal_Attack()
        {
            // ������� �Ѵ�. 
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
            // ������� �Ѵ�. 
            playerAnimator.PlayAttack(false);
        }

    }

}
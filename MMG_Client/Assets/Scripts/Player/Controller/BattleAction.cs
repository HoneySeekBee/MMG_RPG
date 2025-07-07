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
    public enum BattleSkill { None, Skill_1, Skill_2, Skill_3, Skill_4 }
    public enum AttackInputType { Normal, Strong, Skill_1, Skill_2, Skill_3, Skill_4 }

    public class BattleAction : ActionBase<BattleInputData>
    {
        private PlayerAnimator playerAnimator;
        [System.Serializable]
        public class SaveKeyWithAttackData
        {
            public AttackInputType InputType;
            public AttackData attackData;
        }
        [SerializeField] private List<SaveKeyWithAttackData> attackDatas;
        private Dictionary<AttackInputType, AttackData> attackDataDic = new Dictionary<AttackInputType, AttackData>();
        public override void Initialize(bool isLocal, IInputBase input)
        {
            base.Initialize(isLocal, input);

            Debug.Log("[BattleAction] �ӽ÷� ���⼭ ������� �����ϱ� ");
            GameContextManager.SetContext(GameContext.Battle);

            playerAnimator = GetComponent<PlayerAnimator>();

            foreach (var attackData in attackDatas)
            {
                attackDataDic.Add(attackData.InputType, attackData.attackData);
            }
        }

        protected override void Action(BattleInputData value)
        {
            string clickType = value.isLeftClick ? "��Ŭ��" : "��Ŭ��";
            string skillName = value.skillType.ToString();

            Debug.Log($"{skillName}�� {clickType} �߽��ϴ�.");
            if (value.skillType == BattleSkill.None && value.isLeftClick)
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
                Skill_Action(AttackInputType.Skill_1);
            }
            else if (value.skillType == BattleSkill.Skill_2 && value.isLeftClick)
            {
                Debug.Log("��ų 2 �ߵ�");
                Skill_Action(AttackInputType.Skill_2);
            }
            else if (value.skillType == BattleSkill.Skill_3 && value.isLeftClick)
            {
                Debug.Log("��ų 3 �ߵ�");
                Skill_Action(AttackInputType.Skill_3);
            }
            else if (value.skillType == BattleSkill.Skill_4 && value.isLeftClick)
            {
                Debug.Log("��ų 4 �ߵ�");
                Skill_Action(AttackInputType.Skill_4);
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
                AttackId = attackDataDic[AttackInputType.Normal].AttackId,
            };

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[AttackInputType.Normal];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Critical_Attack()
        {
            // ������� �Ѵ�. 
            playerAnimator.PlayAttack(false);

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
                AttackId = attackDataDic[AttackInputType.Strong].AttackId,
            };

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[AttackInputType.Strong];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Skill_Action(AttackInputType SkillType)
        {
            playerAnimator.PlayAttack(false); // ��ų �ִϸ��̼��� ���߿� ���� 

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
                AttackId = attackDataDic[SkillType].AttackId,
            };

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[SkillType];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
    }

}
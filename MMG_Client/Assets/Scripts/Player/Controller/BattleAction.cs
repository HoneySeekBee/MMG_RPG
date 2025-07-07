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

            Debug.Log("[BattleAction] 임시로 여기서 전투모드 설정하기 ");
            GameContextManager.SetContext(GameContext.Battle);

            playerAnimator = GetComponent<PlayerAnimator>();

            foreach (var attackData in attackDatas)
            {
                attackDataDic.Add(attackData.InputType, attackData.attackData);
            }
        }

        protected override void Action(BattleInputData value)
        {
            string clickType = value.isLeftClick ? "좌클릭" : "우클릭";
            string skillName = value.skillType.ToString();

            Debug.Log($"{skillName}을 {clickType} 했습니다.");
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
                Debug.Log("스킬 취소");
            }
            else if (value.skillType == BattleSkill.Skill_1 && value.isLeftClick)
            {
                Debug.Log("스킬 1 발동");
                Skill_Action(AttackInputType.Skill_1);
            }
            else if (value.skillType == BattleSkill.Skill_2 && value.isLeftClick)
            {
                Debug.Log("스킬 2 발동");
                Skill_Action(AttackInputType.Skill_2);
            }
            else if (value.skillType == BattleSkill.Skill_3 && value.isLeftClick)
            {
                Debug.Log("스킬 3 발동");
                Skill_Action(AttackInputType.Skill_3);
            }
            else if (value.skillType == BattleSkill.Skill_4 && value.isLeftClick)
            {
                Debug.Log("스킬 4 발동");
                Skill_Action(AttackInputType.Skill_4);
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
                AttackId = attackDataDic[AttackInputType.Normal].AttackId,
            };

            #region 공격 테스트 
            AttackData data = attackDataDic[AttackInputType.Normal];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Critical_Attack()
        {
            // 약공격을 한다. 
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

            #region 공격 테스트 
            AttackData data = attackDataDic[AttackInputType.Strong];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Skill_Action(AttackInputType SkillType)
        {
            playerAnimator.PlayAttack(false); // 스킬 애니메이션은 나중에 구현 

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

            #region 공격 테스트 
            AttackData data = attackDataDic[SkillType];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
    }

}
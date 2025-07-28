using AttackPacket;
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
    public enum AttackInputType { None, Normal, Strong, Skill_1, Skill_2, Skill_3, Skill_4 }

    public enum TargetType { Attacker, Damaged }
    public struct BattleData
    {
        public TargetType targetType;
        public int TargetId;
        public int attackTypeId;
    }
    [System.Serializable]
    public class SaveKeyWithAttackData
    {
        public AttackInputType InputType;
        public AttackData attackData;
        public int InputTypeInt => (int)InputType;
    }

    public class BattleAction : ActionBase<BattleInputData>
    {
        private CharacterAnimator playerAnimator;
       
        [SerializeField] private List<SaveKeyWithAttackData> attackDatas; // DB -> Server -> Client로 받아와야함 

        private Dictionary<AttackInputType, AttackData> attackDataDic = new Dictionary<AttackInputType, AttackData>();
        public override void Initialize(bool isLocal, IInputBase input = null)
        {
            base.Initialize(isLocal, input);

            Debug.Log("[BattleAction] 임시로 여기서 전투모드 설정하기 ");
            GameContextManager.SetContext(GameContext.Battle);

            playerAnimator = GetComponent<CharacterAnimator>();

        }
        public override void SetAttackData(List<SaveKeyWithAttackData> attackData)
        {
            Debug.Log($"[BattleAction] SetAttackData {attackData.Count} ");
            attackDatas = attackData;

            if (attackDatas != null && attackDatas.Count > 0)
            {

                foreach (var _attackData in attackDatas)
                {
                    attackDataDic.Add(_attackData.InputType, _attackData.attackData);
                }
            }
        }
        protected override void Action(BattleInputData value)
        {
            if (stopAction)
                return;
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
            playerAnimator.PlayAttack(true);
            // 약공격을 한다. 
            Vector3 pos = transform.position;
            float dirY = transform.eulerAngles.y;

            C_AttackData attackRequest = new C_AttackData()
            {
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
            bool isProjectile = data.AttackType == AttackType.Arrow;
            if (isProjectile)
            {
                Debug.Log("화살 공격");
                SpawnArrowProjectile(pos, dirY, data);
            }

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Critical_Attack()
        {
            playerAnimator.PlayAttack(false);
            Vector3 pos = transform.position;
            float dirY = transform.eulerAngles.y;

            C_AttackData attackRequest = new C_AttackData()
            {
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
            bool isProjectile = (data.AttackType == AttackType.Arrow);
            Debug.Log($"공격 타입 체크 {data.AttackType} {isProjectile}");
            if (isProjectile)
            {
                Debug.Log("화살 공격");
                SpawnArrowProjectile(pos, dirY, data);
            }


            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        private void Skill_Action(AttackInputType SkillType)
        {
            Vector3 pos = transform.position;
            float dirY = transform.eulerAngles.y;

            C_AttackData attackRequest = new C_AttackData()
            {
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
            bool isProjectile = data.AttackType == AttackType.Arrow;
            Debug.Log($"공격 타입 체크 {data.AttackType} || {attackRequest.PosX}");
            if (isProjectile)
            {
                Debug.Log("화살 공격");
                SpawnArrowProjectile(pos, dirY, data);
            }

            NetworkManager.Instance.Send_Attack(attackRequest);
        }
        public override void DoAction(BattleData battleData)
        {
            if (stopAction)
                return;
            if (battleData.targetType == TargetType.Damaged)
            {
                Debug.Log($"[BattleAction] DoAction : 데미지입은 애니메이션 하기 ");
                playerAnimator.GetDamaged();
            }
            else
            {
                Debug.Log($"[BattleAction] DoAction : 공격하는 애니메이션 하기 ");
                playerAnimator.PlayAttack(false);
            }
        }
        private void SpawnArrowProjectile(Vector3 origin, float dirY, AttackData data)
        {
            float rad = dirY * Mathf.Deg2Rad;
            Vector3 forward = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));

            GameObject arrowObj = ArrowPool.Instance.GetArrow();
            arrowObj.transform.position = origin;
            arrowObj.transform.rotation = Quaternion.LookRotation(forward);

            ArrowController arrow = arrowObj.GetComponent<ArrowController>();
            if (arrow != null)
            {
                arrow.Init(
                    attackerId: PlayerData.Instance.MyCharaceterData.id,
                    direction: forward,
                    speed: 10f,              // or data.Speed
                    maxDistance: data.Range,
                    damage: data.Damage,
                    onComplete: () => ArrowPool.Instance.ReturnArrow(arrowObj) // 사용 후 반환
                );
            }
        }
    }

}
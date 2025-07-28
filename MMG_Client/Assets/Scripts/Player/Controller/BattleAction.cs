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
        public bool isLeftClick;       // ���콺 ��Ŭ��
        public BattleSkill skillType;       // ��� ���� ��ų ��ȣ (��: 1�� ��ų, 2�� ��ų)
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
       
        [SerializeField] private List<SaveKeyWithAttackData> attackDatas; // DB -> Server -> Client�� �޾ƿ;��� 

        private Dictionary<AttackInputType, AttackData> attackDataDic = new Dictionary<AttackInputType, AttackData>();
        public override void Initialize(bool isLocal, IInputBase input = null)
        {
            base.Initialize(isLocal, input);

            Debug.Log("[BattleAction] �ӽ÷� ���⼭ ������� �����ϱ� ");
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
            playerAnimator.PlayAttack(true);
            // ������� �Ѵ�. 
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

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[AttackInputType.Normal];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion
            bool isProjectile = data.AttackType == AttackType.Arrow;
            if (isProjectile)
            {
                Debug.Log("ȭ�� ����");
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

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[AttackInputType.Strong];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion
            bool isProjectile = (data.AttackType == AttackType.Arrow);
            Debug.Log($"���� Ÿ�� üũ {data.AttackType} {isProjectile}");
            if (isProjectile)
            {
                Debug.Log("ȭ�� ����");
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

            #region ���� �׽�Ʈ 
            AttackData data = attackDataDic[SkillType];
            AttackGizmoPreview.Instance.Show(pos, dirY, data);
            #endregion
            bool isProjectile = data.AttackType == AttackType.Arrow;
            Debug.Log($"���� Ÿ�� üũ {data.AttackType} || {attackRequest.PosX}");
            if (isProjectile)
            {
                Debug.Log("ȭ�� ����");
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
                Debug.Log($"[BattleAction] DoAction : ���������� �ִϸ��̼� �ϱ� ");
                playerAnimator.GetDamaged();
            }
            else
            {
                Debug.Log($"[BattleAction] DoAction : �����ϴ� �ִϸ��̼� �ϱ� ");
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
                    onComplete: () => ArrowPool.Instance.ReturnArrow(arrowObj) // ��� �� ��ȯ
                );
            }
        }
    }

}
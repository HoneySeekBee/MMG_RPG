using GamePacket;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace MMG.UI
{
    public class InGamePopup : PopupBase
    {
        [Header("캐릭터 상태")]
        [SerializeField] private TMP_Text HP_Text;
        [SerializeField] private TMP_Text MP_Text;
        [SerializeField] private Animator HP_BarAnimator;
        [SerializeField] private Animator MP_BarAnimator;
        [SerializeField] private TMP_Text Level_Text;

        [Header("캐릭터 경험치")]
        [SerializeField] private TMP_Text EXP_Text;
        [SerializeField] private Animator EXP_Animator;

        [Header("캐릭터 스킬")]
        [SerializeField] private RectTransform SkillAreaRect;
        [SerializeField] private GameObject SkillIconPrefab;
        private Dictionary<AttackInputType, SkillIcon> SkillDictionary = new Dictionary<AttackInputType, SkillIcon>();

        [Header("몬스터 HP")]
        [SerializeField] private GameObject MonsterHPObject;
        [SerializeField] private TMP_Text Monster_Text;
        [SerializeField] private Animator Monster_Animator;
        [SerializeField] private RemoteMonster CurrentMonster;
        private float cnt;
        private const float SHOW_TIME = 3;
        private Coroutine CoMonsterHPTimer;

        Status myCharStatus => GameRoom.Instance.MyCharacter.StatInfo;
        public void Set(List<SaveKeyWithAttackData> attackDatas)
        {
            SetSkill(attackDatas);
            UnShowMonsterHP();
            StartCoroutine(Open_Status());
        }

        private void SetSkill(List<SaveKeyWithAttackData> attackDatas)
        {
            foreach (var skill in attackDatas)
            {
                GameObject skillIconObject = Instantiate(SkillIconPrefab, SkillAreaRect);
                SkillIcon skillIcon = skillIconObject.GetComponent<SkillIcon>();
                skillIcon.init(skill.InputType, null, skill.attackData.Cooldown);
                SkillDictionary.Add(skill.InputType, skillIcon);
            }
        }

        private IEnumerator Open_Status()
        {
            var waitSec = new WaitForSeconds(1f);
         
            while (true)
            {
                // 갱신 
                HP_Text.text = $"{myCharStatus.NowHP}/{myCharStatus.MaxHP}";
                MP_Text.text = $"{myCharStatus.NowMP}/{myCharStatus.MaxMP}";
                Level_Text.text = $"LV.{myCharStatus.Level}";
                EXP_Text.text = $"{myCharStatus.Exp}/100";

                SetBarProgress((myCharStatus.NowHP / myCharStatus.MaxHP), (myCharStatus.NowMP / myCharStatus.MaxMP), (myCharStatus.Exp / 100));

                yield return waitSec;
            }
        }
        public void SetBarProgress(float hpRatio, float mpRatio, float expRatio)
        {
            //Debug.Log($"HP {hpRatio} / MP {mpRatio} / EXP {expRatio}");
            HP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(hpRatio));
            MP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(mpRatio));
            EXP_Animator.SetFloat("BarFill", Mathf.Clamp01(expRatio));
        }
        public void ShowMonsterHP(RemoteMonster NowMonster)
        {
            if (CoMonsterHPTimer == null)
            {
                CurrentMonster = NowMonster;
                cnt = 0;
                CoMonsterHPTimer = StartCoroutine(MonsterHPTimer());
            }
            else
            {
                cnt = 0;
                if (CurrentMonster.Id != NowMonster.Id)
                {
                    StopCoroutine(CoMonsterHPTimer);
                    UnShowMonsterHP();
                    CurrentMonster = NowMonster;
                    CoMonsterHPTimer = StartCoroutine(MonsterHPTimer());
                }
            }
        }
        private void ShowMonsterHP()
        {
            MonsterHPObject.SetActive(true);
        }
        private void UnShowMonsterHP()
        {
            MonsterHPObject.SetActive(false);
        }
        private IEnumerator MonsterHPTimer()
        {
            ShowMonsterHP(); // SetActiveTrue
            var waitSec = new WaitForSeconds(0.1f);

            while (cnt < 3)
            {
                cnt += 0.1f;
                if (CurrentMonster == null)
                    break;
                Monster_Text.text = $"[{CurrentMonster.name} {CurrentMonster.HP}/{CurrentMonster.RemoteMonsterData._MaxHP}]";
                Monster_Animator.SetFloat("BarFill", Mathf.Clamp01(CurrentMonster.HP/ CurrentMonster.RemoteMonsterData._MaxHP));
                yield return waitSec;
            }
            UnShowMonsterHP();
            CoMonsterHPTimer = null;
        }

    }
}
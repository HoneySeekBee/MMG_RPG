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
        private Dictionary<KeyCode, SkillIcon> SkillDictionary = new Dictionary<KeyCode, SkillIcon>();

        public void SetSkill()
        {

        }

        private IEnumerator Open_Status()
        {
            var waitSec = new WaitForSeconds(1f);
            Status myCharStatus = GameRoom.Instance.MyCharacter.StatInfo;
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
            HP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(hpRatio));
            MP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(mpRatio));
            EXP_Animator.SetFloat("BarFill", Mathf.Clamp01(expRatio));
        }

    }

}
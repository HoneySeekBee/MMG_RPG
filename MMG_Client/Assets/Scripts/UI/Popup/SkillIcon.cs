using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MMG.UI
{
    public class SkillIcon : MonoBehaviour
    {
        [SerializeField] private TMP_Text InputKey_Text;
        [SerializeField] private Image SkillIcon_Image;
        private float coolTime;
        [SerializeField] private Image CoolTimeImage;
        public void init(AttackInputType inputType, Sprite skillIcon, float _coolTime)
        {
            InputKey_Text.text = KeyCode(inputType);
            SkillIcon_Image.sprite = skillIcon;
            coolTime = _coolTime;
        }
        private string KeyCode(AttackInputType inputType)
        {
            switch (inputType)
            {
                case AttackInputType.Normal:
                    return "LC";
                case AttackInputType.Strong:
                    return "RC";
                case AttackInputType.Skill_1:
                    return "1";
                case AttackInputType.Skill_2:
                    return "2";
                case AttackInputType.Skill_3:
                    return "3";
                case AttackInputType.Skill_4:
                    return "4";
                default:
                    return "";
            }
        }
        public void StartCoolTime()
        {
            StartCoroutine(ShowCoolTime());
        }
        private IEnumerator ShowCoolTime()
        {
            float time = 0;
            var waitTime = 0.25f;
            var wait = new WaitForSeconds(waitTime);
            while (time <= coolTime)
            {
                yield return wait;
                time += waitTime;
                CoolTimeImage.fillAmount = time / coolTime;
            }
        }
    }
}

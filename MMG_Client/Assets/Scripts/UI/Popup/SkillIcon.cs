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
        public SkillIcon(string InputKey, Sprite skillIcon, float _coolTime)
        {
            InputKey_Text.text = InputKey;
            SkillIcon_Image.sprite = skillIcon;
            coolTime = _coolTime;
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

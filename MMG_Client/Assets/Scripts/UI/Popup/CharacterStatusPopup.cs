using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GamePacket;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace MMG.UI
{
    public class CharacterStatusPopup : PopupBase
    {
        private bool isOpen;

        [Header("ĳ���� ����")]
        [SerializeField] private TMP_Text NickNameText;
        [SerializeField] private TMP_Text GenderText;
        [SerializeField] private TMP_Text ClassText;

        [Header("ĳ���� Status")]
        [SerializeField] private TMP_Text HP_Text;
        [SerializeField] private TMP_Text MP_Text;
        [SerializeField] private Animator HP_BarAnimator;
        [SerializeField] private Animator MP_BarAnimator;

        [SerializeField] private TMP_Text Level_Text;
        [SerializeField] private TMP_Text EXP_Text;
        [SerializeField] private Animator EXP_Animator;

        [SerializeField] private TMP_Text STR_Text;
        [SerializeField] private TMP_Text INT_Text;
        [SerializeField] private TMP_Text DEC_Text;
        [SerializeField] private TMP_Text LUK_Text;

        [SerializeField] private TMP_Text Gold_Text;

        /* ��� ���
         * PopupManager.Instance.Show<CharacterStatusPopup>((popup) => popup.Open());
         */
        public override void Open()
        {
            base.Open();
            isOpen = true;
            Set_CharacterData();
            StartCoroutine(Open_Status());
            StartCoroutine(ForceRebuild());
        }

        private IEnumerator ForceRebuild()
        {
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            //this.GetComponentInParent<Canvas>()
            RectTransform rect = this.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            foreach (var layout in rect.GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
            foreach (var fitter in rect.GetComponentsInChildren<ContentSizeFitter>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
            }
            Debug.Log("���� ���̾ƿ� ������ �Ϸ�");
        }
        /* ��� ���
         * PopupManager.Instance.UnShow<CharacterStatusPopup>((popup) => popup.Close());
         */
        public override void Close()
        {
            isOpen = false;
            base.Close();
        }
        public void SetBarProgress(float hpRatio, float mpRatio, float expRatio)
        {
            HP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(hpRatio));
            MP_BarAnimator.SetFloat("BarFill", Mathf.Clamp01(mpRatio));
            EXP_Animator.SetFloat("BarFill", Mathf.Clamp01(expRatio));
        }

        private void Set_CharacterData()
        {
            CharacterData myCharacterData = GameRoom.Instance.MyCharacter.RemoteCharaceterData;

            string Name = myCharacterData.characterName;
            Gender gender = myCharacterData.Gender;
            ClassType classType = myCharacterData.@class;

            NickNameText.text = Name;
            GenderText.text = gender == Gender.Man ? "����" : gender == Gender.Girl ? "����" : "����";

            string className;
            switch (classType)
            {
                case ClassType.NoHave:
                    className = "�ʺ���";
                    break;
                case ClassType.Knight:
                    className = "����";
                    break;
                case ClassType.Mage:
                    className = "������";
                    break;
                case ClassType.Archer:
                    className = "�ü�";
                    break;
                default:
                    className = "����";
                    break;
            }

            ClassText.text = className;
        }
        private IEnumerator Open_Status()
        {
            var waitSec = new WaitForSeconds(1.5f);
            Status myCharStatus = GameRoom.Instance.MyCharacter.StatInfo;
            while (isOpen)
            {
                // ���� 
                HP_Text.text = $"{myCharStatus.NowHP}/{myCharStatus.MaxHP}";
                MP_Text.text = $"{myCharStatus.NowMP}/{myCharStatus.MaxMP}";
                Level_Text.text = $"LV.{myCharStatus.Level}";
                EXP_Text.text = $"{myCharStatus.Exp}/100";

                STR_Text.text = "1";
                INT_Text.text = "1";
                DEC_Text.text = "1";
                LUK_Text.text = "1";

                Gold_Text.text = $"{myCharStatus.Gold} ���";

                SetBarProgress((myCharStatus.NowHP / myCharStatus.MaxHP), (myCharStatus.NowMP / myCharStatus.MaxMP), (myCharStatus.Exp / 100));

                yield return waitSec;
            }
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MMG.Quest
{
    public class QuestItem : MonoBehaviour
    {
        private Toggle _toggle;
        [SerializeField] private Image iconImage;
        // ����Ʈ�� ������ ��� �־����. 


        private void Start()
        {
            _toggle = this.GetComponent<Toggle>();
            _toggle.group = this.GetComponentInParent<ToggleGroup>();
        }


    }

}
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
        // 퀘스트의 정보를 담고 있어야함. 


        private void Start()
        {
            _toggle = this.GetComponent<Toggle>();
            _toggle.group = this.GetComponentInParent<ToggleGroup>();
        }


    }

}
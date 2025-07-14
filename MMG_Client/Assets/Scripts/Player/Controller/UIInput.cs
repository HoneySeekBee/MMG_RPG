using MMG;
using MMG.UI;
using Ricimi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MMG
{
    public class UIInput : MonoBehaviour
    {
        bool isOpen = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                StatusOpen();
            }
        }

        public void StatusOpen()
        {
            if (isOpen)
            {
                PopupManager.Instance.UnShow<CharacterStatusPopup>((popup) =>
                {
                    popup.Close();
                });
                isOpen = false;
            }
            else
            {
                PopupManager.Instance.Show<CharacterStatusPopup>((popup) =>
                {
                    popup.Open();
                });
                isOpen = true;
            }
        }
    }
}
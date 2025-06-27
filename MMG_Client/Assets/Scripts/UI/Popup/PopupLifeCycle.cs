using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG.UI
{
    public class PopupLifeCycle : MonoBehaviour
    {
        public Action onClosed;

        // Animator Close �ִϸ��̼� ���� �� ȣ��
        public void OnCloseComplete()
        {
            onClosed?.Invoke();
        }
    }
}

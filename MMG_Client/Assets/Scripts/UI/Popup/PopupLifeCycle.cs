using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG.UI
{
    public class PopupLifeCycle : MonoBehaviour
    {
        public Action onClosed;

        // Animator Close 애니메이션 끝날 때 호출
        public void OnCloseComplete()
        {
            onClosed?.Invoke();
        }
    }
}

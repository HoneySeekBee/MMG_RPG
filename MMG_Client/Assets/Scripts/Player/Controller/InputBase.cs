using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public abstract class InputBase<T> : MonoBehaviour, IInputBase
    {
        [Header("Key bindings")]
        [SerializeField] protected List<MMG_KeyCode> MMG_KeyCodes = new List<MMG_KeyCode>();
        [SerializeField] protected Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

        public event Action<T> InputAction; 
        
        public void InvokeRaw()
        {
            // 여기서 기본값으로 호출하거나, 추상 오버라이드 가능
            InvokeAction(default(T));
        }
        protected virtual void Initialize()
        {
            foreach (var key in MMG_KeyCodes)
            {
                if (keys.ContainsKey(key.KeyId) == false)
                    keys.Add(key.KeyId, key.KeyCode);
            }
        }
        // Update를 호출하는 책임은 Base에서 담당
        protected virtual void Update()
        {
            CheckInput();
        }

        // 자식 클래스에서 구체적인 입력 처리 구현
        protected abstract void CheckInput();
        protected void InvokeAction(T value)
        {
            InputAction?.Invoke(value);
        }
    }
    [Serializable]
    public class MMG_KeyCode
    {
        public string KeyId;
        public KeyCode KeyCode;
    }
    public struct Unit { }
}

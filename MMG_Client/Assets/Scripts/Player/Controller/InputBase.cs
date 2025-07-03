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
            // ���⼭ �⺻������ ȣ���ϰų�, �߻� �������̵� ����
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
        // Update�� ȣ���ϴ� å���� Base���� ���
        protected virtual void Update()
        {
            CheckInput();
        }

        // �ڽ� Ŭ�������� ��ü���� �Է� ó�� ����
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

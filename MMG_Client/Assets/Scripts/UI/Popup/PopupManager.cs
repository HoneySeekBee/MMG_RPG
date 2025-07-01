using Ricimi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MMG.UI
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        [SerializeField] private Transform popupParent;


        private Dictionary<Type, PopupBase> _activePopups = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }
        public async void Show<T>(Action<T> onInit = null) where T : PopupBase
        {
            Type popupType = typeof(T);

            // �̹� �� �ִ� �˾��̸� ���� or ����
            if (_activePopups.ContainsKey(popupType))
            {
                Debug.LogWarning($"{popupType.Name} �˾��� �̹� ���� �ֽ��ϴ�.");
                return;
            }

            string addressKey = popupType.Name;

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(addressKey);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[PopupManager] Failed to load popup: {addressKey}");
                return;
            }

            GameObject popupGO = Instantiate(handle.Result, popupParent);
            T popup = popupGO.GetComponent<T>();

            if (popup == null)
            {
                Debug.LogError($"[PopupManager] {addressKey} �����տ� {popupType.Name} ������Ʈ�� �����ϴ�.");
                Destroy(popupGO);
                return;
            }

            _activePopups.Add(popupType, popup);
            popup.Open();
            onInit?.Invoke(popup);

            // �˾��� ������ Dictionary���� ����
            popupGO.GetComponent<PopupLifeCycle>().onClosed += () =>
            {
                _activePopups.Remove(popupType);
            };
        }
        public void UnShow<T>() where T : PopupBase
        {
            Type popupType = typeof(T);

            if (_activePopups.TryGetValue(popupType, out PopupBase popup))
            {
                popup.Close(); // �Ǵ� popup.gameObject.SetActive(false); ��
                _activePopups.Remove(popupType);
            }
        }
    }
}
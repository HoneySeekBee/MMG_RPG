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

            // 이미 떠 있는 팝업이면 재사용 or 무시
            if (_activePopups.ContainsKey(popupType))
            {
                Debug.LogWarning($"{popupType.Name} 팝업이 이미 열려 있습니다.");
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
                Debug.LogError($"[PopupManager] {addressKey} 프리팹에 {popupType.Name} 컴포넌트가 없습니다.");
                Destroy(popupGO);
                return;
            }

            _activePopups.Add(popupType, popup);
            popup.Open();
            onInit?.Invoke(popup);

            // 팝업이 닫히면 Dictionary에서 제거
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
                popup.Close(); // 또는 popup.gameObject.SetActive(false); 등
                _activePopups.Remove(popupType);
            }
        }
    }
}
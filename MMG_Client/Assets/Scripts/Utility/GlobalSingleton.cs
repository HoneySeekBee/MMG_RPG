using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GlobalSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _isQuitting;

    public static T Instance
    {
        get
        {
            if (_isQuitting)
                return null;

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject($"[GlobalSingleton] {typeof(T)}");
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _isQuitting = true;
    }
}

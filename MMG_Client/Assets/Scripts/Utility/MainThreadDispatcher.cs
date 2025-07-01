using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _actions = new Queue<Action>();

    public static void RunOnMainThread(Action action)
    {
        Debug.Log("OnMainThread");
        lock (_actions)
        {
            _actions.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (_actions)
        {
            while (_actions.Count > 0)
                _actions.Dequeue().Invoke();
        }
    }
}

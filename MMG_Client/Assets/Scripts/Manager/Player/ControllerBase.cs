using MMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerBase<TActionType, TEvent> : MonoBehaviour
    where TActionType : System.Enum
    where TEvent : ICharacterEvent<TActionType>
{
    public List<TEvent> Events = new();
    protected Dictionary<TActionType, TEvent> EventDict = new();

    public virtual void Initialize(bool isLocal)
    {
        foreach (var evt in Events)
        {
            if (!EventDict.ContainsKey(evt.ActionType))
                EventDict.Add(evt.ActionType, evt);

            var input = null as IInputBase;
            if (evt.Input != null)
                input = evt.Input as IInputBase;
            var action = evt.Action as IActionBase;
            action.Initialize(isLocal, input);
        }
    }


    public void SetMove(Vector3 goalPos, float dirY, float speed)
    {
        if (EventDict.TryGetValue((TActionType)(object)System.Enum.Parse(typeof(TActionType), "move"), out var evt))
        {
            var action = evt.Action as IActionBase;
            action.SetMove(goalPos, dirY, speed);
        }
    }

    public void SetAttack(BattleData battleData)
    {
        if (EventDict.TryGetValue((TActionType)(object)System.Enum.Parse(typeof(TActionType), "battle"), out var evt))
        {
            var action = evt.Action as IActionBase;
            action.DoAction(battleData);
        }
    }
}
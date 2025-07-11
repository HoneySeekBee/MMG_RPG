using MMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MonsterActionType
{
    move,
    battle,
    idle,
    // �߰� ����
}

[System.Serializable]
public class MonsterEvent : ICharacterEvent<MonsterActionType>
{
    public string eventName;
    public MonsterActionType actionType;
    public MonoBehaviour input;
    public MonoBehaviour action;

    public string EventName => eventName;
    public MonsterActionType ActionType => actionType;
    public MonoBehaviour Input => input;
    public MonoBehaviour Action => action;
}
public class MonsterController : ControllerBase<MonsterActionType, MonsterEvent>
{
    public override void Initialize(bool isLocal = false)
    {
        base.Initialize(false); // ���ʹ� �׻� Remote��

    }
    public void Init_Position(Vector3 position, float dirY)
    {
        var action = EventDict[MonsterActionType.move].action as IActionBase;
        action.Init_Position(position, dirY);

    }
}
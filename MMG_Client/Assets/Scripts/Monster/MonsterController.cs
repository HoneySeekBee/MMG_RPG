using MMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MonsterActionType
{
    move,
    battle,
    idle,
    // 추가 가능
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
    public MonsterAnimator monsterAnimator;
    public override void Initialize(bool isLocal = false)
    {
        monsterAnimator = GetComponent<MonsterAnimator>();
        base.Initialize(false); // 몬스터는 항상 Remote로

    }
    public override void Init_Position(Vector3 position, float dirY)
    {
        var action = EventDict[MonsterActionType.move].action as IActionBase;
        action.Init_Position(position, dirY);
    }

    public void Init_AttackData(List<SaveKeyWithAttackData> attackData)
    {
        var action = EventDict[MonsterActionType.battle].action as IActionBase;
        action.SetAttackData(attackData);
    }
}
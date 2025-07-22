using MMG;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterEvent : ICharacterEvent<PlayerActionType>
{
    public string eventName;
    public PlayerActionType actionType;
    public MonoBehaviour input;
    public MonoBehaviour action;

    public string EventName => eventName;
    public PlayerActionType ActionType => actionType;
    public MonoBehaviour Input => input;
    public MonoBehaviour Action => action;
}

public enum PlayerActionType
{
    move,
    battle,

}
public class PlayerController : ControllerBase<PlayerActionType, CharacterEvent>
{
    [SerializeField] private bool _isLocalPlayer = false;
    public bool isLocalPlayer => _isLocalPlayer;
    public PlayerAnimator animator;
    private void Awake()
    {
        // ÃÊ±âÈ­ (if needed)

        animator = GetComponent<PlayerAnimator>();
    }

    private void OnDestroy()
    {
        Debug.Log($"PlayerController ÆÄ±«µÊ");
    }

    public override void Initialize(bool isLocal)
    {
        _isLocalPlayer = isLocal;
        if (_isLocalPlayer)
            this.tag = "Player";

        Debug.Log($"[PlayerController] {GameRoom.Instance.MyCharacter == null}");
        base.Initialize(isLocal);
    }
    public void Init_AttackData(List<SaveKeyWithAttackData> attackData)
    {
        var action = EventDict[PlayerActionType.battle].action as IActionBase;
        action.SetAttackData(attackData);
    }
    public override void Init_Position(Vector3 position, float dirY)
    {
        var action = EventDict[PlayerActionType.move].action as IActionBase;
        action.Init_Position(position, dirY);
    }
}

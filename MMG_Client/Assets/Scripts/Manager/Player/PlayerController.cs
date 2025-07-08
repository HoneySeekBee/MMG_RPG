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

    private void Awake()
    {
        // ÃÊ±âÈ­ (if needed)
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

        base.Initialize(isLocal);
    }
}

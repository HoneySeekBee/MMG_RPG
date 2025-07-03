using MMG;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterEvent
{
    public string eventName;
    public PlayerActionType actionType;
    public MonoBehaviour input;
    public MonoBehaviour action;
}
public enum PlayerActionType
{
    move,
    attack,

}
public class PlayerController : MonoBehaviour
{
    public List<CharacterEvent> characterEvents = new List<CharacterEvent>();
    public Dictionary<PlayerActionType, CharacterEvent> eventDictionary = new Dictionary<PlayerActionType, CharacterEvent>();
    [SerializeField] private bool _isLocalPlayer = false;
    public bool isLocalPlayer { get { return _isLocalPlayer; } }

    private void Awake()
    {
        foreach (CharacterEvent characterEvent in characterEvents)
        {
            if (eventDictionary.ContainsKey(characterEvent.actionType) == false)
                eventDictionary.Add(characterEvent.actionType, characterEvent);
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"파괴되었습니다.");
    }

    public void Initialize(bool isLocal)
    {
        _isLocalPlayer = isLocal;
        if (_isLocalPlayer)
        {
            this.tag = "Player";
            //InputManager.Instance.localController = this;
        }
        foreach (var item in characterEvents)
        {
            var input = item.input as IInputBase;
            var action = item.action as IActionBase;
            action.Initialize(isLocal, input);
        }
    }
    public void SetMove(Vector3 goalPos, float dirY, float speed)
    {
        var action = eventDictionary[PlayerActionType.move].action as IActionBase;
        action.SetMove(goalPos, dirY, speed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterEvent<T>
{
    string EventName { get; }
    T ActionType { get; }
    MonoBehaviour Input { get; }
    MonoBehaviour Action { get; }
}
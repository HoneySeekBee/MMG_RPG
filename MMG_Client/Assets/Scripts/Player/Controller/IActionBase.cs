using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionBase
{
    void Initialize(bool isLocal, IInputBase input);
    void SetMove(Vector3 goalPos, float dirY, float speed);
}

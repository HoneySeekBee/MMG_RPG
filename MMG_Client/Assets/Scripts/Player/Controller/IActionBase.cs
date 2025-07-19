using MMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MMG.BattleAction;

public interface IActionBase
{
    void Initialize(bool isLocal, IInputBase input);
    void Init_Position(Vector3 pos, float dirY);
    void SetMove(Vector3 goalPos, float dirY, float speed);
    void DoAction(BattleData battleData);
    void SetAttackData(List<SaveKeyWithAttackData> attackData);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MMG
{
    public abstract class ActionBase<T> : MonoBehaviour, IActionBase
    {
        protected bool IsLocal;

        public virtual void DoAction(BattleData battleData)
        {

        }

        public virtual void Initialize(bool isLocal, IInputBase input = null)
        {
            IsLocal = isLocal;
            if (input != null && input is InputBase<T> typedInput && IsLocal)
            {
                typedInput.InputAction += Action;
            }
        }

        public virtual void Init_Position(Vector3 pos, float dirY)
        {

        }

        public abstract void SetAttackData(List<SaveKeyWithAttackData> attackData);

        public virtual void SetMove(Vector3 goalPos, float dirY, float speed)
        {

        }

        protected abstract void Action(T value);

    }
}
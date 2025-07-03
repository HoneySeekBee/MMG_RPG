using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MMG
{
    public abstract class ActionBase<T> : MonoBehaviour, IActionBase
    {
        protected bool IsLocal;
        public virtual void Initialize(bool isLocal, IInputBase input)
        {
            IsLocal = isLocal;
            if (input is InputBase<T> typedInput && IsLocal)
            {
                typedInput.InputAction += Action;
            }
        }

        public virtual void SetMove(Vector3 goalPos, float dirY, float speed)
        {

        }

        protected abstract void Action(T value);
    }
}
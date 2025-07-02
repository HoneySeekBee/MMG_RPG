using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : GlobalSingleton<InputManager>
{
    public static event Action<Vector2> OnMoveInput;
    public static event Action OnAttackInput;
    public static event Action OnRuninputDown;
    public static event Action OnRuninputUp;
    public PlayerController localController;

    void Update()
    {
        if (localController == null)
            return;
        // 로컬 캐릭터만 적용되게 한다. 

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (move.sqrMagnitude > 0.01f)
            OnMoveInput?.Invoke(move);

        if (Input.GetKeyDown(KeyCode.Space))
            OnAttackInput?.Invoke();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            OnRuninputDown?.Invoke();

        if (Input.GetKeyUp(KeyCode.LeftShift))
            OnRuninputUp?.Invoke();
    }
    public void Initialize()
    {
        OnMoveInput = null;
        OnAttackInput = null;
        OnRuninputDown = null;
        OnRuninputUp = null;
        localController = null;
    }
}

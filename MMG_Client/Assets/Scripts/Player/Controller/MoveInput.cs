using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public struct MoveData
{
    public Vector2 Direction;
    public bool IsRunning;
}
namespace MMG
{
    public class MoveInput : InputBase<MoveData>
    {
        MoveData moveData = new MoveData();
        private void Start()
        {
            Initialize();
        }
        protected override void Initialize()
        {
            MMG_KeyCodes = new List<MMG_KeyCode>();
            MMG_KeyCode leftKey = new MMG_KeyCode() { KeyId = "left", KeyCode = KeyCode.A };
            MMG_KeyCode rightkey = new MMG_KeyCode() { KeyId = "right", KeyCode = KeyCode.D };
            MMG_KeyCode forwardKey = new MMG_KeyCode() { KeyId = "forward", KeyCode = KeyCode.W };
            MMG_KeyCode backwardKey = new MMG_KeyCode() { KeyId = "backward", KeyCode = KeyCode.S };
            MMG_KeyCode leftShiftKey = new MMG_KeyCode() { KeyId = "leftShift", KeyCode = KeyCode.LeftShift };
            MMG_KeyCodes.Add(leftKey);
            MMG_KeyCodes.Add(rightkey);
            MMG_KeyCodes.Add(forwardKey);
            MMG_KeyCodes.Add(backwardKey);
            MMG_KeyCodes.Add(leftShiftKey);
            base.Initialize();
        }
        protected override void CheckInput()
        {
            float x = 0f;
            float y = 0f;
            bool isRunning = false;

            if (keys.TryGetValue("left", out var leftKey) && Input.GetKey(leftKey))
                x -= 1f;
            if (keys.TryGetValue("right", out var rightKey) && Input.GetKey(rightKey))
                x += 1f;
            if (keys.TryGetValue("forward", out var forwardKey) && Input.GetKey(forwardKey))
                y += 1f;
            if (keys.TryGetValue("backward", out var backwardKey) && Input.GetKey(backwardKey))
                y -= 1f;
            if (keys.TryGetValue("leftShift", out var shiftKey) && Input.GetKey(shiftKey))
                isRunning = true;
            else
                isRunning = false;

            Vector2 move = new Vector2(x, y);

            if (move.sqrMagnitude > 0.01f)
            {
                moveData.Direction = move.normalized;
                moveData.IsRunning = isRunning;
                if (isRunning)
                    Debug.Log("달리는 중 ");
                InvokeAction(moveData); // 정규화된 방향값 전달
            }
        }

    }
}
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
            AddKeycode("left", KeyCode.A);
            AddKeycode("right", KeyCode.D);
            AddKeycode("forward", KeyCode.W);
            AddKeycode("backward", KeyCode.S);
            AddKeycode("leftShift", KeyCode.LeftShift);
            base.Initialize();
        }
        protected override void CheckInput()
        {
            if (ChatContentUI.Instance != null && ChatContentUI.Instance.isActiveInputField)
                return;


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

                InvokeAction(moveData); // 정규화된 방향값 전달
            }
        }

    }
}
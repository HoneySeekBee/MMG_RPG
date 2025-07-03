using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MMG
{
    public class MoveInput : InputBase<Vector2>
    {
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
            MMG_KeyCodes.Add(leftKey);
            MMG_KeyCodes.Add(rightkey);
            MMG_KeyCodes.Add(forwardKey);
            MMG_KeyCodes.Add(backwardKey);
            base.Initialize();
        }
        protected override void CheckInput()
        {
            float x = 0f;
            float y = 0f;

            if (keys.TryGetValue("left", out var leftKey) && Input.GetKey(leftKey))
                x -= 1f;
            if (keys.TryGetValue("right", out var rightKey) && Input.GetKey(rightKey))
                x += 1f;
            if (keys.TryGetValue("forward", out var forwardKey) && Input.GetKey(forwardKey))
                y += 1f;
            if (keys.TryGetValue("backward", out var backwardKey) && Input.GetKey(backwardKey))
                y -= 1f;

            Vector2 move = new Vector2(x, y);

            if (move.sqrMagnitude > 0.01f)
            {
                InvokeAction(move.normalized); // 정규화된 방향값 전달
            }
        }
    }
}
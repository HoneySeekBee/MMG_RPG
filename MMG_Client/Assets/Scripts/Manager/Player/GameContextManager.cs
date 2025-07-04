using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public class GameContextManager
    {
        public static GameContext Current { get; private set; } = GameContext.None;

        public static void SetContext(GameContext context)
        {
            Current = context;
            Debug.Log("Context º¯°æµÊ: " + context);
        }

        public static bool Is(GameContext context)
        {
            return Current == context;
        }
    }
    public enum GameContext
    {
        None,
        Battle,
        Talk,
        UI,
        Shop,
        Inventory
    }
}
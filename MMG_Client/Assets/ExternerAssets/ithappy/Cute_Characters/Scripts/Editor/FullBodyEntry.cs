using System;
using UnityEngine;

namespace CharacterCustomizationTool.Editor
{
    [CreateAssetMenu(menuName = "Character Customization Tool/Full Body Entry", fileName = "FullBodyEntry")]
    public class FullBodyEntry : ScriptableObject
    {
        public FullBodySlotEntry[] Slots;
    }

    [Serializable]
    public class FullBodySlotEntry
    {
        public SlotType Type;
        public GameObject GameObject;
    }
}
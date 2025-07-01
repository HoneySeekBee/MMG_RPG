using JetBrains.Annotations;
using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectPopup : PopupBase
{
    [SerializeField] private CharacterSelectButton[] characterSelectButtons;

    public void Init()
    {
        var previewManager = PreviewManager.Instance;
        // ������ �� ������ �������� 
        for (int i = 0; i < characterSelectButtons.Length; i++)
        {
            var button = characterSelectButtons[i];
            int id = button.Id;

            if (previewManager.HasPlayer(id))
            {
                var playerInfo = previewManager.GetPlayerInfo(id);
                var texture = previewManager.GetTexture(id);

                Debug.Log($"Slot {i}: {playerInfo.characterName} / {playerInfo.@class}");
                button.Set(true, playerInfo, texture);
            }
            else
            {
                button.Set(false);
            }
        }
    }
}

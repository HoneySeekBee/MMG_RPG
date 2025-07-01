using JetBrains.Annotations;
using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectPopup : PopupBase
{
    [SerializeField] private CharacterSelectButton[] characterSelectButtons;
    PreviewManager previewManager;

    public void Init()
    {
        previewManager = PreviewManager.Instance;
        // ������ �� ������ �������� 
        for (int i = 0; i < characterSelectButtons.Length; i++)
        {
            int id = characterSelectButtons[i].Id;

            bool hasPlayer = previewManager.HasPlayer(id);

            if (hasPlayer)
            {
                CharacterData thisCharacter = previewManager.GetPlayerInfo(id);
                Debug.Log($"Slot��ȣ {i} : �̸� {thisCharacter.characterName} : ���� {thisCharacter.@class}");
                characterSelectButtons[i].Set(hasPlayer,
                    previewManager.GetPlayerInfo(id), previewManager.GetTexture(id));

            }
            else
            {
                characterSelectButtons[i].Set(hasPlayer);
            }
        }
    }
}

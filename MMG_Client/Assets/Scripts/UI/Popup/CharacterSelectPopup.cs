using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectPopup : PopupBase
{
    [SerializeField] private CharacterSelectButton[] characterSelectButtons;
    PreviewManager previewManager;

    private void OnEnable()
    {
        previewManager = PreviewManager.Instance;
        // 가지고 온 정보를 바탕으로 
       for(int i = 0; i < characterSelectButtons.Length; i++)
        {
            int id = characterSelectButtons[i].Id;
            bool hasPlayer = previewManager.HasPlayer(id);
            if (hasPlayer)
            {
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

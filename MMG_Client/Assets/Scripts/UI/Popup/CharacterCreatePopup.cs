using DevionGames.InventorySystem;
using MMG.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class CharacterCreatePopup : PopupBase
{
    public static CharacterCreatePopup Instance { get; private set; }

    [SerializeField] private PartItemChangeButton[] partItemButtons;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        CreateCharacterManager.Instance.Initialize();
        SetGender(Gender.Man);
    }

    public void SetGender(Gender gender)
    {
        CreateCharacterManager.Instance.SetGender(gender);
        UpdateAllButtonNames();
    }
    public void Toggle_Gender_Man()
    {
        SetGender(Gender.Man);
    }
    public void Toggle_Gender_Gril()
    {
        SetGender(Gender.Girl);
    }
    public void OnClickNext(EquipSlot slot)
    {
        CreateCharacterManager.Instance.CycleNext(slot);
        UpdateButtonName(slot);
    }

    public void OnClickPrev(EquipSlot slot)
    {
        CreateCharacterManager.Instance.CyclePrev(slot);
        UpdateButtonName(slot);
    }

    private void UpdateAllButtonNames()
    {
        foreach (var button in partItemButtons)
            UpdateButtonName(button.ThisSlot);
    }

    private void UpdateButtonName(EquipSlot slot)
    {
        var item = CreateCharacterManager.Instance.GetCurrentItem(slot);
        foreach (var button in partItemButtons)
        {
            if (button.ThisSlot == slot)
                button.SetButtonsName(item.Name);
        }
    }
}

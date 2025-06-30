using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartItemChangeButton : MonoBehaviour
{
    public EquipSlot ThisSlot;
    [SerializeField] private TMP_Text itemName;

    public void OnClickNext()
    {
        CreateCharacterManager.Instance.CycleNext(ThisSlot);
        UpdateName();
    }

    public void OnClickPrev()
    {
        CreateCharacterManager.Instance.CyclePrev(ThisSlot);
        UpdateName();
    }

    public void UpdateName()
    {
        var item = CreateCharacterManager.Instance.GetCurrentItem(ThisSlot);
        itemName.text = item.Name;
    }

    public void SetButtonsName(string name)
    {
        itemName.text = name;
    }
}

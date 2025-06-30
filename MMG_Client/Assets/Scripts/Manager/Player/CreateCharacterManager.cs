using MMG.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateCharacterManager : MonoBehaviour
{
    public static CreateCharacterManager Instance { get; private set; }
    public CharacterAppearance createCharacter;

    [SerializeField] private DefaultItemSet defaultItemSet;
    private Dictionary<EquipSlot, List<CharacterPartItem>> availableParts;
    private Dictionary<EquipSlot, List<CharacterPartItem>> filteredParts;
    private Dictionary<EquipSlot, int> currentIndexes;
    public Gender currentGender = Gender.Man;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
    }
    public void Initialize()
    {
        availableParts = new();
        filteredParts = new();
        currentIndexes = new();

        foreach (var item in defaultItemSet.defaultItems)
        {
            if (!availableParts.ContainsKey(item.equipSlot))
                availableParts[item.equipSlot] = new List<CharacterPartItem>();

            availableParts[item.equipSlot].Add(item);
        }

        // 부위별 기본 아이템 적용
        foreach (var kvp in availableParts)
        {
            var item = kvp.Value[0];
            createCharacter.Equip(item);
        }
        ApplyDefaultParts();
    }
    public void ShowCharacterCreatePopup()
    {
        PopupManager.Instance.Show<CharacterCreatePopup>();
    }

    public void SetGender(Gender gender)
    {
        Debug.Log($"성별? {gender.ToString()}");
        currentGender = gender; 
        createCharacter.MyCharacterGender = gender;
        FilterPartsByGender();
        ApplyDefaultParts();
    }
    public void CycleNext(EquipSlot slot)
    {
        if (!currentIndexes.ContainsKey(slot))
            currentIndexes[slot] = 0;

        currentIndexes[slot]++;
        if (currentIndexes[slot] >= filteredParts[slot].Count)
            currentIndexes[slot] = 0;
        createCharacter.Equip(GetCurrentItem(slot));
    }
    public void CyclePrev(EquipSlot slot)
    {
        if (!currentIndexes.ContainsKey(slot))
            currentIndexes[slot] = 0;

        currentIndexes[slot]--;
        if (currentIndexes[slot] < 0)
            currentIndexes[slot] = filteredParts[slot].Count - 1;
        createCharacter.Equip(GetCurrentItem(slot));
    }

    public CharacterPartItem GetCurrentItem(EquipSlot slot)
    {
        return filteredParts[slot][currentIndexes[slot]];
    }
    private void ApplyDefaultParts()
    {
        foreach (var kvp in filteredParts)
        {
            if (kvp.Value.Count > 0)
            {
                var item = kvp.Value[0];
                createCharacter.Equip(item);
                currentIndexes[kvp.Key] = 0;
            }
        }
    }
    private void FilterPartsByGender()
    {
        filteredParts.Clear();

        foreach (var kvp in availableParts)
        {
            List<CharacterPartItem> filtered = kvp.Value.FindAll(part =>
                part.Gender == Gender.Both || part.Gender == currentGender);

            filteredParts[kvp.Key] = filtered;
        }
        currentIndexes = filteredParts.ToDictionary(kvp => kvp.Key, kvp => 0);
    }
}

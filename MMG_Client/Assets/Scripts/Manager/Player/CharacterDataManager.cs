using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataManager : GlobalSingleton<CharacterDataManager>
{

    [SerializeField] private MMGItemDatabase MMGItemSet; 
    private Dictionary<string, CharacterPartItem> itemById = new();

    private void Start()
    {

        Initialize();
    }

    private void Initialize()
    {
        foreach (var item in MMGItemSet.Total_PartItem)
        {
            itemById[item.itemId] = item;
        }
    }
    public bool TryGetItemById(string id, out CharacterPartItem item)
    {
        return itemById.TryGetValue(id, out item);
    }
    public List<CharacterPartItem> GetDefaultItem()
    {
        return MMGItemSet.Default_PartItem;
    }

}

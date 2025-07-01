using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDataManager : MonoBehaviour
{
    public static CharacterDataManager Instance { get; private set; }

    [SerializeField] private MMGItemDatabase MMGItemSet; 
    private Dictionary<string, CharacterPartItem> itemById = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

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

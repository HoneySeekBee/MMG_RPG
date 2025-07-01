using DevionGames.InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MMG/CustomItemDatabase")]
public class MMGItemDatabase : ItemDatabase
{
    public List<CharacterPartItem> Total_PartItem = new List<CharacterPartItem>();
    public List<CharacterPartItem> Default_PartItem = new List<CharacterPartItem>();
    public CharacterPartItem GetPartItemById(string itemId)
    {
        foreach (Item item in this.Total_PartItem) // this.items는 Devion의 ItemDatabase에서 상속
        {
            if (item is CharacterPartItem part && part.itemId == itemId)
            {
                return part;
            }
        }

        Debug.LogWarning($"Item with ID '{itemId}' not found in ItemDatabase.");
        return null;
    }
}

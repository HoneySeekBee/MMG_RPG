using DevionGames.InventorySystem;
using UnityEngine;

[CreateAssetMenu(menuName = "MMG/Items/EquippableItem", fileName = "EquippableItem_")]
public class EquippableItem : Item
{
    [Header("Preview Prefab")]
    public GameObject previewPrefab;
}
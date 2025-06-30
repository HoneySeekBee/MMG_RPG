using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.InventorySystem;

[CreateAssetMenu(menuName = "MMG/Items/CharacterPartItem", fileName = "Item_")]
public class CharacterPartItem : EquippableItem
{
    public string itemId; // 서버 통신용
    public Gender Gender; // 얼굴, 헤어 등
    public EquipSlot equipSlot; // 얼굴, 헤어 등

    public Mesh mesh;
    public Material material;
}
public enum Gender
{
    Both,
    Man,
    Girl
}
public enum EquipSlot
{
    Skin,
    Ear,

    Hair,
    Face,

    Hat,
    Costumes,
    Top,
    Bottom,

    Gloves,
    Shoes,

    FaceAccessory,
    Glasses,
}

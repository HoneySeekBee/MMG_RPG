using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.InventorySystem;

[CreateAssetMenu(menuName = "MMG/Items/CharacterPartItem", fileName = "Item_")]
public class CharacterPartItem : EquippableItem
{
    public string itemId; // ���� ��ſ�
    public Gender Gender; // ��, ��� ��
    public EquipSlot equipSlot; // ��, ��� ��

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

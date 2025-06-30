using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAppearance : MonoBehaviour
{
    [SerializeField] private Gender myCharacterGender;
    public Gender MyCharacterGender {get{ return myCharacterGender; } set{ myCharacterGender = value; } }

    private Dictionary<EquipSlot, CharacterPartItem> equippedParts = new Dictionary<EquipSlot, CharacterPartItem>();

    [Header("Skinned Mesh Renderers")]
    public SkinnedMeshRenderer SkinRenderer;
    public SkinnedMeshRenderer EarRenderer;

    public SkinnedMeshRenderer HairRenderer;
    public SkinnedMeshRenderer FacesRenderer;

    public SkinnedMeshRenderer HatRenderer;
    public SkinnedMeshRenderer CostumesRenderer; // 이게 활성화 되면 상의 하의 비활성화
    public SkinnedMeshRenderer ShirtsRenderer;
    public SkinnedMeshRenderer PantsRenderer;

    public SkinnedMeshRenderer GlovesRenderer;
    public SkinnedMeshRenderer ShoesRenderer;

    public SkinnedMeshRenderer FaceAccessoriesRenderer;
    public SkinnedMeshRenderer GlassesRenderer;
    public void Equip(CharacterPartItem item)
    {
        if (item == null || !IsCompatible(item))
        {
            return;
        }

        SkinnedMeshRenderer targetRenderer = GetRendererBySlot(item.equipSlot);
        if (targetRenderer == null) return;


        ApplyCostumeLogic(item.equipSlot);

        targetRenderer.sharedMesh = item.mesh;
        targetRenderer.material = item.material;
        targetRenderer.enabled = true;

        // 상태 저장
        equippedParts[item.equipSlot] = item;
    }
    public void Unequip(EquipSlot slot)
    {
        SkinnedMeshRenderer targetRenderer = GetRendererBySlot(slot);
        if (targetRenderer != null)
        {
            targetRenderer.sharedMesh = null;
            targetRenderer.material = null;
            targetRenderer.enabled = false;
        }
    }

    private bool IsCompatible(CharacterPartItem item)
    {
        return item.Gender == Gender.Both || item.Gender == myCharacterGender;
    }
    private SkinnedMeshRenderer GetRendererBySlot(EquipSlot slot)
    {
        return slot switch
        {
            EquipSlot.Skin => SkinRenderer,
            EquipSlot.Ear => EarRenderer,
            EquipSlot.Hair => HairRenderer,
            EquipSlot.Face => FacesRenderer,

            EquipSlot.Hat => HatRenderer,
            EquipSlot.Costumes => CostumesRenderer,
            EquipSlot.Top => ShirtsRenderer,
            EquipSlot.Bottom => PantsRenderer,

            EquipSlot.Gloves => GlovesRenderer,
            EquipSlot.Shoes => ShoesRenderer,
            EquipSlot.FaceAccessory => FaceAccessoriesRenderer,
            EquipSlot.Glasses => GlassesRenderer,

            _ => null,
        };
    }
    private void ApplyCostumeLogic(EquipSlot slot)
    {
        if (slot == EquipSlot.Costumes)
        {
            ShirtsRenderer.enabled = false;
            PantsRenderer.enabled = false;
        }
        else if (slot == EquipSlot.Top || slot == EquipSlot.Bottom)
        {
            CostumesRenderer.enabled = false;
        }
    }
}

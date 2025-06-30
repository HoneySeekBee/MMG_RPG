using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MMG/Character/DefaultItemSet")]
public class DefaultItemSet : ScriptableObject
{
    public string Description;
    public List<CharacterPartItem> defaultItems; // ���� ScriptableObject ����
}
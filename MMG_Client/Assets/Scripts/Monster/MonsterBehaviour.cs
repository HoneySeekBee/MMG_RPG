using MMG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    public int MonsterID { get { return data.MonsterId; } }
    public void SetData(MonsterData _data)
    {
        data = _data;
    }

}

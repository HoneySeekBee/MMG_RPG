using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MMG/MapDataSet")]
public class MapDataSet : ScriptableObject
{
    public List<MapData> mapDatas;
}

[System.Serializable]
public class MapData
{
    public int MapId;
    public MapType MapType;
    public string SceneName;
    public string Description;
    public List<SpawnPointData> SpawnPoints;
}

[System.Serializable]
public class SpawnPointData
{
    public int SpawnPointId;
    public string Description;

    // AABB 영역 정보 (Plane 기반 추출)
    public List<int> Min = new();
    public List<int> Max = new();
}

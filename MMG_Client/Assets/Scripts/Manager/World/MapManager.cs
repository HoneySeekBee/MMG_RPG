using DevionGames;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : SceneSingleton<MapManager>
{
    [SerializeField] private MapDataSet _mapData;
    public const int DEFAULT_MAP_NUMBER = 1;
    public int MapNumber;
    public void EnterGameScene(int mapNumber = 0)
    {
        Debug.Log("EnterGame");
        MapNumber = mapNumber;
        string sceneName = _mapData.mapDatas.FirstOrDefault(x => x.MapId == mapNumber).SceneName;
        SceneLoader.Instance.LoadScene(sceneName);
    }
}

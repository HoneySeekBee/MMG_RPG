using DevionGames;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : SceneSingleton<MapManager>
{
    [SerializeField] private MapDataSet _mapData;

    public void EnterGameScene(int mapNumber = 0)
    {
        Debug.Log("EnterGame");
        string sceneName = _mapData.mapDatas[mapNumber].SceneName;
        SceneLoader.Instance.LoadScene(sceneName);
    }
}

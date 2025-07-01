using DevionGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    [SerializeField] private MapDataSet _mapData;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
    }

    public void EnterGameScene(int mapNumber = 0)
    {
        SceneLoader.Instance.LoadScene(_mapData.mapDatas[mapNumber].SceneName);
    }
}

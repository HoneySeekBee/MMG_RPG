using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

public class SpawnZoneUpdater
{
    public static void UpdateFromScene(MapDataSet mapDataSet)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        var mapData = mapDataSet.mapDatas.Find(md => md.SceneName == currentSceneName);

        if (mapData == null)
        {
            Debug.LogError($"[SpawnZoneUpdater] ���� ��({currentSceneName})�� �ش��ϴ� MapData ����");
            return;
        }

        var zones = GameObject.FindObjectsOfType<PlaneSpawnZone>(true);
        int updated = 0, added = 0;

        foreach (var zone in zones)
        {
            var existing = mapData.SpawnPoints.Find(sp => sp.SpawnPointId == zone.Id);
            Bounds b = zone.GetBounds();
            List<int> min = new() {
                Mathf.RoundToInt(b.min.x),
                Mathf.RoundToInt(b.min.y),
                Mathf.RoundToInt(b.min.z)
            };
            List<int> max = new() {
                Mathf.RoundToInt(b.max.x),
                Mathf.RoundToInt(b.max.y),
                Mathf.RoundToInt(b.max.z)
            };

            if (existing != null)
            {
                existing.Description = zone.Description;
                existing.Min = min;
                existing.Max = max;
                updated++;
            }
            else
            {
                mapData.SpawnPoints.Add(new SpawnPointData
                {
                    SpawnPointId = zone.Id,
                    Description = zone.Description,
                    Min = min,
                    Max = max
                });
                added++;
            }
        }

        EditorUtility.SetDirty(mapDataSet);
        AssetDatabase.SaveAssets();
        Debug.Log($"[SpawnZoneUpdater] ������Ʈ �Ϸ� �� ����: {updated}, �߰�: {added}");
    }
}

#endif
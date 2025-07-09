using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
namespace MMG
{
    public class SpawnZoneUpdater
    {
        public static void UpdateFromScene(MapDataSet mapDataSet)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            var mapData = mapDataSet.mapDatas.Find(md => md.SceneName == currentSceneName);

            if (mapData == null)
            {
                Debug.LogError($"[SpawnZoneUpdater] 현재 씬({currentSceneName})에 해당하는 MapData 없음");
                return;
            }

            var zones = GameObject.FindObjectsOfType<PlaneSpawnZone>(true);
            int updated = 0, added = 0;

            foreach (var zone in zones)
            {
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

                switch (zone.spawnType)
                {
                    case AreaType.Player:
                        {
                            var existing = mapData.SpawnPoints.Find(sp => sp.SpawnPointId == zone.Id);
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
                            break;
                        }

                    case AreaType.Monster:
                        {
                            var existing = mapData.MonsterSpawnPoints.Find(sp => sp.SpawnPointId == zone.Id);

                            List<MonsterSpawnInfo> monsterList = new();
                            foreach (var sm in zone.spawnMonsterData)
                            {
                                if (sm.monsterData == null)
                                {
                                    Debug.LogWarning($"[SpawnZoneUpdater] monsterData is null for spawn point {zone.Id}");
                                    continue;
                                }

                                monsterList.Add(new MonsterSpawnInfo
                                {
                                    MonsterId = sm.monsterData.MonsterId,
                                    SpawnCount = sm.SpawnCount
                                });
                            }

                            if (existing != null)
                            {
                                existing.Description = zone.Description;
                                existing.Min = min;
                                existing.Max = max;
                                existing.MonsterList = monsterList;
                                updated++;
                            }
                            else
                            {
                                mapData.MonsterSpawnPoints.Add(new MonsterSpawnPointData
                                {
                                    SpawnPointId = zone.Id,
                                    Description = zone.Description,
                                    Min = min,
                                    Max = max,
                                    MonsterList = monsterList
                                });
                                added++;
                            }
                            break;
                        }

                    case AreaType.Block:
                        {
                            var existing = mapData.BlockPoints.Find(sp => sp.SpawnPointId == zone.Id);
                            if (existing != null)
                            {
                                existing.Description = zone.Description;
                                existing.Min = min;
                                existing.Max = max;
                                updated++;
                            }
                            else
                            {
                                mapData.BlockPoints.Add(new SpawnPointData
                                {
                                    SpawnPointId = zone.Id,
                                    Description = zone.Description,
                                    Min = min,
                                    Max = max
                                });
                                added++;
                            }
                            break;
                        }
                }
            }

            EditorUtility.SetDirty(mapDataSet);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SpawnZoneUpdater] 업데이트 완료 → 수정: {updated}, 추가: {added}");
        }

    }
}
#endif
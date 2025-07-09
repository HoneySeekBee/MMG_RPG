using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MMG
{

    public class SpawnZoneExporter
    {
        public static void ExportToJson(MapDataSet mapDataSet)
        {
            string folderPath = EditorUtility.SaveFolderPanel("�ʺ� JSON ���� ���� ����", "", "");
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("[SpawnZoneExporter] ������ ��ҵǾ����ϴ�.");
                return;
            }

            foreach (var map in mapDataSet.mapDatas)
            {
                var playerZoneList = new List<ExportSpawnZone>();
                var monsterZoneList = new List<ExportMonsterSpawnZone>();

                // �÷��̾� ����
                foreach (var sp in map.SpawnPoints)
                {
                    if (sp.Min.Count == 3 && sp.Max.Count == 3)
                    {
                        playerZoneList.Add(new ExportSpawnZone
                        {
                            Id = sp.SpawnPointId,
                            Description = sp.Description,
                            Min = new Vec3(sp.Min[0], sp.Min[1], sp.Min[2]),
                            Max = new Vec3(sp.Max[0], sp.Max[1], sp.Max[2])
                        });
                    }
                }
                foreach (var sp in map.MonsterSpawnPoints)
                {
                    if (sp.Min.Count == 3 && sp.Max.Count == 3)
                    {
                        var monsterList = new List<ExportMonsterSpawnInfo>();
                        if (sp.MonsterList != null)
                        {
                            foreach (var m in sp.MonsterList)
                            {
                                monsterList.Add(new ExportMonsterSpawnInfo
                                {
                                    MonsterId = m.MonsterId,
                                    SpawnCount = m.SpawnCount
                                });
                            }
                        }

                        monsterZoneList.Add(new ExportMonsterSpawnZone
                        {
                            Id = sp.SpawnPointId,
                            Description = sp.Description,
                            Min = new Vec3(sp.Min[0], sp.Min[1], sp.Min[2]),
                            Max = new Vec3(sp.Max[0], sp.Max[1], sp.Max[2]),
                            Monsters = monsterList
                        });
                    }
                }
                var exportData = new ExportMapZone
                {
                    MapId = map.MapId,
                    PlayerSpawnPoints = playerZoneList,
                    MonsterSpawnPoints = monsterZoneList
                };

                string json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
                string fileName = $"map_{map.MapId}_spawn.json";
                string fullPath = Path.Combine(folderPath, fileName);
                File.WriteAllText(fullPath, json);
                Debug.Log($"[SpawnZoneExporter] ���� �Ϸ�: {fullPath}");
            }

            Debug.Log("[SpawnZoneExporter] ��� ���� SpawnZone JSON ���� �Ϸ�!");
        }
    }
    [System.Serializable]
    public class ExportMapZone
    {
        public int MapId;
        public List<ExportSpawnZone> PlayerSpawnPoints;
        public List<ExportMonsterSpawnZone> MonsterSpawnPoints;
    }

    [System.Serializable]
    public class ExportSpawnZone
    {
        public int Id;
        public string Description;
        public Vec3 Min;
        public Vec3 Max;
    }
    [System.Serializable]
    public class ExportMonsterSpawnZone : ExportSpawnZone
    {
        public List<ExportMonsterSpawnInfo> Monsters;
    }

    [System.Serializable]
    public class ExportMonsterSpawnInfo
    {
        public int MonsterId;
        public int SpawnCount;
    }

    [System.Serializable]
    public struct Vec3
    {
        public float x, y, z;
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }
}
#endif
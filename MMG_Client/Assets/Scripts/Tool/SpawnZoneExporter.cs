using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpawnZoneExporter
{
    public static void ExportToJson(MapDataSet mapDataSet)
    {
        var exportList = new List<ExportMapZone>();

        foreach (var map in mapDataSet.mapDatas)
        {
            var zoneList = new List<ExportSpawnZone>();

            foreach (var sp in map.SpawnPoints)
            {
                if (sp.Min.Count == 3 && sp.Max.Count == 3)
                {
                    zoneList.Add(new ExportSpawnZone
                    {
                        Id = sp.SpawnPointId,
                        Description = sp.Description,
                        Min = new Vec3(sp.Min[0], sp.Min[1], sp.Min[2]),
                        Max = new Vec3(sp.Max[0], sp.Max[1], sp.Max[2])
                    });
                }
            }

            exportList.Add(new ExportMapZone
            {
                MapId = map.MapId,
                SpawnPoints = zoneList
            });
        }

        string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);
        string path = Path.Combine(Application.dataPath, "../ServerData/SpawnZones.json");
        File.WriteAllText(path, json);
        Debug.Log($"[SpawnZoneExporter] 서버 JSON 내보내기 완료 → {path}");
    }

    [System.Serializable]
    public class ExportMapZone
    {
        public int MapId;
        public List<ExportSpawnZone> SpawnPoints;
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
    public struct Vec3
    {
        public float x, y, z;
        public Vec3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }
}

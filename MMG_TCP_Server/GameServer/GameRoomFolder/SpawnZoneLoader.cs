
using Newtonsoft.Json;
using System.Numerics;

namespace GameServer.GameRoomFolder
{
    public class SpawnZoneLoader
    {
        public class Vec3
        {
            public float x;
            public float y;
            public float z;
        }

        public class SpawnPointData
        {
            public int Id;
            public string Description;
            public Vec3 Min;
            public Vec3 Max;
        }

        public class MapSpawnZone
        {
            public int MapId;
            public List<SpawnPointData> SpawnPoints;
        }

        public static Dictionary<(int MapId, int SpawnId), (Vec3 min, Vec3 max)> ZoneDict = new();

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[SpawnZoneLoader] 파일 없음: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            var mapZones = JsonConvert.DeserializeObject<List<MapSpawnZone>>(json);

            foreach (var map in mapZones)
            {
                foreach (var sp in map.SpawnPoints)
                {
                    ZoneDict[(map.MapId, sp.Id)] = (sp.Min, sp.Max);
                }
            }

            Console.WriteLine($"[SpawnZoneLoader] 로드 완료: 총 {ZoneDict.Count}개 SpawnZone");
        }

        public static Vector3 GetRandomSpawnPos(int mapId, int spawnId)
        {
            if (ZoneDict.TryGetValue((mapId, spawnId), out var range))
            {
                float x = RandomFloat(range.min.x, range.max.x);
                float y = RandomFloat(range.min.y, range.max.y);
                float z = RandomFloat(range.min.z, range.max.z);
                return new Vector3(x, y, z);
            }

            return new Vector3(0, 0, 0); // fallback
        }

        private static float RandomFloat(float min, float max)
        {
            return (float)(new Random().NextDouble() * (max - min) + min);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Room
{
    public class Vec3
    {
        public float x;
        public float y;
        public float z;
    }
    public class MonsterSpawnInfo
    {
        public int MonsterId;
        public int SpawnCount;
    }
    public class MonsterSpawnZone
    {
        public int Id;
        public string Description;
        public Vec3 Min;
        public Vec3 Max;
        public List<MonsterSpawnInfo> Monsters;
    }
    public class SpawnPointData
    {
        public int Id;
        public string Description;
        public Vec3 Min;
        public Vec3 Max;
    }
    public class BlockPointData
    {
        public int Id;
        public string Description;
        public Vec3 Min;
        public Vec3 Max;
    }
    public class PlayerSpawnZone
    {
        public int Id;
        public string Description;
        public Vec3 Min;
        public Vec3 Max;
    }
    public class MapSpawnZone
    {
        public int MapId;
        public List<MonsterSpawnZone> MonsterSpawnPoints;
        public List<PlayerSpawnZone> PlayerSpawnPoints; // 생략 가능
        public List<BlockPointData> BlockPoints;
    }

    public class SpawnZoneManager
    {
        private List<PlayerSpawnZone> _playerSpawnZones = new();
        private List<MonsterSpawnZone> _monsterSpawnZones = new();
        private List<BlockPointData> _blockPoints = new();
        public SpawnZoneManager(int mapId)
        {
            string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            string path = Path.Combine(projectRoot, "Resources", "SpawnZones", $"map_{mapId}_spawn.json");

            if (!File.Exists(path))
            {
                Console.WriteLine($"[SpawnZoneManager] 스폰 JSON 없음: {path}");
                return;
            }

            string json = File.ReadAllText(path);
            MapSpawnZone data = JsonConvert.DeserializeObject<MapSpawnZone>(json);

            _playerSpawnZones = data.PlayerSpawnPoints ?? new();
            _monsterSpawnZones = data.MonsterSpawnPoints ?? new();
            _blockPoints = data.BlockPoints ?? new();  // 추가

            Console.WriteLine($"[SpawnZoneManager] map({mapId}) 로드 완료 - Player: {_playerSpawnZones.Count}, Monster: {_monsterSpawnZones.Count}, Block: {_blockPoints.Count}");
        }
        public Vector3 GetRandomPlayerSpawnPos(int spawnId)
        {
            var zone = _playerSpawnZones.Find(z => z.Id == spawnId);
            if (zone == null)
            {
                Console.WriteLine($"[SpawnZoneManager] Player SpawnPointId({spawnId}) 없음");
                return new Vector3(0, 0, 0);
            }

            return GetRandomPosition(zone.Min, zone.Max);
        }
        public List<MonsterSpawnZone> GetAllMonsterZones()
        {
            return _monsterSpawnZones;
        }
        public Vector3 GetRandomPosition(Vec3 min, Vec3 max)
        {
            float x = RandomFloat(min.x, max.x);
            float y = RandomFloat(min.y, max.y);
            float z = RandomFloat(min.z, max.z);
            return new Vector3(x, y, z);
        }
        public float RandomFloat(float min, float max)
        {
            return (float)(new Random().NextDouble() * (max - min) + min);
        }
        public List<BlockPointData> GetAllBlockPoints()
        {
            return _blockPoints;
        }
    }
}
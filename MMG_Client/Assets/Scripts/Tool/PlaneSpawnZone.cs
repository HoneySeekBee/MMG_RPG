using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG
{
    public enum SpawnType
    {
        Player = 0,
        Monster = 1, 

    }

    [DisallowMultipleComponent]
    public class PlaneSpawnZone : MonoBehaviour
    {
        public int Id;
        public string Description;
        public SpawnType spawnType;
        [System.Serializable]
        public class SpawnMonster
        {
            public int SpawnCount;
            public MonsterData monsterData;
        }
        public List<SpawnMonster> spawnMonsterData;
        public Bounds GetBounds()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
                return renderer.bounds;

            return new Bounds(transform.position, transform.localScale);
        }
    }
}
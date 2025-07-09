using Packet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnCharacterManager : GlobalSingleton<SpawnCharacterManager>
{
    private Dictionary<int, GameObject> MonsterDictionary = new Dictionary<int, GameObject>();

    public IEnumerator SpawnDataCashing()
    {
        // 2. 몬스터 프리팹 전체 로드 (Label or AddressableName "Monster")
        var monsterHandle = Addressables.LoadAssetsAsync<GameObject>("Monster", null);
        yield return monsterHandle;

        if (monsterHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var prefab in monsterHandle.Result)
            {
                var monster = prefab.GetComponent<MonsterBehaviour>();
                if (monster == null)
                {
                    Debug.LogWarning($"[GlobalCashing] MonsterBehaviour 없음: {prefab.name}");
                    continue;
                }

                int id = monster.MonsterID;
                if (!MonsterDictionary.ContainsKey(id))
                {
                    MonsterDictionary.Add(id, prefab);
                    Debug.Log($"[GlobalCashing] 몬스터 등록됨: ID={id}, Name={prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[GlobalCashing] 중복 MonsterID: {id}, 무시됨");
                }
            }

            Debug.Log($"[GlobalCashing] 몬스터 {MonsterDictionary.Count}개 로딩 완료");
        }
        else
        {
            Debug.LogError("[GlobalCashing] 몬스터 프리팹 로딩 실패");
        }
    }
    public GameObject GetMonster(int monsterID) { return MonsterDictionary[monsterID]; }
}

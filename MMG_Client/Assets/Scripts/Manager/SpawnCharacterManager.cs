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
        // 2. ���� ������ ��ü �ε� (Label or AddressableName "Monster")
        var monsterHandle = Addressables.LoadAssetsAsync<GameObject>("Monster", null);
        yield return monsterHandle;

        if (monsterHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var prefab in monsterHandle.Result)
            {
                var monster = prefab.GetComponent<MonsterBehaviour>();
                if (monster == null)
                {
                    Debug.LogWarning($"[GlobalCashing] MonsterBehaviour ����: {prefab.name}");
                    continue;
                }

                int id = monster.MonsterID;
                if (!MonsterDictionary.ContainsKey(id))
                {
                    MonsterDictionary.Add(id, prefab);
                    Debug.Log($"[GlobalCashing] ���� ��ϵ�: ID={id}, Name={prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[GlobalCashing] �ߺ� MonsterID: {id}, ���õ�");
                }
            }

            Debug.Log($"[GlobalCashing] ���� {MonsterDictionary.Count}�� �ε� �Ϸ�");
        }
        else
        {
            Debug.LogError("[GlobalCashing] ���� ������ �ε� ����");
        }
    }
    public GameObject GetMonster(int monsterID) { return MonsterDictionary[monsterID]; }
}

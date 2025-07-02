using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnpointManager : SceneSingleton<SpawnpointManager>
{
    [SerializeField]
    private List<SpawnPointEntry> spawnPoints;

    private Dictionary<int, Transform> _spawnDict = new();

    private void Start()
    {
        foreach (var entry in spawnPoints)
        {
            if (!_spawnDict.ContainsKey(entry.Id))
                _spawnDict.Add(entry.Id, entry.Point);
            else
                Debug.LogWarning($"�ߺ��� SpawnPoint ID: {entry.Id}");
        }
    }

    /// <summary>
    /// �ش� ID�� SpawnPoint ��ġ�� ��ȯ. ������ ���� �ɼ� ����.
    /// </summary>
    public Vector3 GetSpawnPosition(int id = 0, bool applyOffset = true)
    {
        if (_spawnDict.TryGetValue(id, out var transform))
        {
            Vector3 pos = transform.position;
            if (applyOffset)
            {
                pos += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }
            return pos;
        }

        Debug.LogWarning($"[SpawnPointManager] SpawnPoint ID {id} ����");
        return Vector3.zero;
    }

    public IReadOnlyDictionary<int, Transform> GetAllSpawnPoints()
    {
        return _spawnDict;
    }
}
[System.Serializable]
public class SpawnPointEntry
{
    public int Id;
    public Transform Point;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ArrowPool : SceneSingleton<ArrowPool>
{

    [SerializeField] private string arrowAddress = "Arrow";
    [SerializeField] private int preloadCount = 10;

    private Queue<GameObject> arrowPool = new();

    private void Start()
    {
        StartCoroutine(Preload());
    }

    private IEnumerator Preload()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            var handle = Addressables.InstantiateAsync(arrowAddress);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject arrow = handle.Result;
                arrow.SetActive(false);
                arrow.transform.SetParent(transform);
                arrowPool.Enqueue(arrow);
            }
        }
    }

    public GameObject GetArrow()
    {
        if (arrowPool.Count > 0)
        {
            GameObject arrow = arrowPool.Dequeue();
            arrow.SetActive(true);
            return arrow;
        }
        else
        {
            // 없으면 새로 생성해서라도 제공
            var handle = Addressables.InstantiateAsync(arrowAddress);
            handle.WaitForCompletion(); // 비추천이긴 한데 예제용
            return handle.Result;
        }
    }

    public void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        arrow.transform.SetParent(transform);
        arrowPool.Enqueue(arrow);
    }
}

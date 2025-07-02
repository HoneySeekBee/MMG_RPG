using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : GlobalSingleton<SceneLoader>
{

    [SerializeField] private GameObject loadingScreen; // �ε� UI ���� ����
    [SerializeField] private float fakeLoadingTime = 1f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // ���� �ε�
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (!op.isDone)
        {
            timer += Time.deltaTime;

            // �� �ε��� �������� ��� �ð� �Ǵ� ���� ���
            if (op.progress >= 0.9f && timer >= fakeLoadingTime)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}

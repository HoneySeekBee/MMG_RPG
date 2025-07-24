using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatContentUI : SceneSingleton<ChatContentUI>
{
    public GameObject chatLinePrefab;
    public Transform contentTransform;
    private ScrollRect scrollRect;
    [SerializeField] private TMP_InputField inputField;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeChatLines = new List<GameObject>();
    private DateTime lastChatTime;
    [SerializeField] private GameObject chatRootUI; 
    private void Start()
    {
        scrollRect = GetComponentInChildren<ScrollRect>();
        inputField.onSubmit.AddListener(OnChatSubmit);
    }
    private void Update()
    {
        DateTime now = DateTime.Now;

        for (int i = activeChatLines.Count - 1; i >= 0; i--)
        {
            GameObject go = activeChatLines[i];
            ChatLineUI chatLine = go.GetComponent<ChatLineUI>();

            if ((now - chatLine.ChatTime).TotalMinutes >= 5)
            {
                activeChatLines.RemoveAt(i);
                Return(go);
            }
        }
        if ((now - lastChatTime).TotalSeconds >= 5)
        {
            if (chatRootUI.activeSelf)
                chatRootUI.SetActive(false);
        }
    }
    public GameObject Get()
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(chatLinePrefab, contentTransform);
        }
        return go;
    }
    public void Return(GameObject go)
    {
        go.SetActive(false);
        pool.Enqueue(go);
    }

    public void ClearAll()
    {
        foreach (Transform child in contentTransform)
        {
            Return(child.gameObject);
        }
    }
    public void AddChat(string message, DateTime time)
    {
        GameObject go = Get();
        ChatLineUI chatLineUI = go.GetComponent<ChatLineUI>();
        chatLineUI.Get_Text(message, time);

        activeChatLines.Add(go); // 리스트에 등록
        lastChatTime = DateTime.Now;   
        chatRootUI.SetActive(true);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void UserChat(string nickName, string message, DateTime time)
    {
        string chat = $"<color=#00BFFF>{nickName}</color> : <color=#FFFFFF>{message}</color>";
        AddChat(chat, time);
    }
    public void AdminChat(string message, DateTime time)
    {
        string chat = $"<color=#FFD700>{message}</color>";
        AddChat(chat, time);

    }
    public void SystemChat(string message, DateTime time)
    {
        string chat = $"<color=#FFA500>System : {message}</color>";
        AddChat(chat, time);
    }


    private void OnChatSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        SystemChat(text, DateTime.Now);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class ChatLineUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    public DateTime ChatTime { get; private set; }

    public void Get_Text(string _text, DateTime chatTime)
    {
        text.text = _text;
        ChatTime = chatTime;
    }
    
}

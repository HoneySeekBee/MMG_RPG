using DevionGames.UIWidgets;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMG_System
{
    [CreateAssetMenu(menuName = "MMG/Quest")]
    public class QuestModule : ScriptableObject
    {
        [Header("����Ʈ ����")]
        public string QuestName;
        public string QuestDescription;

        [Header("����Ʈ ���� ����")]
        public int UnlockLevel;
        public List<QuestCondition> Quest_Condition;

        [Header("����Ʈ ��ǥ")]
        public List<QuestGoal> Quest_Goal;

        [Header("����Ʈ �޼� ����")]
        public List<Item> Quest_Achiev;
        public float Quest_EXP;

    }
    public enum ItemType { Equip, Useable, Etc }

    [System.Serializable]
    public class Item
    {
        public string Item_Name;
        public Sprite Item_Icon;
        public ItemType Item_Type; // ������ Ÿ�� : ��� / �Ҹ�ǰ / ��Ÿ  => Protobuf�� ���� �����ϱ� 
        public TimeSpan? Duration; // ������ �Ⱓ : �Ⱓ�� or ���� => ȹ�� ��¥ ����ϱ� 
        public List<string> UseCondition; // ������ ��� ���� : Ư�� ���� �̻� Ȥ�� Ư�� Ŭ���� 
        public List<float> Status; // ������ �ɷ� : ���� �߰�
        public List<float> UseEffect; // ������ ��� ȿ�� : ü�� ȸ��, ���� ȸ�� �� 
    }

    [System.Serializable]
    public class QuestCondition
    {
        public string ConditionDescription;
        //public bool checkContidion { get { return CheckCondition(); } }
        //public abstract bool CheckCondition();

        // [1] Ư�� ��� ���� 
        // [2] ���� ����Ʈ �Ϸ�
        // [3] NPC���� ���ɱ� 
    }
    [System.Serializable]
    public class QuestGoal
    {
        public string GoalDescription;
        public bool isAchiev;
        //public abstract bool CheckAchiev();

        // [1] Ư�� ���� N���� ��� 

        // [2] Ư�� NPC���� ���ɱ� 

        // [3] Ư�� ������ N�� �̻� ȹ�� 
    }
}
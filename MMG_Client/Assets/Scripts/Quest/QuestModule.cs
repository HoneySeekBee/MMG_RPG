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
        [Header("퀘스트 설명")]
        public string QuestName;
        public string QuestDescription;

        [Header("퀘스트 등장 조건")]
        public int UnlockLevel;
        public List<QuestCondition> Quest_Condition;

        [Header("퀘스트 목표")]
        public List<QuestGoal> Quest_Goal;

        [Header("퀘스트 달성 보상")]
        public List<Item> Quest_Achiev;
        public float Quest_EXP;

    }
    public enum ItemType { Equip, Useable, Etc }

    [System.Serializable]
    public class Item
    {
        public string Item_Name;
        public Sprite Item_Icon;
        public ItemType Item_Type; // 아이템 타입 : 장비 / 소모품 / 기타  => Protobuf를 통해 구현하기 
        public TimeSpan? Duration; // 아이템 기간 : 기간제 or 영구 => 획득 날짜 기록하기 
        public List<string> UseCondition; // 아이템 사용 조건 : 특정 레벨 이상 혹은 특정 클래스 
        public List<float> Status; // 아이템 능력 : 스텟 추가
        public List<float> UseEffect; // 아이템 사용 효과 : 체력 회복, 마나 회복 등 
    }

    [System.Serializable]
    public class QuestCondition
    {
        public string ConditionDescription;
        //public bool checkContidion { get { return CheckCondition(); } }
        //public abstract bool CheckCondition();

        // [1] 특정 장소 입장 
        // [2] 선행 퀘스트 완료
        // [3] NPC에게 말걸기 
    }
    [System.Serializable]
    public class QuestGoal
    {
        public string GoalDescription;
        public bool isAchiev;
        //public abstract bool CheckAchiev();

        // [1] 특정 몬스터 N마리 사냥 

        // [2] 특정 NPC에게 말걸기 

        // [3] 특정 아이템 N개 이상 획득 
    }
}
#if UNITY_EDITOR
using MMG;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class MonsterDataManageWindow : EditorWindow
{
    private MonsterData newData;
    public UnityEngine.Object saveFolderObject;
    [MenuItem("Tools/MMG/MonsterData Window")]
    public static void ShowWindow()
    {
        GetWindow<MonsterDataManageWindow>("MonsterData ����");
    }
    private void OnGUI()
    {
        GUILayout.Label("MonsterData ���� ���� ����");
        saveFolderObject = EditorGUILayout.ObjectField("����", saveFolderObject, typeof(DefaultAsset), false);

        GUILayout.Space(5);
        GUILayout.Label("�ű� MonsterData ����", EditorStyles.boldLabel);

        if (GUILayout.Button("MonsterData ���� (ID �ڵ� �ο�)"))
        {
            _ = CreateNewMonster();
        }

        GUILayout.Space(10);
        if (newData != null)
        {
            EditorGUILayout.LabelField("������ MonsterData ����:", EditorStyles.boldLabel);
            newData.MonsterName = EditorGUILayout.TextField("�̸�", newData.MonsterName);
            newData._MaxHP = EditorGUILayout.FloatField("HP", newData._MaxHP);
            newData._MoveSpeed = EditorGUILayout.FloatField("Speed", newData._MoveSpeed);
            newData._ChaseRange = EditorGUILayout.FloatField("Chase Range", newData._ChaseRange);
            newData._AttackRange = EditorGUILayout.FloatField("Attack Range", newData._AttackRange);

            GUILayout.Space(10);
            GUILayout.Label("Attack Data", EditorStyles.boldLabel);

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedObject monsterSerialized = new SerializedObject(newData);
            SerializedProperty attackList = monsterSerialized.FindProperty("_AttackData");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(attackList, new GUIContent("Attack Data"), true);
            if (EditorGUI.EndChangeCheck())
            {
                monsterSerialized.ApplyModifiedProperties();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("MonsterData ����"))
            {
                SaveMonsterDataAsset(newData);
                newData = null;
            }
        }
    }

    private async Task CreateNewMonster()
    {
        int nextId = await GetNextMonsterIdFromServer();

        if (nextId <= 0)
        {
            Debug.LogError("��ȿ���� ���� MonsterId");
            newData = null;
        }
        else
        {
            newData = ScriptableObject.CreateInstance<MonsterData>();
            newData.MonsterId = nextId;
            newData.MonsterName = $"New_Monster_{nextId}";
        }

        Repaint();
    }

    private async Task<int> GetNextMonsterIdFromServer()
    {
        try
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (a, b, c, d) => true }))
            {
                string url = NetworkManager.MMG_API_URL + "/api/monster/max-id";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("���� ���� ����: " + response.StatusCode);

                string result = await response.Content.ReadAsStringAsync();
                int maxId = int.TryParse(result, out int id) ? id : 0;
                return maxId + 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("�������� ID �޾ƿ��� ����: " + ex.Message);
            return -1;
        }
    }
    private async void SaveMonsterDataAsset(MonsterData data)
    {
        if (saveFolderObject == null)
        {
            Debug.LogError("������ ������ �������� �ʾҽ��ϴ�.");
            return;
        }

        // [1] ���� ��ü ���� ���ε�
        var monsterDto = new
        {
            Id = data.MonsterId,
            Name = data.MonsterName,
            HP = data._MaxHP,
            Speed = data._MoveSpeed,
            ChaseRange = data._ChaseRange,
            AttackRange = data._AttackRange
        };

        string monsterJson = JsonConvert.SerializeObject(monsterDto);

        using (HttpClient client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        }))
        {
            string monsterUrl = NetworkManager.MMG_API_URL + "/api/monster/upload";
            var monsterContent = new StringContent(monsterJson, Encoding.UTF8, "application/json");

            try
            {
                var res = await client.PostAsync(monsterUrl, monsterContent);
                if (!res.IsSuccessStatusCode)
                {
                    Debug.LogError("���� DB ���� ����: " + res.StatusCode);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("���� ���� �� ���� �߻�: " + ex.Message);
                return;
            }
        }

        // [2] MonsterSkill ���� ���ε�
        var skillList = data._AttackData
    .Where(x => x.attackData != null)
    .Select(x => new MonsterSkillDto
    {
        MonsterId = data.MonsterId,
        SkillId = (int)x.attackData.AttackId,
        Frequency = x.AttackAppearanceAmount,
        InputType = (int)x.inputType
    }).ToList();

        Debug.Log($"��ų üũ {skillList[0].SkillId} || {data._AttackData[0].attackData.AttackId}");

        if (skillList.Count > 0)
        {
            string skillJson = JsonConvert.SerializeObject(skillList);

            using (HttpClient client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            }))
            {
                string skillUrl = NetworkManager.MMG_API_URL + "/api/monsterskill/upload-all"; //  �̸� ����
                var content = new StringContent(skillJson, Encoding.UTF8, "application/json");

                try
                {
                    var res = await client.PostAsync(skillUrl, content);
                    if (!res.IsSuccessStatusCode)
                    {
                        Debug.LogError("���� ��ų ���� ����: " + res.StatusCode);
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("���� ��ų ���� �� ���� �߻�: " + ex.Message);
                    return;
                }
            }
        }

        // [3] ���� ����
        string saveFolderPath = AssetDatabase.GetAssetPath(saveFolderObject);
        string path = $"{saveFolderPath}/Monster_{data.MonsterId}_{data.name}.asset";

        if (AssetDatabase.LoadAssetAtPath<MonsterData>(path) != null)
            AssetDatabase.DeleteAsset(path);

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;

        Debug.Log($"MonsterData ���� �Ϸ�: {path}");
    }
}
#endif
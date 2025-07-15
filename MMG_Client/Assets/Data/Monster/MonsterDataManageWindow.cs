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
        GetWindow<MonsterDataManageWindow>("MonsterData 관리");
    }
    private void OnGUI()
    {
        GUILayout.Label("MonsterData 저장 폴더 선택");
        saveFolderObject = EditorGUILayout.ObjectField("폴더", saveFolderObject, typeof(DefaultAsset), false);

        GUILayout.Space(5);
        GUILayout.Label("신규 MonsterData 생성", EditorStyles.boldLabel);

        if (GUILayout.Button("MonsterData 생성 (ID 자동 부여)"))
        {
            _ = CreateNewMonster();
        }

        GUILayout.Space(10);
        if (newData != null)
        {
            EditorGUILayout.LabelField("생성할 MonsterData 정보:", EditorStyles.boldLabel);
            newData.MonsterName = EditorGUILayout.TextField("이름", newData.MonsterName);
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
            if (GUILayout.Button("MonsterData 저장"))
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
            Debug.LogError("유효하지 않은 MonsterId");
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
                    throw new Exception("서버 응답 실패: " + response.StatusCode);

                string result = await response.Content.ReadAsStringAsync();
                int maxId = int.TryParse(result, out int id) ? id : 0;
                return maxId + 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("서버에서 ID 받아오기 실패: " + ex.Message);
            return -1;
        }
    }
    private async void SaveMonsterDataAsset(MonsterData data)
    {
        if (saveFolderObject == null)
        {
            Debug.LogError("저장할 폴더가 지정되지 않았습니다.");
            return;
        }

        // [1] 몬스터 본체 정보 업로드
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
                    Debug.LogError("몬스터 DB 저장 실패: " + res.StatusCode);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("몬스터 저장 중 예외 발생: " + ex.Message);
                return;
            }
        }

        // [2] MonsterSkill 정보 업로드
        var skillList = data._AttackData
    .Where(x => x.attackData != null)
    .Select(x => new MonsterSkillDto
    {
        MonsterId = data.MonsterId,
        SkillId = (int)x.attackData.AttackId,
        Frequency = x.AttackAppearanceAmount,
        InputType = (int)x.inputType
    }).ToList();

        Debug.Log($"스킬 체크 {skillList[0].SkillId} || {data._AttackData[0].attackData.AttackId}");

        if (skillList.Count > 0)
        {
            string skillJson = JsonConvert.SerializeObject(skillList);

            using (HttpClient client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            }))
            {
                string skillUrl = NetworkManager.MMG_API_URL + "/api/monsterskill/upload-all"; //  이름 수정
                var content = new StringContent(skillJson, Encoding.UTF8, "application/json");

                try
                {
                    var res = await client.PostAsync(skillUrl, content);
                    if (!res.IsSuccessStatusCode)
                    {
                        Debug.LogError("몬스터 스킬 저장 실패: " + res.StatusCode);
                        return;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("몬스터 스킬 저장 중 예외 발생: " + ex.Message);
                    return;
                }
            }
        }

        // [3] 에셋 저장
        string saveFolderPath = AssetDatabase.GetAssetPath(saveFolderObject);
        string path = $"{saveFolderPath}/Monster_{data.MonsterId}_{data.name}.asset";

        if (AssetDatabase.LoadAssetAtPath<MonsterData>(path) != null)
            AssetDatabase.DeleteAsset(path);

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;

        Debug.Log($"MonsterData 저장 완료: {path}");
    }
}
#endif
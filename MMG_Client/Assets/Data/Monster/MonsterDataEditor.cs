#if UNITY_EDITOR
using MMG;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterData))]
public class MonsterDataEditor : Editor
{
    private bool isEditing = false;
    private MonsterData originalCopy;

    public override void OnInspectorGUI()
    {
        MonsterData data = (MonsterData)target;

        if (!isEditing)
        {
            GUI.enabled = false;
            DrawDefaultInspector();
            GUI.enabled = true;

            if (GUILayout.Button("수정하기"))
            {
                isEditing = true;
                originalCopy = CreateInstance<MonsterData>();
                EditorUtility.CopySerialized(data, originalCopy);
            }
        }
        else
        {
            DrawDefaultInspector();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("DB에 저장하기"))
            {
                isEditing = false;
                UploadToDatabase(data);
                originalCopy = null;
            }

            if (GUILayout.Button("취소"))
            {
                isEditing = false;
                if (originalCopy != null)
                    EditorUtility.CopySerialized(originalCopy, data);
                originalCopy = null;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private async void UploadToDatabase(MonsterData data)
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (a, b, c, d) => true
        }))
        {
            try
            {
                // 1. Monster 정보 전송
                var monsterDto = data.ToMonsterDto();
                string monsterJson = JsonConvert.SerializeObject(monsterDto);
                var monsterRes = await client.PostAsync(
                    NetworkManager.MMG_API_URL + "/api/monster/upload",
                    new StringContent(monsterJson, Encoding.UTF8, "application/json")
                );

                if (monsterRes.IsSuccessStatusCode)
                    Debug.Log("몬스터 본체 정보 저장 성공!");
                else
                    Debug.LogError("몬스터 저장 실패: " + monsterRes.StatusCode);

                // 2. MonsterSkill 정보 전송
                var skillDtos = data.ToMonsterSkillDtos();

                string skillJson = JsonConvert.SerializeObject(skillDtos);
                var skillRes = await client.PostAsync(
                    NetworkManager.MMG_API_URL + "/api/monsterskill/upload-all",
                    new StringContent(skillJson, Encoding.UTF8, "application/json")
                );

                if (skillRes.IsSuccessStatusCode)
                    Debug.Log("몬스터 스킬 정보 저장 성공!");
                else
                    Debug.LogError("몬스터 스킬 저장 실패: " + skillRes.StatusCode);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("예외 발생: " + ex.Message);
            }
        }
    }
}


#endif
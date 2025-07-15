#if UNITY_EDITOR
using AttackPacket;
using GamePacket;
using Newtonsoft.Json;
using Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AttackDataManageWindow : EditorWindow
{
    private AttackData newData; 
    public UnityEngine.Object saveFolderObject;
    private string saveFolder = "Assets/Resources/AttackData/";

    [MenuItem("Tools/MMG/AttackData Manage")]
    public static void ShowWindow()
    {
        GetWindow<AttackDataManageWindow>("AttackData 관리");
    }
    private void OnGUI()
    {
        GUILayout.Label("AttackData 저장 폴더 선택");
        saveFolderObject = EditorGUILayout.ObjectField("폴더", saveFolderObject, typeof(DefaultAsset), false);

        GUILayout.Space(5);
        GUILayout.Label("신규 AttackData 생성", EditorStyles.boldLabel);

        if (GUILayout.Button("AttackData 생성 (ID 자동 부여)"))
        {
            // 비동기 처리 함수 따로 실행
            _ = CreateNewAttackData();
        }

        GUILayout.Space(10);

        if (newData != null)
        {
            EditorGUILayout.LabelField("생성할 AttackData 정보:");
            newData.AttackName = EditorGUILayout.TextField("이름", newData.AttackName);
            newData.OwnerType = (OwnerType)EditorGUILayout.EnumPopup("소유자 타입", newData.OwnerType);
            newData.WeaponType = (WeaponType)EditorGUILayout.EnumPopup("무기 타입", newData.WeaponType);
            newData.AttackType = (AttackType)EditorGUILayout.EnumPopup("공격 타입", newData.AttackType);
            newData.Range = EditorGUILayout.FloatField("Range", newData.Range);
            newData.Angle = EditorGUILayout.FloatField("Angle", newData.Angle);
            newData.Damage = EditorGUILayout.FloatField("Damage", newData.Damage);
            newData.Cooldown = EditorGUILayout.FloatField("Cooldown", newData.Cooldown);
            newData.DelayAfter = EditorGUILayout.FloatField("DelayAfter", newData.DelayAfter);
            newData.CastTime = EditorGUILayout.FloatField("CastTime", newData.CastTime);

            GUILayout.Space(10);
            if (GUILayout.Button("AttackData 저장"))
            {
                SaveAttackDataAsset(newData);
                newData = null; // 초기화
            }
        }
    }
    private async Task CreateNewAttackData()
    {
        int nextId = await GetNextAttackIdFromServer();

        if (nextId <= 0)
        {
            Debug.LogError("유효하지 않은 AttackId");
            newData = null;
        }
        else
        {
            newData = ScriptableObject.CreateInstance<AttackData>();
            newData.AttackId = nextId;
            newData.AttackName = $"New_Attack_{nextId}";
        }

        Repaint(); // GUI 다시 그리도록 강제 트리거
    }
    private async void SaveAttackDataAsset(AttackData data)
    {
        bool success = await UploadToDatabase(data);
        if (!success)
        {
            Debug.LogError(" DB 업로드 실패: AttackData 에셋 저장 중단됨.");
            return;
        }

        string path = $"{saveFolder}Attack_{data.AttackId}.asset";
        if (saveFolderObject != null)
        {
            string saveFolderPath = AssetDatabase.GetAssetPath(saveFolderObject);
            path = $"{saveFolderPath}/Attack_{data.AttackId}.asset";
        }

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = data;

        Debug.Log($"AttackData 저장 완료: {path}");
    }
    private async Task<bool> UploadToDatabase(AttackData data)
    {
        var dto = new
        {
            AttackId = data.AttackId,
            AttackName = data.AttackName,
            OwnerType = (int)data.OwnerType,
            WeaponType = (int)data.WeaponType,
            AttackType = (int)data.AttackType,
            Range = data.Range,
            Angle = data.Angle,
            Damage = data.Damage,
            Cooldown = data.Cooldown,
            DelayAfter = data.DelayAfter,
            CastTime = data.CastTime
        };

        string json = JsonConvert.SerializeObject(dto);

        try
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            }))
            {
                string url = NetworkManager.MMG_API_URL + "/api/skill/upload-single";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(url, content);

                if (res.IsSuccessStatusCode)
                {
                    Debug.Log("DB 업로드 성공");
                    return true;
                }
                else
                {
                    Debug.LogError("DB 업로드 실패: " + res.StatusCode);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("예외 발생: " + ex.Message);
            return false;
        }
    }
    private async Task<int> GetNextAttackIdFromServer()
    {
        try
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            }))
            {
                string url = NetworkManager.MMG_API_URL + "/api/skill/max-id";
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
            return -1; // 오류 표시용
        }
    }
}
#endif
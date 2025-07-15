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
        GetWindow<AttackDataManageWindow>("AttackData ����");
    }
    private void OnGUI()
    {
        GUILayout.Label("AttackData ���� ���� ����");
        saveFolderObject = EditorGUILayout.ObjectField("����", saveFolderObject, typeof(DefaultAsset), false);

        GUILayout.Space(5);
        GUILayout.Label("�ű� AttackData ����", EditorStyles.boldLabel);

        if (GUILayout.Button("AttackData ���� (ID �ڵ� �ο�)"))
        {
            // �񵿱� ó�� �Լ� ���� ����
            _ = CreateNewAttackData();
        }

        GUILayout.Space(10);

        if (newData != null)
        {
            EditorGUILayout.LabelField("������ AttackData ����:");
            newData.AttackName = EditorGUILayout.TextField("�̸�", newData.AttackName);
            newData.OwnerType = (OwnerType)EditorGUILayout.EnumPopup("������ Ÿ��", newData.OwnerType);
            newData.WeaponType = (WeaponType)EditorGUILayout.EnumPopup("���� Ÿ��", newData.WeaponType);
            newData.AttackType = (AttackType)EditorGUILayout.EnumPopup("���� Ÿ��", newData.AttackType);
            newData.Range = EditorGUILayout.FloatField("Range", newData.Range);
            newData.Angle = EditorGUILayout.FloatField("Angle", newData.Angle);
            newData.Damage = EditorGUILayout.FloatField("Damage", newData.Damage);
            newData.Cooldown = EditorGUILayout.FloatField("Cooldown", newData.Cooldown);
            newData.DelayAfter = EditorGUILayout.FloatField("DelayAfter", newData.DelayAfter);
            newData.CastTime = EditorGUILayout.FloatField("CastTime", newData.CastTime);

            GUILayout.Space(10);
            if (GUILayout.Button("AttackData ����"))
            {
                SaveAttackDataAsset(newData);
                newData = null; // �ʱ�ȭ
            }
        }
    }
    private async Task CreateNewAttackData()
    {
        int nextId = await GetNextAttackIdFromServer();

        if (nextId <= 0)
        {
            Debug.LogError("��ȿ���� ���� AttackId");
            newData = null;
        }
        else
        {
            newData = ScriptableObject.CreateInstance<AttackData>();
            newData.AttackId = nextId;
            newData.AttackName = $"New_Attack_{nextId}";
        }

        Repaint(); // GUI �ٽ� �׸����� ���� Ʈ����
    }
    private async void SaveAttackDataAsset(AttackData data)
    {
        bool success = await UploadToDatabase(data);
        if (!success)
        {
            Debug.LogError(" DB ���ε� ����: AttackData ���� ���� �ߴܵ�.");
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

        Debug.Log($"AttackData ���� �Ϸ�: {path}");
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
                    Debug.Log("DB ���ε� ����");
                    return true;
                }
                else
                {
                    Debug.LogError("DB ���ε� ����: " + res.StatusCode);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("���� �߻�: " + ex.Message);
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
                    throw new Exception("���� ���� ����: " + response.StatusCode);

                string result = await response.Content.ReadAsStringAsync();
                int maxId = int.TryParse(result, out int id) ? id : 0;
                return maxId + 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("�������� ID �޾ƿ��� ����: " + ex.Message);
            return -1; // ���� ǥ�ÿ�
        }
    }
}
#endif
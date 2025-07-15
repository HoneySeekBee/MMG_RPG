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
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackData))]
public class AttackDataEditor : Editor
{
    private bool isEditing = false;
    private AttackData originalCopy = null;

    public override void OnInspectorGUI()
    {
        AttackData data = (AttackData)target;

        // �б� ���� ó��
        EditorGUI.BeginDisabledGroup(!isEditing);

        data.AttackName = EditorGUILayout.TextField("Attack Name", data.AttackName);
        data.OwnerType = (OwnerType)EditorGUILayout.EnumPopup("Owner Type", data.OwnerType);
        data.WeaponType = (WeaponType)EditorGUILayout.EnumPopup("Weapon Type", data.WeaponType);
        data.AttackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type", data.AttackType);
        data.Range = EditorGUILayout.FloatField("Range", data.Range);
        data.Angle = EditorGUILayout.FloatField("Angle", data.Angle);
        data.Damage = EditorGUILayout.FloatField("Damage", data.Damage);
        data.Cooldown = EditorGUILayout.FloatField("Cooldown", data.Cooldown);
        data.DelayAfter = EditorGUILayout.FloatField("DelayAfter", data.DelayAfter);
        data.CastTime = EditorGUILayout.FloatField("CastTime", data.CastTime);

        EditorGUI.EndDisabledGroup();

        GUILayout.Space(10);

        if (!isEditing)
        {
            if (GUILayout.Button("�����ϱ�"))
            {
                isEditing = true;

                // ���纻 ����
                originalCopy = ScriptableObject.CreateInstance<AttackData>();
                EditorUtility.CopySerialized(data, originalCopy);
            }
        }
        else
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("DB �����ϱ�"))
            {
                _ = UploadToDatabase(data); // async ����
                isEditing = false;
                originalCopy = null;
            }

            if (GUILayout.Button("���"))
            {
                if (originalCopy != null)
                {
                    EditorUtility.CopySerialized(originalCopy, data);
                    EditorUtility.SetDirty(data); // ������� �ݿ�
                }

                isEditing = false;
                originalCopy = null;
            }

            GUILayout.EndHorizontal();
        }
    }

    private async System.Threading.Tasks.Task UploadToDatabase(AttackData data)
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
                    Debug.Log("DB ���ε� ����!");
                else
                    Debug.LogError("DB ���ε� ����: " + res.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("���� �߻�: " + ex.Message);
        }
    }
}

#endif
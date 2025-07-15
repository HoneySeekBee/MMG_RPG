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

        // 읽기 전용 처리
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
            if (GUILayout.Button("수정하기"))
            {
                isEditing = true;

                // 복사본 생성
                originalCopy = ScriptableObject.CreateInstance<AttackData>();
                EditorUtility.CopySerialized(data, originalCopy);
            }
        }
        else
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("DB 저장하기"))
            {
                _ = UploadToDatabase(data); // async 저장
                isEditing = false;
                originalCopy = null;
            }

            if (GUILayout.Button("취소"))
            {
                if (originalCopy != null)
                {
                    EditorUtility.CopySerialized(originalCopy, data);
                    EditorUtility.SetDirty(data); // 변경사항 반영
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
                    Debug.Log("DB 업로드 성공!");
                else
                    Debug.LogError("DB 업로드 실패: " + res.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("예외 발생: " + ex.Message);
        }
    }
}

#endif
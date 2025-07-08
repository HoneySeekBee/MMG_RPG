using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMG;
using Newtonsoft.Json;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
public class MonsterDataExporter : EditorWindow
{
    private SerializedObject serializedObject;
    private SerializedProperty monsterDataListProperty;
    private ReorderableList reorderableList;

    [SerializeField]
    private List<MonsterData> monsterDataList = new List<MonsterData>();

    [MenuItem("Tools/MMG/Monster Data Exporter")]
    public static void ShowWindow()
    {
        var window = GetWindow<MonsterDataExporter>("Monster Exporter");
        window.minSize = new Vector2(400, 300);
    }


    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        monsterDataListProperty = serializedObject.FindProperty("monsterDataList");

        reorderableList = new ReorderableList(serializedObject, monsterDataListProperty, true, true, true, true);
        reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "MonsterData 리스트 (드래그앤드롭 가능)");
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = monsterDataListProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                element, GUIContent.none);
        };
    }

    private void OnGUI()
    {
        serializedObject.Update();
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);
        DrawDragDropArea();

        GUILayout.Space(20);
        if (GUILayout.Button("JSON으로 저장"))
        {
            ExportToJson();
        }
    }

    private void DrawDragDropArea()
    {
        GUILayout.Label("이 영역에 MonsterData 여러 개를 드래그해서 넣으세요", EditorStyles.helpBox);
        Rect dropArea = GUILayoutUtility.GetLastRect();

        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            if (dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is MonsterData data && !monsterDataList.Contains(data))
                        {
                            monsterDataList.Add(data);
                        }
                    }

                    serializedObject.Update();
                    monsterDataListProperty.serializedObject.ApplyModifiedProperties();
                }

                Event.current.Use();
            }
        }
    }

    private void ExportToJson()
    {
        foreach (var monster in monsterDataList)
        {
            monster.Save(); // 딕셔너리 갱신
        }

        // 여기에서 바로 List<MonsterStatus 형식의 익명 객체>를 만듦
        var monsterExportList = monsterDataList.Select(m => new
        {
            monsterId = m.MonsterId,
            monsterName = m.MonsterName,
            hp = m._MaxHP,
            maxHP = m._MaxHP,
            moveSpeed = m._MoveSpeed,
            chaseRange = m._ChaseRange,
            attackRange = m._AttackRange,
            attacks = m._AttackData.Select(ad => new
            {
                inputType = (int)ad.inputType,
                frequency = ad.AttackAppearanceAmount,
                attackId = ad.attackData.AttackId
            }).ToList()
        }).ToList();

        var rootObject = new { monsterList = monsterExportList };

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            FloatFormatHandling = FloatFormatHandling.String,
            Converters = new List<JsonConverter> { new FloatRoundingConverter(2) } // 소수점 둘째 자리까지
        };

        string json = JsonConvert.SerializeObject(rootObject, settings);

        string path = EditorUtility.SaveFilePanel("Export MonsterData JSON", Application.dataPath, "monster_data", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"MonsterData 저장 완료: {path}");
            EditorUtility.RevealInFinder(path);
        }

    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMG;
using System.IO;
using Packet;
using static MMG.AttackDataListWrapper;
using Newtonsoft.Json;



#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

public class AttackDataExporter : EditorWindow
{
    private SerializedObject serializedObject;
    private SerializedProperty attackDataListProperty;
    private ReorderableList reorderableList;

    [SerializeField]
    private List<AttackData> attackDataList = new List<AttackData>();


    [MenuItem("Tools/MMG/Attack Data Exporter")]
    public static void ShowWindow()
    {
        var window = GetWindow<AttackDataExporter>("Attack Exporter");
        window.minSize = new Vector2(400, 300);
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        attackDataListProperty = serializedObject.FindProperty("attackDataList");

        reorderableList = new ReorderableList(serializedObject, attackDataListProperty, true, true, true, true);
        reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "AttackData 리스트 (드래그앤드롭 가능)");
        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var element = attackDataListProperty.GetArrayElementAtIndex(index);
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

        DrawDragDropArea(); // 드래그 앤 드롭 영역 추가

        GUILayout.Space(20);

        if (GUILayout.Button("JSON으로 저장"))
        {
            ExportToJson();
        }
    }
    private void DrawDragDropArea()
    {
        GUILayout.Label("이 영역에 AttackData 여러 개를 드래그해서 넣으세요", EditorStyles.helpBox);
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
                        if (obj is AttackData data && !attackDataList.Contains(data))
                        {
                            attackDataList.Add(data);
                        }
                    }

                    // 갱신
                    serializedObject.Update();
                    attackDataListProperty.serializedObject.ApplyModifiedProperties();
                }

                Event.current.Use();
            }
        }
    }

    private void ExportToJson()
    {
        List<AttackData> validList = attackDataList.FindAll(data => data != null);
        if (validList.Count == 0)
        {
            Debug.LogWarning("저장할 데이터가 없습니다.");
            return;
        }

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            FloatFormatHandling = FloatFormatHandling.String,
            Converters = new List<JsonConverter> { new FloatRoundingConverter(2) } // 소수점 둘째 자리까지
        };

        string json = JsonConvert.SerializeObject(new { attackList = validList }, settings);

        string path = EditorUtility.SaveFilePanel("Export AttackData JSON", Application.dataPath, "attack_data", "json");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"저장 완료: {path}");
            EditorUtility.RevealInFinder(path);
        }
    }

}

#endif
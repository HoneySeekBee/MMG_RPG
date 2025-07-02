using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class SpawnZoneUpdaterWindow : EditorWindow
{
    [SerializeField] private MapDataSet mapDataSet;

    [MenuItem("Tools/Update Spawn Zones From Scene")]
    public static void OpenWindow()
    {
        GetWindow<SpawnZoneUpdaterWindow>("Spawn Zone Updater");
    }

    private void OnGUI()
    {
        mapDataSet = (MapDataSet)EditorGUILayout.ObjectField("MapDataSet", mapDataSet, typeof(MapDataSet), false);

        if (GUILayout.Button("1. Update MapDataSet from Scene"))
        {
            if (mapDataSet == null)
            {
                Debug.LogError("MapDataSet 비었음");
                return;
            }

            SpawnZoneUpdater.UpdateFromScene(mapDataSet);
        }

        if (GUILayout.Button("2. Export to Server JSON"))
        {
            if (mapDataSet == null)
            {
                Debug.LogError("MapDataSet 비었음");
                return;
            }

            SpawnZoneExporter.ExportToJson(mapDataSet);
        }

        if (GUILayout.Button("한 번에 실행 (Update + Export)"))
        {
            if (mapDataSet == null)
            {
                Debug.LogError("MapDataSet 비었음");
                return;
            }

            SpawnZoneUpdater.UpdateFromScene(mapDataSet);
            SpawnZoneExporter.ExportToJson(mapDataSet);
        }
    }
}
#endif

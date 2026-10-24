using UnityEditor;
using UnityEngine;

public class SurvivorArenaWindow : EditorWindow
{
    private const string ArenaFolder = "Assets/SurvivorArenas";

    private SurvivorArenaConfiguration configuration;
    private SerializedObject serializedConfig;
    private Vector2 scroll;
    private bool foldGeneral = true;
    private bool foldPrefab = true;
    private bool foldZones = true;

    [MenuItem("Tools/Survivor Arena")]
    public static void Open()
    {
        GetWindow<SurvivorArenaWindow>("Survivor Arena");
    }

    private void OnGUI()
    {
        DrawSelector();

        if (configuration == null)
        {
            EditorGUILayout.HelpBox("Selecciona o crea una configuración de arena.", MessageType.Info);
            return;
        }

        if (serializedConfig == null || serializedConfig.targetObject == null)
            serializedConfig = new SerializedObject(configuration);

        serializedConfig.Update();
        EditorGUI.BeginChangeCheck();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawGeneral();
        DrawEnvironmentPrefab();
        DrawSpawnZones();
        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            serializedConfig.ApplyModifiedProperties();
            EditorUtility.SetDirty(configuration);
            SceneView.RepaintAll();
        }
    }

    private void DrawSelector()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        var selected = (SurvivorArenaConfiguration)EditorGUILayout.ObjectField(
            "Configuración", configuration, typeof(SurvivorArenaConfiguration), false);
        if (EditorGUI.EndChangeCheck())
        {
            configuration = selected;
            serializedConfig = configuration != null ? new SerializedObject(configuration) : null;
        }

        if (GUILayout.Button("Nueva", GUILayout.Width(60)))
            CreateNewConfiguration();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        foldGeneral = EditorGUILayout.Foldout(foldGeneral, "Datos generales", true);
        if (!foldGeneral) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedConfig.FindProperty("arenaName"));
        EditorGUILayout.PropertyField(serializedConfig.FindProperty("size"));
        EditorGUILayout.PropertyField(serializedConfig.FindProperty("waveConfiguration"));
        EditorGUI.indentLevel--;
    }

    private void DrawEnvironmentPrefab()
    {
        foldPrefab = EditorGUILayout.Foldout(foldPrefab, "Prefab de entorno", true);
        if (!foldPrefab) return;

        SerializedProperty prefabProp = serializedConfig.FindProperty("environmentPrefab");

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(prefabProp);
        EditorGUI.indentLevel--;

        Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
        GUI.Box(dropRect, "Arrastra aquí un prefab desde la ventana Project");
        HandleDragAndDrop(dropRect, prefabProp);

        var prefab = prefabProp.objectReferenceValue as GameObject;
        if (prefab != null)
        {
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null)
            {
                if (AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
                    Repaint();
                preview = AssetPreview.GetMiniThumbnail(prefab);
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(preview, GUILayout.Width(128), GUILayout.Height(128));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    private void HandleDragAndDrop(Rect dropRect, SerializedProperty prefabProp)
    {
        Event e = Event.current;
        if (e.type != EventType.DragUpdated && e.type != EventType.DragPerform) return;
        if (!dropRect.Contains(e.mousePosition)) return;

        GameObject dragged = null;
        foreach (Object obj in DragAndDrop.objectReferences)
        {
            if (obj is GameObject go && EditorUtility.IsPersistent(go))
            {
                dragged = go;
                break;
            }
        }

        if (dragged == null)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            return;
        }

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (e.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            prefabProp.objectReferenceValue = dragged;
            GUI.changed = true;
        }
        e.Use();
    }


    private void DrawSpawnZones()
    {
        SerializedProperty zones = serializedConfig.FindProperty("spawnZones");
        foldZones = EditorGUILayout.Foldout(foldZones, $"Zonas de spawn ({zones.arraySize})", true);
        if (!foldZones) return;

        EditorGUI.indentLevel++;
        int toRemove = -1;

        for (int i = 0; i < zones.arraySize; i++)
        {
            SerializedProperty zone = zones.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(zone.FindPropertyRelative("name"));
            if (GUILayout.Button("X", GUILayout.Width(24)))
                toRemove = i;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(zone.FindPropertyRelative("position"));
            EditorGUILayout.PropertyField(zone.FindPropertyRelative("radius"));
            EditorGUILayout.EndVertical();

            HandleZoneContextMenu(GUILayoutUtility.GetLastRect(), i);
        }

        if (toRemove >= 0)
            zones.DeleteArrayElementAtIndex(toRemove);

        EditorGUI.indentLevel--;

        if (GUILayout.Button("Añadir zona de spawn"))
        {
            int index = zones.arraySize;
            zones.arraySize++;
            SerializedProperty zone = zones.GetArrayElementAtIndex(index);
            zone.FindPropertyRelative("name").stringValue = $"Spawn Zone {index + 1}";
            zone.FindPropertyRelative("position").vector3Value = Vector3.zero;
            zone.FindPropertyRelative("radius").floatValue = 2f;
        }
    }

    private void HandleZoneContextMenu(Rect rect, int index)
    {
        Event e = Event.current;
        if (e.type != EventType.ContextClick || !rect.Contains(e.mousePosition)) return;

        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Duplicar zona"), false, () => DuplicateZone(index));
        menu.AddItem(new GUIContent("Radio = 1"), false, () => SetZoneRadius(index, 1f));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Eliminar zona"), false, () => DeleteZone(index));
        menu.ShowAsContext();
        e.Use();
    }

    private void DuplicateZone(int index)
    {
        serializedConfig.Update();
        SerializedProperty zones = serializedConfig.FindProperty("spawnZones");
        zones.InsertArrayElementAtIndex(index); // copia el elemento en index + 1
        SerializedProperty copy = zones.GetArrayElementAtIndex(index + 1);
        copy.FindPropertyRelative("name").stringValue += " (Copia)";
        copy.FindPropertyRelative("position").vector3Value += new Vector3(1f, 0f, 1f);
        ApplyAndRepaint();
    }

    private void SetZoneRadius(int index, float radius)
    {
        serializedConfig.Update();
        SerializedProperty zones = serializedConfig.FindProperty("spawnZones");
        zones.GetArrayElementAtIndex(index).FindPropertyRelative("radius").floatValue = radius;
        ApplyAndRepaint();
    }

    private void DeleteZone(int index)
    {
        serializedConfig.Update();
        serializedConfig.FindProperty("spawnZones").DeleteArrayElementAtIndex(index);
        ApplyAndRepaint();
    }

    private void ApplyAndRepaint()
    {
        serializedConfig.ApplyModifiedProperties();
        EditorUtility.SetDirty(configuration);
        SceneView.RepaintAll();
        Repaint();
    }


    private void CreateNewConfiguration()
    {
        if (!AssetDatabase.IsValidFolder(ArenaFolder))
            AssetDatabase.CreateFolder("Assets", "SurvivorArenas");

        var asset = CreateInstance<SurvivorArenaConfiguration>();
        string path = AssetDatabase.GenerateUniqueAssetPath($"{ArenaFolder}/NewSurvivorArena.asset");
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(asset);
        Selection.activeObject = asset;

        configuration = asset;
        serializedConfig = new SerializedObject(asset);
    }
}
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SurvivorArenaVisor))]
public class SurvivorArenaVisorEditor : Editor
{
    private void OnSceneGUI()
    {
        var visor = (SurvivorArenaVisor)target;
        if (visor.configuration == null) return;

        var so = new SerializedObject(visor.configuration);
        so.Update();
        SerializedProperty zones = so.FindProperty("spawnZones");

        for (int i = 0; i < zones.arraySize; i++)
        {
            SerializedProperty zone = zones.GetArrayElementAtIndex(i);
            SerializedProperty posProp = zone.FindPropertyRelative("position");
            SerializedProperty radiusProp = zone.FindPropertyRelative("radius");
            SerializedProperty nameProp = zone.FindPropertyRelative("name");

            Vector3 worldPos = visor.transform.position + posProp.vector3Value;

            Handles.Label(worldPos + Vector3.up * (radiusProp.floatValue + 0.3f), nameProp.stringValue);

            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            float newRadius = Handles.RadiusHandle(Quaternion.identity, worldPos, radiusProp.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(visor.configuration, "Edit Spawn Zone");
                posProp.vector3Value = newPos - visor.transform.position;
                radiusProp.floatValue = Mathf.Max(0.1f, newRadius);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(visor.configuration);
            }
        }
    }
}
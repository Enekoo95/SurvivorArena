using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SurvivorArenaVisor : MonoBehaviour
{
    public SurvivorArenaConfiguration configuration;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (configuration == null) return;

        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(configuration.size.x, 0.1f, configuration.size.y);
        Gizmos.DrawWireCube(transform.position, size);

        Handles.color = Color.red;
        foreach (var zone in configuration.spawnZones)
        {
            Vector3 center = transform.position + zone.position;
            Handles.DrawWireDisc(center, Vector3.up, zone.radius);
        }
    }
#endif
}
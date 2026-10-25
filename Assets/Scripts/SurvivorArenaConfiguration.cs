using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSurvivorArena", menuName = "Survivor Arena/Arena Configuration")]
public class SurvivorArenaConfiguration : ScriptableObject
{
    public string arenaName = "New Arena";
    public Vector2 size = new Vector2(30f, 30f);
    public GameObject environmentPrefab;
    public WaveConfiguration waveConfiguration; 
    public List<SpawnZoneData> spawnZones = new List<SpawnZoneData>();
}
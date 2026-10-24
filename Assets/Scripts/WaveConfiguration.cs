using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Survivor Arena/Wave Configuration")]
public class WaveConfiguration : ScriptableObject
{
    public int enemyCount = 10;
    public float spawnInterval = 1f;
}
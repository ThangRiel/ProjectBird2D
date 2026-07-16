using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelProgression", menuName = "LevelDesign/Level Progression")]
public class LevelProgressionSO : ScriptableObject
{
    public List<ZoneConfigSO> zonesInOrder; //
    public float bossTriggerDistance;
    public GameObject bossEncounterPrefab; // hoặc string bossSceneName
}
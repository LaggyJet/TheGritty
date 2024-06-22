// Worked on by - Joshua Furber
using UnityEngine;

[CreateAssetMenu]

public class EnemyLimiter : ScriptableObject {
    [Range(0, 5)] public int attackAmount, closeRangeAmount, rangeMultiplier;
}
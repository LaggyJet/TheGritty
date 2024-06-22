// Worked on by - Joshua Furber
using UnityEngine;

[CreateAssetMenu]

public class DamageStats : ScriptableObject {
    [Range(0, 1)] public float damage;
    [Range(1, 5)] public int length;
}
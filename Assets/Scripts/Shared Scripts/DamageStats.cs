// Worked on by - Joshua Furber
using UnityEngine;

[CreateAssetMenu]

public class DamageStats : ScriptableObject {
    public enum DoTType { BURN, POISON };
    
    [Range(0, 1)] public float damage;
    [Range(1, 5)] public int length;
    public DoTType type;
}
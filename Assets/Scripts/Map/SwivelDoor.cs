using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwivelDoor : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] int killThreshold;
    [SerializeField] public float angle;
    [SerializeField] public float speed;
    [SerializeField] public int limit;
    public int count;
    public bool swivel;

    readonly List<EnemyLimiter> curLimiters = new();

    void Start() {
        for (int i = 0; i < enemies.Length; i++)
            curLimiters.Add(enemies[i].GetComponent<MeleeAI>().GetEnemyLimiter());
    }

    private void Update() {
        int currentKills = 0;
        foreach (EnemyLimiter limiter in curLimiters)
            if (EnemyManager.Instance.GetEnemyIndex(limiter) != -1)
                currentKills += EnemyManager.Instance.GetKilledEnemyCount(limiter);

        if ((count == limit && limit != 0) || (currentKills >= killThreshold && killThreshold != 0) || swivel)
        {
            Swivel();
        }
    }

    public void Swivel()
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed);
    }
}

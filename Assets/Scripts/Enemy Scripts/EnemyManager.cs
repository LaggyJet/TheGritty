// Worked on by - Joshua Furber
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;

    // Enemy Type, Close Range, Attacking
    readonly List<Tuple<EnemyLimiter, List<int>, List<int>>> currentEnemies = new();
    readonly List<int> enemiesDead = new();
    readonly List<GameObject> aliveEnemies = new();

    void Awake() { Instance = this; }

    public void AddEnemy(GameObject enemy) { aliveEnemies.Add(enemy); }

    public void RemoveEnemy(GameObject enemy) { aliveEnemies.Remove(enemy); }

    public bool IsEnemyAlive(GameObject enemy) { return aliveEnemies.Contains(enemy); }

    public void AddEnemyType(EnemyLimiter type) {
        if (!currentEnemies.Any(tuple => tuple.Item1.Equals(type))) {
            currentEnemies.Add(new Tuple<EnemyLimiter, List<int>, List<int>>(type, new List<int>(type.closeRangeAmount), new List<int>(type.attackAmount)));
            enemiesDead.Add(0);
        }
    }

    public void UpdateKillCounter(EnemyLimiter type) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < enemiesDead.Count)
            enemiesDead[index]++;
    }

    public void ResetKillCounter(EnemyLimiter type) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < enemiesDead.Count)
            enemiesDead[index] = 0;
    }

    public int GetKilledEnemyCount(EnemyLimiter type) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < enemiesDead.Count)
            return enemiesDead[index];
        else
            return 0;
    }

    public int GetEnemyIndex(EnemyLimiter type) { return currentEnemies.FindIndex(tuple => tuple.Item1.Equals(type)); }

    public bool IsClose(EnemyLimiter type, int id) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            return currentEnemies[index].Item2.Contains(id);
        else
            return false;
    }

    public bool CanBeClose(EnemyLimiter type) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            return type.closeRangeAmount == 0 || (currentEnemies[index].Item2.Count < type.closeRangeAmount);
        else
            return false;
    }

    public void AddCloseEnemy(EnemyLimiter type, int id) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            currentEnemies[index].Item2.Add(id);
    }

    public void RemoveCloseEnemy(EnemyLimiter type, int id) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            currentEnemies[index].Item2.Remove(id);
    }

    public bool CanAttack(EnemyLimiter type) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            return type.attackAmount == 0 || currentEnemies[index].Item3.Count < type.attackAmount;
        else
            return false;
    }

    public void AddAttackEnemy(EnemyLimiter type, int id) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            currentEnemies[index].Item3.Add(id);
    }

    public void RemoveAttackEnemy(EnemyLimiter type, int id) {
        int index = GetEnemyIndex(type);
        if (index >= 0 && index < currentEnemies.Count)
            currentEnemies[index].Item3.Remove(id);
    }

    public void ClearEnemies() {
        for (int i = aliveEnemies.Count - 1; i >= 0;) {
            MeleeAI meleeAI = aliveEnemies[i].GetComponent<MeleeAI>();
            if (meleeAI != null) { meleeAI.Kill(); i--; continue; }

            ArcherAI archerAI = aliveEnemies[i].GetComponent<ArcherAI>();
            if (archerAI != null) { archerAI.Kill(); i--; continue; }

            WizardAI wizardAI = aliveEnemies[i].GetComponent<WizardAI>();
            if (wizardAI != null) { wizardAI.Kill(); i--; continue; }

            aliveEnemies.RemoveAt(i);
        }
        currentEnemies.Clear();
        enemiesDead.Clear();
    }
}

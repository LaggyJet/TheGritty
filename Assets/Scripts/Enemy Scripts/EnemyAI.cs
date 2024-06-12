//worked on by - Joshua Furber, natalie lubahn, PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPosition;
    [SerializeField] int hp;
    [SerializeField] float shootRate;
    [SerializeField] GameObject bullet;

    bool isShooting;

    void Start() {
        GameManager.instance.updateEnemy(1);    
    }


    void Update() {
        agent.SetDestination(GameManager.instance.player.transform.position);
        if (!isShooting)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot() {
        isShooting = true;
        Instantiate(bullet, shootPosition.position, transform.rotation);
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void TakeDamage(int amount) {
        hp -= amount;
        StartCoroutine(FlashDamage());
        if (hp <= 0) {
            GameManager.instance.updateEnemy(-1);
            Destroy(gameObject);
        }

    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }
}

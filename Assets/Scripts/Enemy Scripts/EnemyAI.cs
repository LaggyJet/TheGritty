// Worked on by - Joshua Furber, Natalie Lubahn, Kheera, PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] int hp;
    [SerializeField] int animationTransitionSpeed;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int attackSpeed;
    [SerializeField] int swingRadius;
    [SerializeField] GameObject weapon;
    [SerializeField] int damage;

    bool isAttacking;
    bool wasKilled;
    Vector3 playerDirection;

    void Start() { 
        GameManager.instance.updateEnemy(1); 
        weapon.AddComponent<WeaponController>().SetDamage(damage); 
    }


    void Update() {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animationTransitionSpeed));

        if (!wasKilled) {
            agent.SetDestination(GameManager.instance.player.transform.position);
            if (agent.remainingDistance < agent.stoppingDistance)
                FaceTarget();
        }

        if (!isAttacking && agent.remainingDistance < swingRadius)
            StartCoroutine(Swing());
    }

    void FaceTarget() { transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerDirection), Time.deltaTime * faceTargetSpeed); }

    IEnumerator Swing() {
        isAttacking = true;
        weapon.GetComponent<Collider>().enabled = true;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackSpeed);
        weapon.GetComponent<Collider>().enabled = false;
        isAttacking = false;
    }

    public void TakeDamage(int amount) {
        hp -= amount;
        agent.SetDestination(GameManager.instance.player.transform.position);
        StartCoroutine(FlashDamage());
        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
            wasKilled = true;
        }

    }

    IEnumerator DeathAnimation() {
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        agent.radius = 0;
        anim.SetTrigger("Death");
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }
}

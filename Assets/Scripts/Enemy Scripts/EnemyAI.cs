// Worked on by - Joshua Furber, Natalie Lubahn, Kheera, PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] bool flipEnemyDirection;
    [SerializeField] float hp;
    [SerializeField] int animationTransitionSpeed;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int attackSpeed;
    [SerializeField] int swingRadius;
    [SerializeField] GameObject weapon;
    [SerializeField] float damage;

    bool isAttacking, wasKilled;
    Vector3 playerDirection;

    void Start() { 
        isAttacking = wasKilled = false;
        GameManager.instance.updateEnemy(1); 
        weapon.AddComponent<WeaponController>().SetDamage(damage); 
    }


    void Update() {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animationTransitionSpeed));

        if (!wasKilled) {
            agent.SetDestination(GameManager.instance.player.transform.position);

            if (agent.remainingDistance < agent.stoppingDistance || flipEnemyDirection)
                FaceTarget();
        }

        if (!isAttacking && agent.remainingDistance < swingRadius)
            StartCoroutine(Swing());
    }

    void FaceTarget() {
        Quaternion rotation = Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, (flipEnemyDirection ? 180 : 0), 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * faceTargetSpeed); 
    }

    IEnumerator Swing() {
        isAttacking = true;
        weapon.GetComponent<Collider>().enabled = true;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length / 2);
        weapon.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(attackSpeed);
        isAttacking = false;
    }


    public void TakeDamage(float amount) {
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
        List<Renderer> renderers = new List<Renderer>();
        Renderer[] childRenders = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenders);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        while (model.material.color.a > 0) {
            foreach (Renderer render in renderers) {
                float fadeSpeed = render.material.color.a - Time.deltaTime;
                render.material.color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, fadeSpeed);
                yield return null;
            }
        }
        Destroy(gameObject);
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }
}

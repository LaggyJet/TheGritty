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
    [SerializeField] bool canDOT;
    [SerializeField] DamageStats type;

    DamageStats status;
    bool isAttacking, wasKilled, isDOT;
    Vector3 playerDirection;

    void Start() { 
        isAttacking = wasKilled = isDOT = false;
        GameManager.instance.updateEnemy(1); 
        weapon.AddComponent<WeaponController>().SetWeapon(damage, canDOT, type); 
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
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackSpeed);
        isAttacking = false;
    }

    public void WeaponColliderOn() { weapon.GetComponent<Collider>().enabled = true; }

    public void WeaponColliderOff() { weapon.GetComponent<Collider>().enabled = false; }


    public void TakeDamage(float damage) {
        hp -= damage;
        agent.SetDestination(GameManager.instance.player.transform.position);
        if (hp > 0)
            StartCoroutine(FlashDamage());
        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
            wasKilled = true;
        }

    }

    public void Afflict(DamageStats type) {
        status = type;
        if (!isDOT)
            StartCoroutine(DamageOverTime());
    }

    IEnumerator DamageOverTime() {
        isDOT = true;
        for (int i = 0; i < status.length; i++) {
            TakeDamage(status.damage);
            yield return new WaitForSeconds(1);
        }
        isDOT = false;
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    IEnumerator DeathAnimation() {
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        agent.radius = 0;
        anim.SetTrigger("Death");
        var renderers = new List<Renderer>();
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
}
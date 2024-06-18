// Worked on by - Joshua Furber
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static IDamage;

public class SpiderController : MonoBehaviour, IDamage {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] float hp;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int distanceFromPlayer;
    [SerializeField] bool spinReversed;
    [SerializeField] int spitRange;
    [SerializeField] float spitDamage;
    [SerializeField] GameObject spider;
    [SerializeField] int spawnRate;
    [SerializeField] int spawnAmount;

    bool isAttacking, wasKilled, isSpawningSpiders, isDOT;
    DamageStatus status;
    Vector3 playerDirection;
    float currentAngle;

    void Awake() {
        isAttacking = wasKilled = isSpawningSpiders = isDOT = false;
        GameManager.instance.updateEnemy(1);
        agent.speed = distanceFromPlayer * 0.1875f;
        StartCoroutine(CirclePlayer());
    }

    void Update() {
        playerDirection = GameManager.instance.player.transform.position - transform.position;

        // Circling animation
        // anim.SetFloat

        if (!isDOT)
            StartCoroutine(DamageOverTime());

        if (!wasKilled)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerDirection), Time.deltaTime * faceTargetSpeed);

        if (!isSpawningSpiders)
            StartCoroutine(SpawnSpiders());
    }

    IEnumerator SpawnSpiders() {
        isSpawningSpiders = true;
        for (int i = 0; i < spawnAmount; i++) {
            Instantiate(spider, transform.position, transform.rotation);
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(spawnRate);
        isSpawningSpiders = false;
    }

    IEnumerator CirclePlayer() {
        float angleAdjustment = (Mathf.PI * 2) / 360;
        float thresh = 1f;
        while (true) {
            float xOffset = (spinReversed ? (Mathf.Sin(currentAngle)) : (Mathf.Cos(currentAngle))) * distanceFromPlayer;
            float zOffset = (spinReversed ? (Mathf.Cos(currentAngle)) : (Mathf.Sin(currentAngle))) * distanceFromPlayer;
            Vector3 target = new Vector3(GameManager.instance.player.transform.position.x + xOffset, GameManager.instance.player.transform.position.y, GameManager.instance.player.transform.position.z + zOffset);
            agent.SetDestination(target);
            while (Vector3.Distance(agent.transform.position, target) > thresh)
                yield return null;
            currentAngle += angleAdjustment * Time.deltaTime;
            if (currentAngle > Mathf.PI * 2)
                currentAngle -= Mathf.PI * 2;
        }
    }

    IEnumerator Spit() {
        isAttacking = true;
        anim.SetTrigger("Spit");
        // Figure out time for wait then spawn acid ball (seperate script)
        yield return new WaitForSeconds(2);
        isAttacking = false;
    }

    public void TakeDamage(float amount) {

        hp -= amount;
        StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
            wasKilled = true;
        }

        if (!isAttacking)
            StartCoroutine(Spit());
    }

    public void Afflict(DamageStatus type)
    {
        status = type;
    }

    IEnumerator DamageOverTime() {
        isDOT = true;
        Debug.Log("yap");
        switch (status)
        {
            case DamageStatus.BURN:
                TakeDamage(1);
                break;
            case DamageStatus.BLEED:
                TakeDamage(1);
                break;
            case DamageStatus.POISON:
                TakeDamage(1);
                break;
        }

        yield return new WaitForSeconds(1);
        isDOT = false;
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

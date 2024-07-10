// Worked on by - Joshua Furber, Natalie Lubahn, Kheera, PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAI : MonoBehaviourPunCallbacks, IDamage, IPunObservable {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] bool flipEnemyDirection;
    [SerializeField] float hp;
    [SerializeField] int animationTransitionSpeed, faceTargetSpeed, attackSpeed, viewAngle;
    [SerializeField] Transform headPosition;
    [SerializeField] float swingRadius;
    [SerializeField] GameObject weapon;
    [SerializeField] float damage;
    [SerializeField] bool canDOT;
    [SerializeField] DamageStats type;
    [SerializeField] EnemyLimiter enemyLimiter;
    [SerializeField] int range;

    DamageStats status;
    bool isAttacking, wasKilled, isDOT, isChasing;
    Vector3 playerDirection, enemyTargetPosition, networkPosition;
    Quaternion networkRotation;
    float originalStoppingDistance, adjustedStoppingDistance, angleToPlayer;
    int id;

    void Start() {
        isAttacking = wasKilled = isDOT = isChasing = false;
        GameManager.instance.updateEnemy(1);
        weapon.AddComponent<WeaponController>().SetWeapon(damage, canDOT, type);
        EnemyManager.Instance.AddEnemyType(enemyLimiter);
        originalStoppingDistance = agent.stoppingDistance;
        adjustedStoppingDistance = originalStoppingDistance * enemyLimiter.rangeMultiplier;
        id = gameObject.GetInstanceID();
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update() {
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animationTransitionSpeed));

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            HandleHostLogic();
        else if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
            SmoothMovement();
    }

    public EnemyLimiter GetEnemyLimiter() { return enemyLimiter; }

    void HandleHostLogic() {
        if (CanSeePlayer()) {
            isChasing = true;
            agent.SetDestination(enemyTargetPosition);

            if (agent.remainingDistance < agent.stoppingDistance)
                FaceTarget();

            if (!EnemyManager.Instance.IsClose(enemyLimiter, id)) {
                if (EnemyManager.Instance.CanBeClose(enemyLimiter) && agent.remainingDistance < range && !agent.pathPending)
                    EnemyManager.Instance.AddCloseEnemy(enemyLimiter, id);
                else if (!EnemyManager.Instance.CanBeClose(enemyLimiter))
                    agent.stoppingDistance = adjustedStoppingDistance;
            }
            else if (EnemyManager.Instance.IsClose(enemyLimiter, id) && agent.remainingDistance > range) {
                EnemyManager.Instance.RemoveCloseEnemy(enemyLimiter, id);
                agent.stoppingDistance = originalStoppingDistance;
            }

            if (!isAttacking && agent.remainingDistance < swingRadius && EnemyManager.Instance.CanAttack(enemyLimiter))
                StartCoroutine(Swing());
        }
        else
            isChasing = false;
    }

    void SmoothMovement() {
        agent.transform.position = Vector3.Lerp(agent.transform.position, networkPosition, Time.deltaTime * 10);
        agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, networkRotation, Time.deltaTime * 10);
    }

    bool CanSeePlayer() {
        GameObject closestPlayer = FindClosestPlayer();
        if (closestPlayer == null) return false;

        playerDirection = closestPlayer.transform.position - headPosition.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDirection.x, playerDirection.y + 1, playerDirection.z), transform.forward);
        if (flipEnemyDirection) {
            FaceTarget();
            angleToPlayer = 180 - angleToPlayer;
        }

        if (Physics.Raycast(headPosition.position, playerDirection, out RaycastHit hit) && hit.collider.CompareTag("Player") && angleToPlayer < viewAngle && !wasKilled) {
            enemyTargetPosition = closestPlayer.transform.position;
            return true;
        }
        return false;
    }

    GameObject FindClosestPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players) {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    void FaceTarget() {
        Quaternion rotation = Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, (flipEnemyDirection ? 180 : 0), 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * faceTargetSpeed);
    }

    IEnumerator Swing() {
        isAttacking = true;
        anim.SetTrigger("Attack");
        EnemyManager.Instance.AddAttackEnemy(enemyLimiter, id);
        yield return new WaitForSeconds(attackSpeed);
        isAttacking = false;
    }

    public void WeaponColliderOn() { weapon.GetComponent<Collider>().enabled = true; }

    public void WeaponColliderOff() {
        weapon.GetComponent<Collider>().enabled = false;
        EnemyManager.Instance.RemoveAttackEnemy(enemyLimiter, id);
    }

    public void TakeDamage(float damage) {
        hp -= damage;
        if (!isDOT) {
            enemyTargetPosition = GameManager.instance.player.transform.position;
            agent.SetDestination(enemyTargetPosition);
        }

        if (hp > 0)
            StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            EnemyManager.Instance.UpdateKillCounter(enemyLimiter);
            gameObject.GetComponent<Collider>().enabled = false;
            wasKilled = true;
            StartCoroutine(DeathAnimation());
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
        enemyTargetPosition = transform.position;
        agent.SetDestination(enemyTargetPosition);
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
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(agent.transform.position);
            stream.SendNext(agent.transform.rotation);
            stream.SendNext(enemyTargetPosition);
            stream.SendNext(isChasing);
        }
        else {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            enemyTargetPosition = (Vector3)stream.ReceiveNext();
            isChasing = (bool)stream.ReceiveNext();
        }
    }
}
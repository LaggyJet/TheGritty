// Worked on by - Joshua Furber, Natalie Lubahn, Kheera, PJ Glover
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class MeleeAI : MonoBehaviourPun, IDamage, I_Interact, IPunObservable {
    [SerializeField] Renderer model;
    [SerializeField] Material mat;
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
    [SerializeField] float dropChance;
    [SerializeField] GameObject itemToDrop, dotColor;
    
    public List<GameObject> doors;
    DamageStats status;
    bool isAttacking, wasKilled, isDOT;
    Vector3 playerDirection, netPos;
    Quaternion netRot;
    float originalStoppingDistance, adjustedStoppingDistance;
    int id;

    void Start() {
        isAttacking = wasKilled = isDOT = false;
        GameManager.instance.updateEnemy(1);
        weapon.AddComponent<WeaponController>().SetWeapon(damage, canDOT, type);
        EnemyManager.Instance.AddEnemyType(enemyLimiter);
        EnemyManager.Instance.AddEnemy(gameObject);
        originalStoppingDistance = agent.stoppingDistance;
        adjustedStoppingDistance = originalStoppingDistance * enemyLimiter.rangeMultiplier;
        id = gameObject.GetInstanceID();
        if (!photonView.IsMine && PhotonNetwork.IsMasterClient)
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
    }

    void Update() {
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animationTransitionSpeed));

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) {
            GameObject closestPlayer = FindClosestPlayer();
            if (closestPlayer == null) return;
            playerDirection = (closestPlayer.transform.position - transform.position).normalized;
            agent.SetDestination(closestPlayer.transform.position);

            if (!wasKilled) {
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

                if (!isAttacking && agent.remainingDistance < swingRadius && EnemyManager.Instance.CanAttack(enemyLimiter)) {
                    if (PhotonNetwork.InRoom)
                        photonView.RPC(nameof(StartSwing), RpcTarget.All);
                    else
                        StartSwing();
                }
            }
        }

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom) {
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, netRot, Time.deltaTime * 10);
        }
    }
    public EnemyLimiter GetEnemyLimiter() { return enemyLimiter; }

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
        Quaternion rotation = Quaternion.LookRotation(playerDirection);
        if (flipEnemyDirection)
            rotation *= Quaternion.Euler(0, 180, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * faceTargetSpeed);
    }


    [PunRPC]
    void StartSwing() { if (!isAttacking) StartCoroutine(Swing()); }

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
        weapon.GetComponent<WeaponController>().didDamage = false;
    }

    [PunRPC]
    public void RpcTakeDamage(float damage) {
        if (wasKilled)
            return;

        hp -= damage;
        if (!isDOT)
            agent.SetDestination(FindClosestPlayer().transform.position);

        if (hp > 0)
            StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            wasKilled = true;
            DropItem.TryDropItem(dropChance, itemToDrop, 0.6f, gameObject);
            GameManager.instance.updateEnemy(-1);
            EnemyManager.Instance.UpdateKillCounter(enemyLimiter);
            gameObject.GetComponent<Collider>().enabled = false;
            if (PhotonNetwork.InRoom)
                photonView.RPC(nameof(StartDeath), RpcTarget.All);
            else if (!PhotonNetwork.InRoom)
                StartDeath();
        }
    }

    public void TakeDamage(float damage) { 
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RpcTakeDamage), RpcTarget.All, damage);
        else if (!PhotonNetwork.InRoom)
            RpcTakeDamage(damage); 
    }

    public void Afflict(DamageStats type) {
        status = type;
        if (!isDOT)
            StartCoroutine(DamageOverTime());
    }

    IEnumerator DamageOverTime() {
        isDOT = true;
        ParticleSystem.MainModule main = dotColor.GetComponent<ParticleSystem>().main;
        switch (status.type)
        {
            case DamageStats.DoTType.BURN:
                main.startColor = Color.red;

                break;
            case DamageStats.DoTType.POISON:
                main.startColor = Color.green;
                break;
        }
        dotColor.SetActive(true);
        for (int i = 0; i < status.length; i++) {
            TakeDamage(status.damage);
            yield return new WaitForSeconds(1);
        }
        dotColor.SetActive(false);
        isDOT = false;
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    [PunRPC]
    void StartDeath() { if (doors.Count > 0) {CallDoor();} StartCoroutine(DeathAnimation()); }

    IEnumerator DeathAnimation() {
        EnemyManager.Instance.RemoveCloseEnemy(enemyLimiter, id);
        EnemyManager.Instance.RemoveAttackEnemy(enemyLimiter, id);
        if (EnemyManager.Instance.IsEnemyAlive(gameObject))
            EnemyManager.Instance.RemoveEnemy(gameObject);
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        agent.radius = 0;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = false;
        anim.SetTrigger("Death");
        var renderers = new List<Renderer>();
        Renderer[] childRenders = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenders);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        while (model.material.color.a > 0) {
            foreach (Renderer render in renderers) {
                if (render.material.HasProperty("_Color")) {
                    RenderModeAdjuster.SetTransparent(render.material);
                    float fadeSpeed = render.material.color.a - Time.deltaTime;
                    render.material.color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, fadeSpeed);
                }
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
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isAttacking);
        }
        else if (stream.IsReading) {
            netPos = (Vector3)stream.ReceiveNext();
            netRot = (Quaternion)stream.ReceiveNext();
            isAttacking = (bool)stream.ReceiveNext();

            if (isAttacking)
                photonView.RPC(nameof(StartSwing), RpcTarget.All);
        }
    }

    public void CallDoor()
    {
        foreach (GameObject object_ in doors)
        {
            object_.GetComponent<SwivelDoor>().Increment(1);
        }
    }

    public void PassGameObject(GameObject object_)
    {
        doors.Add(object_);
    }

    public void Kill() { TakeDamage(hp); }
}
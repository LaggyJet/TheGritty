using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcherAI : MonoBehaviourPun, IDamage, I_Interact, IPunObservable {
    [SerializeField] Renderer model;
    [SerializeField] Material[] mats;
    [SerializeField] UnityEngine.AI.NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] float hp;
    [SerializeField] int animationTransitionSpeed, faceTargetSpeed, attackSpeed, viewAngle, shootAngle;
    [SerializeField] Transform headPosition;
    [SerializeField] GameObject projectile, shootPos;
    [SerializeField] EnemyLimiter enemyLimiter;
    [SerializeField] int range;
    [SerializeField] float dropChance;
    [SerializeField] GameObject itemToDrop;
    [SerializeField] int retreatRange;
    [SerializeField] bool canMove;

    public List<GameObject> doors;
    DamageStats status;
    bool isAttacking, wasKilled, isDOT, isRetreating;
    Vector3 playerDirection, lastPos;
    float retreatSpeed = 10f, stuckTimeThreshold = 3f, stuckTime = 0f;
    int id;

    void Start() {
        isAttacking = wasKilled = isDOT = isRetreating = false;
        GameManager.instance.updateEnemy(1);
        EnemyManager.Instance.AddEnemyType(enemyLimiter);
        id = gameObject.GetInstanceID();
        lastPos = transform.position;
        if (!photonView.IsMine && PhotonNetwork.IsMasterClient)
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
    }

    void Update() {
        if (!isRetreating)
            anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agent.velocity.normalized.magnitude, Time.deltaTime * animationTransitionSpeed));
        
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) {
            GameObject closestPlayer = FindClosestPlayer();
            if (closestPlayer == null) return;
            playerDirection = (closestPlayer.transform.position - transform.position).normalized;

            if (!wasKilled)
                FaceTarget();

            if (canMove) {
                if (!isAttacking && !isRetreating)
                    agent.SetDestination(closestPlayer.transform.position);
                else if (isAttacking && !isRetreating)
                    agent.SetDestination(transform.position);
                
                if (!isRetreating && (Vector3.Distance(closestPlayer.transform.position, transform.position) < retreatRange))
                    StartCoroutine(Retreat());
            }

            if (!isAttacking && EnemyManager.Instance.CanAttack(enemyLimiter) && (Vector3.Distance(closestPlayer.transform.position, transform.position) < range)) {
                if (PhotonNetwork.InRoom)
                    photonView.RPC(nameof(Shoot), RpcTarget.All);
                else
                    Shoot();
            }
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
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * faceTargetSpeed);
    }

    [PunRPC]
    void Shoot() { if (!isAttacking) StartCoroutine(ShootArrow()); }

    IEnumerator ShootArrow() {
        isAttacking = true;
        anim.SetTrigger("Shoot");
        EnemyManager.Instance.AddAttackEnemy(enemyLimiter, id);
        yield return new WaitForSeconds(attackSpeed);
        EnemyManager.Instance.RemoveAttackEnemy(enemyLimiter, id);
        isAttacking = false;
    }

    void QuickShot() {
        if (PhotonNetwork.InRoom && photonView.IsMine)
            PhotonNetwork.Instantiate("Enemy/" + projectile.name, shootPos.transform.position, new Quaternion(0, 180, 0, 0));
        else if (!PhotonNetwork.InRoom)
            Instantiate(projectile, shootPos.transform.position, new Quaternion(0, 180, 0, 0));
    }

    IEnumerator Retreat() {
        isRetreating = true;
        anim.SetTrigger("Retreat");
        GameObject closestPlayer = FindClosestPlayer();
        if (closestPlayer == null) {
            isRetreating = false;
            yield break;
        }
        Vector3 retreatDirection = (transform.position - closestPlayer.transform.position).normalized;
        float stoppingDistance = agent.stoppingDistance;
        Vector3 retreatTargetPosition = transform.position + retreatDirection * (retreatSpeed + stoppingDistance);
        lastPos = transform.position;
        while (Vector3.Distance(closestPlayer.transform.position, transform.position) < retreatRange) {
            Vector3 targetPosition = transform.position + retreatDirection * (retreatSpeed + stoppingDistance);
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
                retreatTargetPosition = hit.position;
            
            agent.SetDestination(retreatTargetPosition); 
            Quaternion targetRotation = Quaternion.LookRotation(retreatDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * faceTargetSpeed);
            if (Vector3.Distance(transform.position, lastPos) < 0.05f)
                stuckTime += Time.deltaTime;
            else
                stuckTime = 0f;
            
            if (stuckTime >= stuckTimeThreshold) {
                float randomAngle = Random.Range(0f, 360f);
                retreatDirection = Quaternion.Euler(0, randomAngle, 0) * retreatDirection;
                retreatTargetPosition = transform.position + retreatDirection * (retreatSpeed + stoppingDistance);
                stuckTime = 0f;
            }
            lastPos = transform.position;
            yield return null;
        }
        isRetreating = false;
    }


    [PunRPC]
    public void RpcTakeDamage(float damage) {
        if (wasKilled)
            return;
        
        hp -= damage;
        
        if (hp > 0)
            StartCoroutine(FlashDamage());
        
        if (hp <= 0 && !wasKilled) {
            DropItem.TryDropItem(dropChance, itemToDrop, 0.6f, gameObject);
            GameManager.instance.updateEnemy(-1);
            EnemyManager.Instance.UpdateKillCounter(enemyLimiter);
            gameObject.GetComponent<Collider>().enabled = false;
            wasKilled = true;
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

    [PunRPC]
    void StartDeath() { if (doors.Count > 0) {CallDoor();} StartCoroutine(DeathAnimation()); }

    IEnumerator DeathAnimation() {
        // Sound for Death 
        PlayerController.instance.PlaySkeletonAud();
        EnemyManager.Instance.RemoveCloseEnemy(enemyLimiter, id);
        EnemyManager.Instance.RemoveAttackEnemy(enemyLimiter, id);
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
        yield return new WaitForSeconds(3);
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
        if (stream.IsWriting)
            stream.SendNext(isAttacking);
        else if (stream.IsReading)
            isAttacking = (bool)stream.ReceiveNext();
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
}
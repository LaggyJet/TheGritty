using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcherAI : MonoBehaviourPun, IDamage, I_Interact, IPunObservable {
    [SerializeField] Renderer model;
    [SerializeField] Material[] mats;
    [SerializeField] Animator anim;
    [SerializeField] float hp;
    [SerializeField] int animationTransitionSpeed, faceTargetSpeed, attackSpeed, viewAngle, shootAngle;
    [SerializeField] Transform headPosition;
    [SerializeField] GameObject projectile, shootPos;
    [SerializeField] EnemyLimiter enemyLimiter;
    [SerializeField] int range;
    [SerializeField] float dropChance;
    [SerializeField] GameObject itemToDrop;

    GameObject[] doors;
    DamageStats status;
    bool isAttacking, wasKilled, isDOT;
    Vector3 playerDirection, enemyTargetPosition;
    float angleToPlayer;
    int id;

    void Start() {
        isAttacking = wasKilled = isDOT = false;
        GameManager.instance.updateEnemy(1);
        EnemyManager.Instance.AddEnemyType(enemyLimiter);
        id = gameObject.GetInstanceID();
        if (!photonView.IsMine && PhotonNetwork.IsMasterClient)
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
    }

    void Update() {
        if (CanSeePlayer() && (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected) && (Vector3.Distance(FindClosestPlayer().transform.position, transform.position) < range)) {
            FaceTarget();

            if (!isAttacking && EnemyManager.Instance.CanAttack(enemyLimiter)) {
                if (PhotonNetwork.IsConnected)
                    photonView.RPC(nameof(Shoot), RpcTarget.All);
                else
                    Shoot();
            }
        }
    }

    public EnemyLimiter GetEnemyLimiter() { return enemyLimiter; }

    bool CanSeePlayer() {
        GameObject closestPlayer = FindClosestPlayer();
        if (closestPlayer == null) return false;

        playerDirection = closestPlayer.GetComponent<PlayerController>().headPosition.transform.position - headPosition.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDirection.x, playerDirection.y + 2, playerDirection.z), transform.forward);

        if (Physics.Raycast(headPosition.position, playerDirection, out RaycastHit hit) && (hit.collider.CompareTag("Player") || hit.collider.CompareTag("PlayerChild")) && angleToPlayer < viewAngle && !wasKilled) {
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
        else if (!PhotonNetwork.IsConnected)
            Instantiate(projectile, shootPos.transform.position, new Quaternion(0, 180, 0, 0));
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
            else if (!PhotonNetwork.IsConnected)
                StartDeath();
        }
    }

    public void TakeDamage(float damage) { 
        if (PhotonNetwork.InRoom)
            photonView.RPC(nameof(RpcTakeDamage), RpcTarget.All, damage);
        else if (!PhotonNetwork.IsConnected)
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
    void StartDeath() { if (doors.Length > 0) {CallDoor();} StartCoroutine(DeathAnimation()); }

    IEnumerator DeathAnimation() {
        enemyTargetPosition = transform.position;
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
        foreach(GameObject object_ in doors)
        {
            object_.GetComponent<SwivelDoor>().Increment(1);
        }
    }

    public void PassGameObject(GameObject object_)
    {
        doors.Append(object_);
    }
}

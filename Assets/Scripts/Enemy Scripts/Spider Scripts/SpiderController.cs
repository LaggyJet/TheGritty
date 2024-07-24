// Worked on by - Joshua Furber, Kheera 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class SpiderController : MonoBehaviourPunCallbacks, IDamage, IPunObservable {
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [Header("------ Audio ------")]
    [SerializeField] public AudioSource spiderAud;
    [SerializeField] AudioClip spiderWalking;
    [SerializeField] public float spiderWalkingVol;
    public bool isPlayingSpiderWalking;
    // For access
    public static SpiderController instance;
    [SerializeField] float hp;
    [SerializeField] int faceTargetSpeed, distanceFromPlayer, spitCooldown;
    [SerializeField] GameObject spitEffectPS, poolEmitter, acidStream, acidPuddle, spider;
    [SerializeField] int spawnRate, spawnAmount;
    [SerializeField] float dropChance;
    [SerializeField] GameObject itemToDrop, dotColor;

    DamageStats status;
    bool isAttacking, wasKilled, isSpawningSpiders, onCooldown, isDOT;
    Vector3 playerDirection, target, networkPosition, randomTargetPosition;
    Quaternion networkRotation, lastRot;
    float currentAngle, lastMovementTime, randomMovementInterval = 15f;

    void Start() {
        isAttacking = wasKilled = isSpawningSpiders = onCooldown = isDOT = false;
        GameManager.instance.updateEnemy(1);
        agent.speed = distanceFromPlayer * 0.1875f;
        lastMovementTime = Time.time;
        if (!photonView.IsMine && PhotonNetwork.IsMasterClient)
            photonView.TransferOwnership(PhotonNetwork.MasterClient);
        GameObject bossFloor = GameObject.Find("Boss_Floor");
        if (bossFloor != null) {
            ParticleSystem particleSystem = poolEmitter.GetComponent<ParticleSystem>();
            ParticleSystem.CollisionModule collisionModule = particleSystem.collision;
            collisionModule.enabled = true;

            List<Transform> planes = new();
            for (int i = 0; i < collisionModule.planeCount; i++)
                planes.Add(collisionModule.GetPlane(i));

            planes.Add(bossFloor.transform);

            for (int i = 0; i < planes.Count; i++)
                collisionModule.SetPlane(i, planes[i]);
        }
    }

    void Update() {
        playerDirection = FindClosetPlayer().transform.position - transform.position;
        if (!isAttacking)
            lastRot = transform.rotation;

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) {
            if (!wasKilled) { 
                //Find some way of fixing the spider tilting when looking at player
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerDirection), Time.deltaTime * faceTargetSpeed);
                
            }

            if (!isSpawningSpiders && !isAttacking && !wasKilled)
                StartCoroutine(SpawnSpiders());

            if (!isAttacking && !wasKilled) {
                agent.isStopped = false;
                if (Time.time - lastMovementTime >= randomMovementInterval) {
                    SetRandomTargetPosition();
                    lastMovementTime = Time.time;
                }

                if (agent.remainingDistance < 0.1f)
                    SetRandomTargetPosition();
            }
            else if (isAttacking) {
                transform.rotation = lastRot;
                agent.isStopped = true;
            }
        }
        else if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom) {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
        }

        // Play spider sounds when spider is walking 
        if (agent.velocity.magnitude > 0.1f && !spiderAud.isPlaying) {
            spiderAud.PlayOneShot(spiderWalking, spiderWalkingVol);
            isPlayingSpiderWalking = true;
        }
        else if (agent.velocity.magnitude <= 0.1f && spiderAud.isPlaying) {
            spiderAud.Stop();
            isPlayingSpiderWalking = false;
        }
    }

    private void SetRandomTargetPosition() {
        lastMovementTime = Time.time;
        Vector3 randomDirection = Random.insideUnitSphere * 40f;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 40f, NavMesh.AllAreas)) {
            randomTargetPosition = hit.position;
            agent.SetDestination(randomTargetPosition);
        }
    }

    public void PauseSpiderSounds() {
        if (spiderAud.isPlaying) {
            spiderAud.Pause();
            isPlayingSpiderWalking = false;
        }
    }

    public void ResumeSpiderSounds() {
        if (!spiderAud.isPlaying && agent.velocity.magnitude > 0.1f) {
            spiderAud.PlayOneShot(spiderWalking, spiderWalkingVol);
            isPlayingSpiderWalking = true;
        }
    }

    IEnumerator SpawnSpiders() {
        isSpawningSpiders = true;
        for (int i = 0; i < spawnAmount; i++) {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate("Enemy/" + spider.name, transform.position, Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, 180, 0));
            else if (!PhotonNetwork.InRoom)
                Instantiate(spider, transform.position, Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, 180, 0));
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(spawnRate);
        isSpawningSpiders = false;
    }

    GameObject FindClosetPlayer() {
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

    void TrySpit() {
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom)
            StartCoroutine(Spit());
        else
            photonView.RPC(nameof(CheckSpit), RpcTarget.MasterClient);
    }

    [PunRPC]
    void CheckSpit() {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Spit());
    }

    [PunRPC]
    void SyncSpit() {
        if (!isAttacking && !onCooldown)
            StartCoroutine(Spit());
    }

    IEnumerator Spit() {
        onCooldown = true;
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(SyncSpit), RpcTarget.All);
        ParticleSystem particleSystem = spitEffectPS.GetComponent<ParticleSystem>();
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        spitEffectPS.SetActive(true);
        particleSystem.Play();
        ParticleSystem.SubEmittersModule subEmitters = particleSystem.subEmitters;
        for (int i = 0; i < subEmitters.subEmittersCount; i++)
            subEmitters.GetSubEmitterSystem(i)?.Play();
        agent.isStopped = isAttacking = true;
        yield return new WaitForSeconds(0.1f);
        anim.enabled = false;
        StartCoroutine(SpawnTracers());
        yield return new WaitForSeconds(particleSystem.main.duration);
        spitEffectPS.SetActive(false);
        anim.enabled = true;
        anim.SetTrigger("Circle");
        agent.isStopped = isAttacking = false;
        yield return new WaitForSeconds(spitCooldown);
        onCooldown = false;
    }

    IEnumerator SpawnTracers() {
        ParticleSystem pS = spitEffectPS.GetComponent<ParticleSystem>();
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[pS.main.maxParticles];
        List<GameObject> curTracers = new List<GameObject>();
        while (curTracers.Count < 5) {
            curTracers.Add(PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine ? 
                PhotonNetwork.Instantiate("Enemy/" + acidStream.name, Vector3.zero, Quaternion.identity) : 
                Instantiate(acidStream));
            yield return null;
        }
        while (true) {    
            if (wasKilled) {
                spitEffectPS.SetActive(false);
                yield break;
            }

            int particlesLeft = pS.GetParticles(particles);
            
            for (int i = 0; i < curTracers.Count; i++) {
                if (i < particlesLeft && particles[i].remainingLifetime > 0f) {
                    curTracers[i].transform.position = particles[i].position;
                    Collider[] hitColliders = Physics.OverlapSphere(curTracers[i].transform.position, 0.1f);
                    foreach (var hitCollider in hitColliders) {
                        if (hitCollider.gameObject.name == "Boss_Floor") {
                            if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine) {
                                PhotonNetwork.Instantiate("Enemy/" + acidPuddle.name, curTracers[i].transform.position, Quaternion.identity);
                                PhotonNetwork.Destroy(curTracers[i]);
                            } else if (!PhotonNetwork.InRoom) {
                                Instantiate(acidPuddle, curTracers[i].transform.position, Quaternion.identity);
                                Destroy(curTracers[i]);
                            }
                            curTracers.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                } else {
                    if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine) {
                        PhotonNetwork.Instantiate("Enemy/" + acidPuddle.name, curTracers[i].transform.position, Quaternion.identity);
                        PhotonNetwork.Destroy(curTracers[i]);
                    } else if (!PhotonNetwork.InRoom) {
                        Instantiate(acidPuddle, curTracers[i].transform.position, Quaternion.identity);
                        Destroy(curTracers[i]);
                    }
                    curTracers.RemoveAt(i);
                    i--;
                }
            }
            if (curTracers.Count == 0) {
                break;
            }
            yield return null;
        }
    }

    [PunRPC]
    public void RpcTakeDamage(float damage) {
        if (wasKilled)
            return;

        hp -= damage;
        if (hp > 0)
            StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            //Uncomment line below if adding item for queen spider (make to specify height)
            //DropItem.TryDropItem(dropChance, itemToDrop, heightofitem, gameObject);
            wasKilled = true;
            StopCoroutine(Spit());
            StopCoroutine(SpawnSpiders());
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
        }

        if (!isAttacking && !onCooldown)
            TrySpit();
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

    bool IsSpitParticles(Transform transform) {
        while (transform != null) {
            if (transform.name == "SpitPosition")
                return true;
            transform = transform.parent;
        }
        return false;
    }

    IEnumerator DeathAnimation() {
        yield return new WaitForSeconds(0.2f);
        agent.isStopped = true;
        agent.SetDestination(transform.position);
        agent.radius = 0;
        anim.SetTrigger("Death");
        transform.rotation = lastRot;
        var renderers = new List<Renderer>();
        Renderer[] childRenderers = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenderers);
        yield return new WaitForSeconds(3);
        while (model.material.color.a > 0) {
            foreach (Renderer render in renderers) {
                if (IsSpitParticles(render.transform))
                    continue;
                if (render.material.HasProperty("_Color")) {
                    RenderModeAdjuster.SetTransparent(render.material);
                    float fadeSpeed = render.material.color.a - Time.deltaTime;
                    render.material.color = new Color(render.material.color.r, render.material.color.g, render.material.color.b, fadeSpeed);
                }
                yield return null;
            }
        }
        yield return new WaitForSeconds(1);

        // Call win game for every player in room through RPC calls, otherwise call normally
        if (PhotonNetwork.InRoom && photonView && photonView.IsMine)
            photonView.RPC(nameof(Win), RpcTarget.All);
        else if (!PhotonNetwork.InRoom)
            Win();
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    [PunRPC]
    private void Win() {
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);

        GameManager.instance.gameWon();
        GameManager.instance.hasBossDied = true;
        DataPersistenceManager.Instance.SaveGame();
        GameManager.playerLocation = GameManager.instance.player.transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isAttacking);
            stream.SendNext(isSpawningSpiders);
            stream.SendNext(onCooldown);
        }
        else {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            isAttacking = (bool)stream.ReceiveNext();
            isSpawningSpiders = (bool)stream.ReceiveNext();
            onCooldown = (bool)stream.ReceiveNext();
        }
    }
}
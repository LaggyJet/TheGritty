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
    [SerializeField] GameObject spitEffectPS, acidStream, acidPuddle, spider;
    [SerializeField] int spawnRate, spawnAmount;

    DamageStats status;
    bool isAttacking, wasKilled, isSpawningSpiders, onCooldown, isDOT;
    Vector3 playerDirection, target, networkPosition;
    Quaternion networkRotation;
    float currentAngle;
    

    void Start() {
        spitEffectPS.SetActive(false);
        isAttacking = wasKilled = isSpawningSpiders = onCooldown = isDOT = false;
        GameManager.instance.updateEnemy(1);
        agent.speed = distanceFromPlayer * 0.1875f;
        StartCoroutine(CirclePlayer());
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update() {
        playerDirection = FindClosetPlayer().transform.position - transform.position;

        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            HandleHostLogic();
        else if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
            SmoothMovement();

            // Play spider sounds when spider is walking 
        if(agent.velocity.magnitude > 0.1f && !spiderAud.isPlaying)
        {
            spiderAud.PlayOneShot(spiderWalking, spiderWalkingVol);
            isPlayingSpiderWalking = true;
            Debug.Log("Spider Walking sounds playing");
        }
        else if(agent.velocity.magnitude <= 0.1f && spiderAud.isPlaying)
        {
            spiderAud.Stop();
            isPlayingSpiderWalking = false;
        }
    }

    void HandleHostLogic() {
        if (!wasKilled) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerDirection), Time.deltaTime * faceTargetSpeed);

            if (!isSpawningSpiders && !isAttacking)
                StartCoroutine(SpawnSpiders());
        }
    }

    void SmoothMovement() {
        agent.transform.position = Vector3.Lerp(agent.transform.position, networkPosition, Time.deltaTime * 10);
        agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, networkRotation, Time.deltaTime * 10);

        
    }

    public void PauseSpiderSounds()
    {
      if(spiderAud.isPlaying)
      {
        spiderAud.Pause();
        isPlayingSpiderWalking = false;
      }
    }

    public void ResumeSpiderSounds()
    {
       if (!spiderAud.isPlaying && agent.velocity.magnitude > 0.1f)
        {
            spiderAud.PlayOneShot(spiderWalking, spiderWalkingVol);
            isPlayingSpiderWalking = true;
            Debug.Log("Spider Walking sounds playing");
        }
    }

    IEnumerator SpawnSpiders() {
        isSpawningSpiders = true;
        for (int i = 0; i < spawnAmount; i++) {
            if (PhotonNetwork.InRoom)
                PhotonNetwork.Instantiate("Enemy" + spider.name, transform.position, Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, 180, 0));
            else if (!PhotonNetwork.InRoom)
                Instantiate(spider, transform.position, Quaternion.LookRotation(playerDirection) * Quaternion.Euler(0, 180, 0));
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(spawnRate);
        isSpawningSpiders = false;
    }

    IEnumerator CirclePlayer() {
        float angleAdjustment = (Mathf.PI * 2) / 360;
        float thresh = 1f;
        while (true) {
            if (isAttacking) {
                agent.SetDestination(transform.position);
                yield return null;
            }
            else {
                float xOffset = Mathf.Sin(currentAngle) * distanceFromPlayer;
                float zOffset = Mathf.Cos(currentAngle) * distanceFromPlayer;
                GameObject closestPlayer = FindClosetPlayer();
                target = new Vector3(closestPlayer.transform.position.x + xOffset, closestPlayer.transform.position.y, closestPlayer.transform.position.z + zOffset);
                agent.SetDestination(target);
                while (Vector3.Distance(agent.transform.position, target) > thresh)
                    yield return null;
                currentAngle += angleAdjustment * Time.deltaTime;
                if (currentAngle > Mathf.PI * 2)
                    currentAngle -= Mathf.PI * 2;
            }
        }
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

    IEnumerator Spit() {
        isAttacking = onCooldown = true;
        yield return new WaitForSeconds(0.1f);
        anim.enabled = false;
        spitEffectPS.SetActive(true);
        StartCoroutine(SpawnTracers());
        yield return new WaitForSeconds(4);
        spitEffectPS.SetActive(false);
        anim.enabled = true;
        anim.SetTrigger("Circle");
        isAttacking = false;
        yield return new WaitForSeconds(spitCooldown);
        onCooldown = false;
    }

    IEnumerator SpawnTracers() {
        yield return new WaitForSeconds(1);
        ParticleSystem pS = spitEffectPS.GetComponent<ParticleSystem>();
        ParticleSystem.Particle[] p = new ParticleSystem.Particle[pS.main.maxParticles];
        List<GameObject> curTracers = new();

        while (curTracers.Count != 5) {
            curTracers.Add((PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine) ? PhotonNetwork.Instantiate("Enemy/" + acidStream.name, Vector3.zero, Quaternion.identity) : Instantiate(acidStream));
            yield return null;
        }

        int particlesLeft = 0;
        while (particlesLeft == 0) {
            particlesLeft = pS.GetParticles(p);
            yield return null;
        }

        while (particlesLeft != 0) {
            particlesLeft = pS.GetParticles(p);
            for (int i = 0, j = i * 3; i < curTracers.Count && j < particlesLeft; i++, j *= 3) {
                if (i < particlesLeft && p[j].remainingLifetime > 0f)
                    curTracers[i].transform.position = p[j].position;
                else {
                    if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine) {
                        PhotonNetwork.Instantiate("Enemy/" + acidPuddle.name, curTracers[i].transform.position, Quaternion.identity);
                        PhotonNetwork.Destroy(curTracers[i]);
                    }
                    else if (!PhotonNetwork.InRoom) {
                        Instantiate(acidPuddle, curTracers[i].transform.position, Quaternion.identity);
                        Destroy(curTracers[i]);
                    }
                    curTracers.RemoveAt(i);
                    i--;
                }
            }
            
            yield return null;
        }
    }

    public void TakeDamage(float amount) {
        hp -= amount;
        if (hp > 0)
            StartCoroutine(FlashDamage());

        if (hp <= 0 && !wasKilled) {
            GameManager.instance.updateEnemy(-1);
            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(DeathAnimation());
            wasKilled = true;
        }

        if (!isAttacking && !onCooldown)
            StartCoroutine(Spit());
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
        var renderers = new List<Renderer>();
        Renderer[] childRenders = transform.GetComponentsInChildren<Renderer>();
        renderers.AddRange(childRenders);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length * 2);
        foreach (Renderer render in renderers) {
            if (IsSpitParticles(render.transform))
                continue;
            float newAlpha = render.material.GetFloat("_Alpha");
            while (newAlpha > 0) {
                newAlpha -= 0.5f * Time.deltaTime;
                render.material.SetFloat("_Alpha", newAlpha);
                yield return null;
            }
        }

        // Call win game for every player in room through RPC calls, otherwise call normally
        if (PhotonNetwork.InRoom && photonView && photonView.IsMine)
            photonView.RPC(nameof(Win), RpcTarget.All);
        else if (!PhotonNetwork.IsConnected)
            Win();

        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }

    IEnumerator FlashDamage() {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = new Color(0.5f, 0.5f, 0.5f, 1);
    }

    [PunRPC]
    private void Win() { 
        GameManager.instance.gameWon();
        DataPersistenceManager.Instance.SaveGame();
        GameManager.playerLocation = GameManager.instance.player.transform.position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) { 
            stream.SendNext(agent.transform.position);
            stream.SendNext(agent.transform.rotation);
            stream.SendNext(target);
            stream.SendNext(isAttacking);
            stream.SendNext(isSpawningSpiders);
            stream.SendNext(onCooldown);
        }
        else {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            target = (Vector3)stream.ReceiveNext();
            isAttacking = (bool)stream.ReceiveNext();
            isSpawningSpiders = (bool)stream.ReceiveNext();
            onCooldown = (bool)stream.ReceiveNext();
        }
    }
}
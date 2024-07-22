//Worked on by : Jacob Irvin, Joshua Furber

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FireSpray : MonoBehaviourPunCallbacks
{
    //the body of our projectile that will handle our physics
    [SerializeField] Rigidbody rb;

    //game variables that may be tweaked
    [SerializeField] float damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] DamageStats type; 
    [SerializeField] float minimumLight, maximumLight;

    //variable to be used in the lighting
    [SerializeField] new Light light;


    // Start is called before the first frame update
    void Start()
    {
        //moves our projectile forward based on its speed
        rb.velocity = transform.forward * speed;
        //after being alive so long our projectile will die
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            StartCoroutine(WaitThenDestroy(gameObject, destroyTime));
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject, destroyTime);
        light.intensity = Random.Range(minimumLight, maximumLight);
    }
    void Update()
    {
        light.intensity = Random.Range(light.intensity + 0.2f, light.intensity - 0.2f);
    }

    IEnumerator WaitThenDestroy(GameObject obj, float seconds) {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.Destroy(obj);
    }

    private void OnTriggerEnter(Collider other)
    {
        //when encountering a collision trigger it checks for component IDamage
        IDamage dmg = other.GetComponent<IDamage>();

        //if there is an IDamage component we run the inside code
        if (dmg != null && !other.gameObject.CompareTag("Player"))
        {
            if (SkillTreeManager.Instance.IsSkillUnlocked(SkillTreeManager.Skills.ATTACK_DAMAGE_UP))
                damage *= 1.5f;

            //deal damage to the object hit
            dmg.TakeDamage(3);
            dmg.Afflict(type);
            //destroy our projectile
            DestroyObject(gameObject);
        }
        else if (!other.gameObject.CompareTag("Player") && !other.isTrigger)
            DestroyObject(gameObject);

    }

    void DestroyObject(GameObject obj) {
        if (PhotonNetwork.InRoom && GetComponent<PhotonView>().IsMine)
            PhotonNetwork.Destroy(gameObject);
        else if (!PhotonNetwork.InRoom)
            Destroy(gameObject);
    }
}

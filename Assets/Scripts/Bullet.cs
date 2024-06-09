//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//
//public class Bullet : MonoBehaviour {
//    [SerializeField] Rigidbody rb;
//    [SerializeField] int damage;
//    [SerializeField] int speed;
//    [SerializeField] int destroyTime;
//
//    void Start() {
//        rb.velocity = transform.forward * speed;
//        Destroy(gameObject, destroyTime);
//    }
//
//    private void OnTriggerEnter(Collider other) {
//        IDamage damageCheck = other.GetComponent<IDamage>();
//        if (damageCheck != null)
//            damageCheck.TakeDamage(damage);
//
//        Destroy(gameObject);
//    }
//}

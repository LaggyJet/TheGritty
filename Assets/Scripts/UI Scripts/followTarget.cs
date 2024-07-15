using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    // Update is called once per frame
    void Update()
    {
        //sets the value of the target as long as the game manager and player aren't null!
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Transform>();
        }
        if (target != null) //updates the position each frame
        {
            transform.position = target.transform.position + offset;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followYrotation : MonoBehaviour
{
    public Transform target;
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
            transform.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
        }
    }
}

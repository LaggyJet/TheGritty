using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followYrotation : MonoBehaviour
{
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
            target = GameManager.instance.player.GetComponent<Transform>();
        transform.eulerAngles = new Vector3(0,target.eulerAngles.y,0);
    }
}

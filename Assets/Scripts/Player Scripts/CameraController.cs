//Worked on by : Jacob Irvin

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;


public class CameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;

    float rotX;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //get input
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        
        rotX -= mouseY;

        //clamp rotX on the xAxis
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);

        if (PhotonNetwork.InRoom && !GameManager.instance.isPaused) {
            //rotate the camera on the xAxis
            transform.localRotation = Quaternion.Euler(rotX, 0, 0);

            //rotate the player on the yAxis
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Class_Archer : MonoBehaviour
{
    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void OnPrimaryFire(InputAction.CallbackContext ctxt)
    {

        if (ctxt.performed && !GameManager.instance.isShooting && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {

        }
    }
    public void OnSecondaryFire(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed && !GameManager.instance.isPaused && SceneManager.GetActiveScene().name != "title menu")
        {

        }
        else if (ctxt.canceled && GameManager.instance.isShooting)
        {

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

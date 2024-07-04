//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindManager : MonoBehaviour
{
    public InputActionReference moveRef;
    public InputActionReference jumpRef;
    public InputActionReference sprintRef;
    public InputActionReference primaryFireRef;
    public InputActionReference secondaryFireRef;
    public InputActionReference abilityRef;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        moveRef.action.Disable();
        jumpRef.action.Disable();
        sprintRef.action.Disable();
        primaryFireRef.action.Disable();
        secondaryFireRef.action.Disable();
        abilityRef.action.Disable();
    }
    private void OnDisable()
    {
        moveRef.action.Enable();
        jumpRef.action.Enable();
        sprintRef.action.Enable();
        primaryFireRef.action.Enable();
        secondaryFireRef.action.Enable();
        abilityRef.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

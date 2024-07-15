//worked on by - natalie lubahn
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindManager : MonoBehaviour
{
    public InputActionAsset controls;
    public Dictionary<System.Guid, string> overrides;
    public InputActionReference moveRef;
    public InputActionReference jumpRef;
    public InputActionReference sprintRef;
    public InputActionReference primaryFireRef;
    public InputActionReference secondaryFireRef;
    public InputActionReference abilityRef;
    // Start is called before the first frame update
    private void Start()
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
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            controls.LoadBindingOverridesFromJson(rebinds);
    }
    private void OnDisable()
    {
        moveRef.action.Enable();
        jumpRef.action.Enable();
        sprintRef.action.Enable();
        primaryFireRef.action.Enable();
        secondaryFireRef.action.Enable();
        abilityRef.action.Enable();
        var rebinds = controls.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}

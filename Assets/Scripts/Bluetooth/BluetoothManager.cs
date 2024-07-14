using UnityEngine;
using ArduinoBluetoothAPI;
using System;
using System.Collections;
using Photon.Pun;
using TMPro;

public class BluetoothManager : MonoBehaviour {
    public static BluetoothManager instance;
    private TMP_Text bluetoothMessage;
    private BluetoothHelper helper;
    readonly private string deviceName = "HC-06";
    enum DebugState { Health, Stamina };
    DebugState curState = DebugState.Health;
    readonly private char key = '=';

    void Awake() {
        bluetoothMessage = GameObject.Find("title ui").transform.Find("bluetooth message").GetComponent<TMP_Text>();
        StartCoroutine(WaitForKey(4)); 
    }

    void Update() {
        if (!PhotonNetwork.IsConnected && GameManager.instance.player != null && helper != null && helper.Available) {
            string text = helper.Read();
            if (text[0] == 'A') 
                curState = DebugState.Health;
            else if (text[0] == 'B')
                curState = DebugState.Stamina;
            else {
                float newAmount;
                switch (curState) {
                    case DebugState.Health:
                        newAmount = (text[0] == '*') ? GameManager.instance.playerScript.hpBase : float.Parse(text);
                        GameManager.instance.playerScript.hp = newAmount;
                        break;
                    case DebugState.Stamina:
                        newAmount = (text[0] == '*') ? GameManager.instance.playerScript.staminaBase : float.Parse(text);
                        GameManager.instance.playerScript.stamina = newAmount;
                        break;
                }
                GameManager.instance.playerScript.UpdatePlayerUI();
            }
        }
    }

    void OnConnected(BluetoothHelper helper) { 
        helper.StartListening();
        bluetoothMessage.text = "Connected to " + deviceName;
        helper.SendData("Init");
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(StartHeartbeat());
        }
    }

    IEnumerator StartHeartbeat() { 
        while (true) { 
            helper.SendData("Alive"); 
            yield return new WaitForSeconds(1);

            //Temp until merge is done
            if (GameManager.instance.player != null)
                UpdateBarGraphHealth(GameManager.instance.playerScript.hp);
        } 
    }

    IEnumerator WaitForKey(int seconds) {
        float curTime = 0f;
        while (curTime < seconds) {
            if (Input.GetKey(key.ToString())) {
                AttemptConnection();
                yield break;
            }
            curTime += Time.deltaTime;
            yield return null;
        }
    }

    void AttemptConnection() {
        try {
            helper = BluetoothHelper.GetInstance(deviceName);
            helper.OnConnected += OnConnected;
            helper.OnConnectionFailed += OnConnectionFailed;

            helper.setTerminatorBasedStream("\n");

            helper.Connect();
        }
        catch (Exception ex) { bluetoothMessage.color = Color.red; bluetoothMessage.text = "Failed: " + ex.Message; }
    }

    void OnConnectionFailed(BluetoothHelper helper) { bluetoothMessage.color = Color.red; bluetoothMessage.text = "Connection to " + deviceName + " failed."; }

    void OnDestroy() { helper?.Disconnect(); }

    public void UpdateBarGraphHealth(float newHp) { helper.SendData("UpdateBar:" + newHp.ToString()); }
}

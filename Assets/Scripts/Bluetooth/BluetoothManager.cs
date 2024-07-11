using UnityEngine;
using ArduinoBluetoothAPI;
using System;

public class BluetoothManager : MonoBehaviour {
    public static BluetoothManager instance;
    private BluetoothHelper helper;
    readonly private string deviceName = "HC-06";

    void Awake() {
        try {
            helper = BluetoothHelper.GetInstance(deviceName);
            helper.OnConnected += OnConnected;
            helper.OnConnectionFailed += OnConnectionFailed;

            helper.setTerminatorBasedStream("\n");

            helper.Connect();
        } catch (Exception ex) { Debug.LogError("Bluetooth connection error: " + ex.Message); }
    }

    void Update() {
        if (helper != null && helper.Available)
            Debug.Log("Received message: " + helper.Read());
    }

    void OnConnected(BluetoothHelper helper) { 
        helper.StartListening();
        helper.SendData("Init");
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnConnectionFailed(BluetoothHelper helper) { Debug.LogError("Connection to " + deviceName + " failed."); }

    void OnDestroy() { helper?.Disconnect(); }

    public void UpdateBarGraphHealth(float newHp) { helper.SendData("UpdateBar:" + newHp.ToString()); }
}

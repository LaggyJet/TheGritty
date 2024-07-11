using UnityEngine;
using ArduinoBluetoothAPI;
using System;

public class BluetoothManager : MonoBehaviour {
    private BluetoothHelper helper;
    private string deviceName = "HC-06";

    void Start() {
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

        if (Input.GetKeyDown(KeyCode.O))
            SendToArduino("Hello Arduino!");
    }

    void SendToArduino(string message) {
        if (helper != null && helper.isConnected()) {
            helper.SendData(message);
            Debug.Log("Sent to Arduino: " + message);
        }
        else
            Debug.LogWarning("Bluetooth helper is not connected. Cannot send message.");
    }

    void OnConnected(BluetoothHelper helper) {
        Debug.Log("Connected to " + deviceName);
        SendToArduino("Testing");
        helper.StartListening();
    }

    void OnConnectionFailed(BluetoothHelper helper) { Debug.LogError("Connection to " + deviceName + " failed."); }

    void OnDestroy() { if (helper != null) helper.Disconnect(); }
}

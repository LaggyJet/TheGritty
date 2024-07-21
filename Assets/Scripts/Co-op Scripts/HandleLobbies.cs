//Worked on By : Joshua Furber
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandleLobbies : MonoBehaviourPunCallbacks {
    [SerializeField] TMP_InputField hostInput, joinInput;
    [SerializeField] GameObject loadMenu, lobbyMenu;
    [SerializeField] TMP_Text infoText;
    [SerializeField] Button hostButton, joinButton;

    List<RoomInfo> rooms = new();
    const string MAP_SCENE = "JoshScene"; //"New Map Scene";

    // Start the connection when loading screen plays
    void Start() { PhotonNetwork.AutomaticallySyncScene = true; PhotonNetwork.ConnectUsingSettings(); }

    // Have the player join the lobby once it loads
    public override void OnConnectedToMaster() { PhotonNetwork.JoinLobby(TypedLobby.Default); }

    // Display the lobby menu
    public override void OnJoinedLobby() { loadMenu.SetActive(false); lobbyMenu.SetActive(true); }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) { rooms = roomList; }

    // Create room based on user input (max of 2 players)
    public void HostRoom() {
        GameManager.instance.PlayMenuSwitchClick();
        for (int i = 0; i < rooms.Count; i++) {
            if (rooms[i].Name == hostInput.text.ToLower()) {
                RaiseWarning("Room with this name already exists");
                return;
            }
        }

        if (hostInput.text.Length == 0) {
            RaiseWarning("Please enter a room name");
            return;
        }
        

        hostButton.interactable = false;
        PhotonNetwork.CreateRoom(hostInput.text.ToLower(), new RoomOptions() { MaxPlayers = 2 }, null);
    }

    // Join room based on user input
    public void JoinRoom() {
        GameManager.instance.PlayMenuSwitchClick();
        for (int i = 0; i < rooms.Count; i++) {
            if (rooms[i].Name == joinInput.text.ToLower()) {
                if (rooms[i].PlayerCount != rooms[i].MaxPlayers)
                    PhotonNetwork.JoinRoom(joinInput.text.ToLower(), null);
                else
                    RaiseWarning("Room is full");
                return;
            }
        }
        
        RaiseWarning("Room doesn't exist");
    }

    // Once player loads, load the scene
    public override void OnJoinedRoom() { StartCoroutine(WaitOnLobby()); }

    IEnumerator WaitOnLobby() {
        infoText.color = Color.white;
        if (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
            infoText.text = "Connected: Waiting for other";
        else if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
            infoText.text = "";
        joinInput.interactable = joinButton.interactable = hostInput.interactable = false;
        while (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
            yield return null;
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(MAP_SCENE);
    }

    void RaiseWarning(string message) {
        infoText.color = Color.red;
        infoText.text = message;
    }
}
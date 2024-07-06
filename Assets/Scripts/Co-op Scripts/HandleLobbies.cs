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

    // Start the connection when loading screen plays
    void Start() { PhotonNetwork.AutomaticallySyncScene = true; PhotonNetwork.ConnectUsingSettings(); }

    // Have the player join the lobby once it loads
    public override void OnConnectedToMaster() { PhotonNetwork.JoinLobby(TypedLobby.Default); }

    // Display the lobby menu
    public override void OnJoinedLobby() { loadMenu.SetActive(false); lobbyMenu.SetActive(true); }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) { rooms = roomList; }

    // Create room based on user input (max of 2 players)
    public void HostRoom() {
        for (int i = 0; i < rooms.Count; i++) {
            if (rooms[i].Name == hostInput.text) {
                infoText.color = Color.red;
                infoText.text = "Room with this name already exists";
                return;
            }
        }
        
        hostButton.interactable = false;
        PhotonNetwork.CreateRoom(hostInput.text, new RoomOptions() { MaxPlayers = 2 }, null);
    }

    // Join room based on user input
    public void JoinRoom() { PhotonNetwork.JoinRoom(joinInput.text, null); }

    // Once player loads, load the scene
    public override void OnJoinedRoom() { StartCoroutine(WaitOnLobby()); }

    IEnumerator WaitOnLobby() {
        infoText.color = Color.white;
        infoText.text = "Connected: Waiting for other";
        joinInput.interactable = joinButton.interactable = false;
        while (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)
            yield return null;
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Build Scene");
    }
}
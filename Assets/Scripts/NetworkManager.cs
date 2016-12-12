using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

    [SerializeField]
    Text connectionText;
    [SerializeField]
    Transform[] spawnPoints;
    [SerializeField]
    Camera sceneCamera;

    GameObject player;

    // Use this for initialization
    void Start() {
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.ConnectUsingSettings("0.1");
    }

    // Update is called once per frame
    void Update() {
        connectionText.text = PhotonNetwork.connectionStateDetailed.ToString();
    }

    void OnJoinedLobby() {
        RoomOptions ro = new RoomOptions() {
            IsVisible = true,
            MaxPlayers = 20
        };
        PhotonNetwork.JoinOrCreateRoom("Mashiro", ro, TypedLobby.Default);
    }

    void OnJoinedRoom() {
        StartSpawnProcess(0f);
    }

    void StartSpawnProcess(float respawnTime) {
        sceneCamera.enabled = true;
        StartCoroutine("SpawnPlayer", respawnTime);
    }

    IEnumerator SpawnPlayer(float respwanTime) {
        yield return new WaitForSeconds(respwanTime);
        int index = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate("Player",
            spawnPoints[index].position,
            spawnPoints[index].rotation,
            0);
        player.GetComponent<PlayerNetworkMover>().RespawnMe += StartSpawnProcess;

        sceneCamera.enabled = false;
    }
}
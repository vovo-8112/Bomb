using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button _createRoom;

    [SerializeField]
    private Button _joinGame;

    [SerializeField]
    private TMP_Text m_Text;

    private void Start()
    {
        PhotonNetworkSetup();
        SubscribeButton();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conect");
        m_Text.SetText("Conect");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("JoinRoom");
        PhotonNetwork.LoadLevel("GameScene");
        m_Text.SetText("JoinRoom");

    }

    private void SubscribeButton()
    {
        _createRoom.onClick.AddListener(CreateRoom);
        _joinGame.onClick.AddListener(JoinGame);
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null);
    }

    private void JoinGame()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    private static void PhotonNetworkSetup()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1, 10);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }
}
using AnimationEvent;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button _createRoom;

    [SerializeField]
    private Button _joinGame;

    [SerializeField]
    private TMP_Text m_Text;

    [SerializeField]
    private EventSystem m_EventSystem;

    [SerializeField]
    private WaitPanelAnim m_LoadingPanel;

    [SerializeField]
    private GameObject m_LobbyPanel;

    private void Start()
    {
        m_EventSystem.enabled = false;
        PhotonNetworkSetup();
        SubscribeButton();
    }

    public override void OnConnectedToMaster()
    {
        m_LobbyPanel.SetActive(true);
        m_LoadingPanel.StartAnim();
        m_EventSystem.enabled = true;
        Log("Conect");
    }

    public override void OnJoinedRoom()
    {
        Log("JoinRoom");
        PhotonNetwork.LoadLevel("GameScene");
    }

    private void Log(string message)
    {
        Debug.Log(message);
        m_Text.text += "\n";
        m_Text.text = message;
    }

    private void SubscribeButton()
    {
        _createRoom.onClick.AddListener(CreateRoom);
        _joinGame.onClick.AddListener(JoinGame);
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null,new RoomOptions{MaxPlayers = 2,CleanupCacheOnLeave = false});
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
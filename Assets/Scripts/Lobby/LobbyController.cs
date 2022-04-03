using System;
using System.Collections.Generic;
using AnimationEvent;
using Extential;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField _tmpInputField;

    [SerializeField]
    private Button m_CreateRoom;

    [SerializeField]
    private Button m_JoinRandomRoom;

    [SerializeField]
    private EventSystem m_EventSystem;

    [SerializeField]
    private WaitPanelAnim m_LoadingPanel;

    [SerializeField]
    private GameObject m_LobbyPanel;

    [SerializeField]
    private RoomPanel m_RoomsPanelPrefab;

    [SerializeField]
    private Transform m_Content;

    private List<RoomInfo> m_Rooms;

    private void UpdateRooms()
    {
        m_Content.transform.Clear();

        foreach (var roomInfo in m_Rooms)
        {
            var prefab = Instantiate(m_RoomsPanelPrefab, m_Content);
            prefab.SetUp(roomInfo, this);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        m_Rooms = roomList;
        UpdateRooms();
    }

    public void JoinRoom(string nameRoom)
    {
        try
        {
            PhotonNetwork.JoinRoom(nameRoom);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
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

    private void Start()
    {
        m_EventSystem.enabled = false;
        m_LoadingPanel.gameObject.SetActive(true);
        m_LobbyPanel.gameObject.SetActive(false);
        PhotonNetworkSetup();
        SubscribeButton();
    }

    private void Log(string message)
    {
        Debug.Log(message);
    }

    private void SubscribeButton()
    {
        m_CreateRoom.onClick.AddListener(CreateRoom);
        m_JoinRandomRoom.onClick.AddListener(JoinGame);
    }

    private void CreateRoom()
    {
        string nameRoom = null;

        nameRoom = _tmpInputField.text.Length == 0 ? _tmpInputField.text : PhotonNetwork.NickName;

        PhotonNetwork.CreateRoom(nameRoom, new RoomOptions
        {
            MaxPlayers = 2,
            CleanupCacheOnLeave = false
        });
    }

    private void JoinGame()
    {
        try
        {
            PhotonNetwork.JoinRandomRoom();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            CreateRoom();
        }
    }

    private static void PhotonNetworkSetup()
    {
        PhotonNetwork.NickName = "Player" + Random.Range(1, 500);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "1.0.0";
        PhotonNetwork.ConnectUsingSettings();
    }
}
using System.Collections.Generic;
using AnimationEvent;
using Extential;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbyController : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private TMP_InputField _tmpInputField;

        [SerializeField]
        private Button m_Battle;

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

        private List<RoomInfo> m_Rooms = new List<RoomInfo>();

        private const string GameScene = "GameScene";

        private void Start()
        {
            m_EventSystem.enabled = false;
            m_LoadingPanel.gameObject.SetActive(true);
            m_LobbyPanel.gameObject.SetActive(false);
            PhotonNetworkSetup();
            SubscribeButton();
        }

        private void SubscribeButton()
        {
            LoadWaitPanel();
            m_CreateRoom.onClick.AddListener(CreateRoom);
            m_JoinRandomRoom.onClick.AddListener(JoinGame);
            m_Battle.onClick.AddListener(JoinGame);
        }

        private void LoadWaitPanel()
        {
            m_EventSystem.enabled = false;
            m_LoadingPanel.SetUp();
            m_LoadingPanel.gameObject.SetActive(true);
            m_LobbyPanel.gameObject.SetActive(false);
        }

        private void UpdateRooms()
        {
            m_Content.transform.Clear();

            foreach (var roomInfo in m_Rooms)
            {
                var prefab = Instantiate(m_RoomsPanelPrefab, m_Content);
                prefab.SetUp(roomInfo, this);
            }

            if (m_Rooms.Count != 0)
            {
                return;
            }

            {
                var prefab = Instantiate(m_RoomsPanelPrefab, m_Content);
                prefab.SetUpInfo(LobbyConstants.NoRoom);
            }
        }

        private void AnimComplete()
        {
            m_LoadingPanel.StartAnim();
            m_EventSystem.enabled = true;
        }

        private void CreateRoom()
        {
            LoadWaitPanel();
            string nameRoom = _tmpInputField.text.Length == 0 ? _tmpInputField.text : PhotonNetwork.NickName;

            PhotonNetwork.CreateRoom(nameRoom, new RoomOptions
            {
                IsVisible = true,
                MaxPlayers = 2,
            });
        }

        private void JoinGame()
        {
            LoadWaitPanel();
            PhotonNetwork.JoinRandomRoom();
        }

        private static void PhotonNetworkSetup()
        {
            PhotonNetwork.NickName = "Player" + Random.Range(1, 500);
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "1.0.0";
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            m_Rooms = roomList;
            UpdateRooms();
        }

        public void JoinRoom(string nameRoom)
        {
            PhotonNetwork.JoinRoom(nameRoom);
        }

        public override void OnConnectedToMaster()
        {
            m_LobbyPanel.SetActive(true);
            AnimComplete();
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel(GameScene);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            CreateRoom();
        }
    }
}
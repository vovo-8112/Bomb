using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameManager
{
    public class PhotoGameManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private Button m_LeaveRoomButton;

        [SerializeField]
        private List<Transform> m_ListSpawnPosition;

        [SerializeField]
        private List<ExplodudesCrate> m_ExplodudesCrates;

        private List<Character> m_Characters = new List<Character>();

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetUpMap();
            }

            m_LeaveRoomButton.onClick.AddListener(OnLeftRoom);

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                var player = PhotonNetwork.Instantiate("MinimalGridCharacter", m_ListSpawnPosition[0].position, Quaternion.identity);
                m_Characters.Add(player.GetComponent<Character>());
            }
            else
            {
                var player = PhotonNetwork.Instantiate("MinimalGridCharacterSecond", m_ListSpawnPosition[1].position, Quaternion.identity);
                m_Characters.Add(player.GetComponent<Character>());
            }
        }

        private void SetUpMap()
        {
            for (int i = 0; i < m_ExplodudesCrates.Count; i++)
            {
                var random = Random.Range(0, 3);

                if (random == 0)
                {
                    m_ExplodudesCrates[i].DisableObject();
                }
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("Player{0} entered room", newPlayer.NickName);
        }

        public override void OnPlayerLeftRoom(Player player)
        {
            Debug.LogFormat("Player{0} entered room", player.NickName);
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        private void AddPlayer(Character character)
        {
            m_Characters.Add(character);
        }
    }
}
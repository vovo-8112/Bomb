using System.Collections;
using ExitGames.Client.Photon;
using MoreMountains.TopDownEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GameManager
{
    public class MapController : MonoBehaviour, IOnEventCallback
    {
        [SerializeField]
        private ExplodudesCrate[] m_ExplodudesCrates;

        private void Awake()
        {
            PhotonPeer.RegisterType(typeof(MapDate), 244, MapDate.Serialize, MapDate.DeSerialize);
        }

        public void SendSyncDate(Player player)
        {
            MapDate date = new MapDate();

            RaiseEventOptions options = new RaiseEventOptions
            {
                TargetActors = new[] { player.ActorNumber }
            };

            SendOptions sendOptions = new SendOptions
            {
                Reliability = true
            };

            date.CratesDate = new BitArray(m_ExplodudesCrates.Length);

            for (int i = 0; i < m_ExplodudesCrates.Length; i++)
            {
                for (int j = 0; j < m_ExplodudesCrates.Length; j++)
                {
                    date.CratesDate.Set(i + j * m_ExplodudesCrates.Length, m_ExplodudesCrates[i].IsEnable);
                }
            }

            PhotonNetwork.RaiseEvent(43, date, options, sendOptions);
        }

        public void OnEvent(EventData eventData)
        {
            switch (eventData.Code)
            {
                case 43:
                    var date = (MapDate)eventData.CustomData;
                    OnSendDateRecursive(date);
                    break;
            }
        }

        private async void OnSendDateRecursive(MapDate eventData)
        {
            
            for (int i = 0; i < m_ExplodudesCrates.Length; i++)
            {
                for (int j = 0; j < m_ExplodudesCrates.Length; j++)
                {
                    var active = eventData.CratesDate.Get(i + j * m_ExplodudesCrates.Length);
                    m_ExplodudesCrates[i].SetActive(active);
                }
            }
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
                SetUpMap();
        }

        private void SetUpMap()
        {
            for (int i = 0; i < m_ExplodudesCrates.Length; i++)
            {
                var random = Random.Range(0, 3);

                if (random == 0)
                {
                    m_ExplodudesCrates[i].SetActive(false);
                }
            }
        }
    }
}
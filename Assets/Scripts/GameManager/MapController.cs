using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
// using MoreMountains.TopDownEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GameManager
{
    public class MapController : MonoBehaviour
    {
        // [SerializeField]
        // private List<ExplodudesCrate> m_ExplodudesCrates;
        //
        // public void SendSyncDate(Player player)
        // {
        //     MapDate date = new MapDate();
        //
        //     RaiseEventOptions options = new RaiseEventOptions
        //     {
        //         TargetActors = new[] { player.ActorNumber }
        //     };
        //
        //     SendOptions sendOptions = new SendOptions
        //     {
        //         Reliability = true
        //     };
        //
        //     date.CratesDate = new BitArray(m_ExplodudesCrates.Count);
        //
        //     for (int i = 0; i < m_ExplodudesCrates.Count; i++)
        //     {
        //         date.CratesDate.Set(i, m_ExplodudesCrates[i].IsEnable);
        //     }
        //
        //     PhotonNetwork.RaiseEvent(43, date, options, sendOptions);
        // }
        //
        // public void OnEvent(EventData eventData)
        // {
        //     switch (eventData.Code)
        //     {
        //         case 43:
        //             var date = (MapDate)eventData.CustomData;
        //             OnSendDateRecursive(date);
        //             break;
        //     }
        // }
        //
        // private async void OnSendDateRecursive(MapDate eventData)
        // {
        //     for (int i = 0; i < m_ExplodudesCrates.Count; i++)
        //     {
        //         var active = eventData.CratesDate.Get(i);
        //         m_ExplodudesCrates[i].SetActive(active);
        //     }
        // }
        //
        // private void Start()
        // {
        //     if (PhotonNetwork.IsMasterClient)
        //         SetUpMap();
        // }

        // private void SetUpMap()
        // {
        //     for (int i = 0; i < m_ExplodudesCrates.Count; i++)
        //     {
        //         var random = Random.Range(0, 3);
        //
        //         if (random == 0)
        //         {
        //             m_ExplodudesCrates[i].SetActive(false);
        //         }
        //     }
        // }
    }
}
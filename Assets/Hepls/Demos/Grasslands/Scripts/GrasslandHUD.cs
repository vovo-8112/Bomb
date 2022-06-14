using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    public class GrasslandHUD : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        [Tooltip("The playerID associated to this HUD")]
        public string PlayerID = "Player1";
        [Tooltip("the progress bar to use to show the healthbar")]
        public MMProgressBar HealthBar;
        [Tooltip("the Text comp to use to display the player name")]
        public Text PlayerName;
        [Tooltip("the radial progress bar to put around the avatar")]
        public MMRadialProgressBar AvatarBar;
        [Tooltip("the counter used to display coin amounts")]
        public Text CoinCounter;
        [Tooltip("the mask to use when the target player dies")]
        public CanvasGroup DeadMask;
        [Tooltip("the screen to display if the target player wins")]
        public CanvasGroup WinnerScreen;

        protected virtual void Start()
        {
            CoinCounter.text = "0";
            DeadMask.gameObject.SetActive(false);
            WinnerScreen.gameObject.SetActive(false);
        }

        public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
        {
            switch (tdEvent.EventType)
            {
                case TopDownEngineEventTypes.PlayerDeath:
                    if (tdEvent.OriginCharacter.PlayerID == PlayerID)
                    {
                        DeadMask.gameObject.SetActive(true);
                        DeadMask.alpha = 0f;
                        StartCoroutine(MMFade.FadeCanvasGroup(DeadMask, 0.5f, 0.8f, true));
                    }
                    break;
                case TopDownEngineEventTypes.Repaint:
                    foreach (GrasslandsMultiplayerLevelManager.GrasslandPoints points in (LevelManager.Instance as GrasslandsMultiplayerLevelManager).Points)
                    {
                        if (points.PlayerID == PlayerID)
                        {
                            CoinCounter.text = points.Points.ToString();
                        }
                    }
                    break;
                case TopDownEngineEventTypes.GameOver:
                    if (PlayerID == (LevelManager.Instance as GrasslandsMultiplayerLevelManager).WinnerID)
                    {
                        WinnerScreen.gameObject.SetActive(true);
                        WinnerScreen.alpha = 0f;
                        StartCoroutine(MMFade.FadeCanvasGroup(WinnerScreen, 0.5f, 0.8f, true));
                    }
                    break;
            }
            
        }
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}

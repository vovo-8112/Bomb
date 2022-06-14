using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesWinnerScreen : MonoBehaviour, MMEventListener<TopDownEngineEvent>
    {
        [Tooltip("the ID of the player we want this screen to appear for")]
        public string PlayerID = "Player1";
        [Tooltip("the canvas group containing the winner screen")]
        public CanvasGroup WinnerScreen;
        protected virtual void Start()
        {
            WinnerScreen.gameObject.SetActive(false);
        }
        public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
        {
            switch (tdEvent.EventType)
            {
                case TopDownEngineEventTypes.GameOver:
                    if (PlayerID == (LevelManager.Instance as ExplodudesMultiplayerLevelManager).WinnerID)
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
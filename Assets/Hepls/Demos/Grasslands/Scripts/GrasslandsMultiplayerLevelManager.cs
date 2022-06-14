using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    public class GrasslandsMultiplayerLevelManager : MultiplayerLevelManager, MMEventListener<PickableItemEvent>
    {
        public struct GrasslandPoints
        {
            public string PlayerID;
            public int Points;
        }

        [Header("Grasslands Bindings")]
        [Tooltip("an array to store each player's points")]
        public GrasslandPoints[] Points;
        [Tooltip("the list of countdowns we need to update")]
        public List<MMCountdown> Countdowns;

        [Header("Grasslands Settings")]
        [Tooltip("the duration of the game, in seconds")]
        public int GameDuration = 99;
        public string WinnerID { get; set; }

        protected string _playerID;
        protected bool _gameOver = false;
        protected override void Initialization()
        {
            base.Initialization();
            WinnerID = "";
            Points = new GrasslandPoints[Players.Count];
            int i = 0;
            foreach(Character player in Players)
            {
                Points[i].PlayerID = player.PlayerID;
                Points[i].Points = 0;
                i++;
            }
            foreach(MMCountdown countdown in Countdowns)
            {
                countdown.CountdownFrom = GameDuration;
                countdown.ResetCountdown();
            }
        }
        protected override void OnPlayerDeath(Character playerCharacter)
        {
            base.OnPlayerDeath(playerCharacter);
            int aliveCharacters = 0;
            int i = 0;
            
            foreach(Character character in LevelManager.Instance.Players)
            {
                if (character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
                {
                    WinnerID = character.PlayerID;
                    aliveCharacters++;
                }
                i++;
            }

            if (aliveCharacters <= 1)
            {
                StartCoroutine(GameOver());
            }
        }
        protected virtual IEnumerator GameOver()
        {
            yield return new WaitForSeconds(2f);
            if (WinnerID == "")
            {
                WinnerID = "Player1";
            }
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
            _gameOver = true;
            MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
        }
        public virtual void Update()
        {
            UpdateCountdown();
            CheckForGameOver();
        }
        protected virtual void UpdateCountdown()
        {
            if (_gameOver)
            {
                return;
            }

            float remainingTime = GameDuration;
            foreach (MMCountdown countdown in Countdowns)
            {
                if (countdown.gameObject.activeInHierarchy)
                {
                    remainingTime = countdown.CurrentTime;
                }
            }
            if (remainingTime <= 0f)
            {
                int maxPoints = 0;
                foreach (GrasslandPoints points in Points)
                {
                    if (points.Points > maxPoints)
                    {
                        WinnerID = points.PlayerID;
                        maxPoints = points.Points;
                    }
                }
                StartCoroutine(GameOver());
            }
        }
        protected virtual void CheckForGameOver()
        {
            if (_gameOver)
            {
                if ( (Input.GetButton("Player1_Jump"))
                    || (Input.GetButton("Player2_Jump"))
                    || (Input.GetButton("Player3_Jump"))
                    || (Input.GetButton("Player4_Jump")) )
                {
                    MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
                    MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
        public virtual void OnMMEvent(PickableItemEvent pickEvent)
        {
            _playerID = pickEvent.Picker.MMGetComponentNoAlloc<Character>()?.PlayerID;
            for (int i = 0; i < Points.Length; i++)
            {
                if (Points[i].PlayerID == _playerID)
                {
                    Points[i].Points++;
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Repaint, null);
                }
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening<PickableItemEvent>();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening<PickableItemEvent>();
        }
    }
}
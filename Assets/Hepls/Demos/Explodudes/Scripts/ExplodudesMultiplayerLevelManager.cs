using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    public class ExplodudesMultiplayerLevelManager : MultiplayerLevelManager
    {
        [Header("Explodudes Settings")]
        [Tooltip("the duration of the game, in seconds")]
        public int GameDuration = 99;
        public string WinnerID { get; set; }

        protected string _playerID;
        protected bool _gameOver = false;
        protected override void Initialization()
        {
            base.Initialization();
            WinnerID = "";
        }
        protected override void OnPlayerDeath(Character playerCharacter)
        {
            base.OnPlayerDeath(playerCharacter);
            int aliveCharacters = 0;
            int i = 0;

            foreach (Character character in LevelManager.Instance.Players)
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
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
        }
        public virtual void Update()
        {
            CheckForGameOver();
        }
        protected virtual void CheckForGameOver()
        {
            if (_gameOver)
            {
                if ((Input.GetButton("Player1_Jump"))
                    || (Input.GetButton("Player2_Jump"))
                    || (Input.GetButton("Player3_Jump"))
                    || (Input.GetButton("Player4_Jump")))
                {
                    MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
                    MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
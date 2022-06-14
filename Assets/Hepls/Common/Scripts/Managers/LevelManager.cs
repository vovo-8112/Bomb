using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/Level Manager")]
    public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
    {
        [Header("Instantiate Characters")]
        [MMInformation(
            "The LevelManager is responsible for handling spawn/respawn, checkpoints management and level bounds. Here you can define one or more playable characters for your level..",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("The list of player prefabs this level manager will instantiate on Start")]
        public Character[] PlayerPrefabs;
        [Tooltip("should the player IDs be auto attributed (usually yes)")]
        public bool AutoAttributePlayerIDs = true;

        [Header("Characters already in the scene")]
        [MMInformation(
            "It's recommended to have the LevelManager instantiate your characters, but if instead you'd prefer to have them already present in the scene, just bind them in the list below.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored")]
        public List<Character> SceneCharacters;

        [Header("Checkpoints")]
        [Tooltip("the checkpoint to use as initial spawn point if no point of entry is specified")]
        public CheckPoint InitialSpawnPoint;

        public CheckPoint SecondInitialSpawnPoint;
        [Tooltip("the currently active checkpoint (the last checkpoint passed by the player)")]
        public CheckPoint CurrentCheckpoint;

        [Header("Points of Entry")]
        [Tooltip("A list of this level's points of entry, which can be used from other levels as initial targets")]
        public Transform[] PointsOfEntry;

        [Space(10)]
        [Header("Intro and Outro durations")]
        [MMInformation(
            "Here you can specify the length of the fade in and fade out at the start and end of your level. You can also determine the delay before a respawn.",
            MMInformationAttribute.InformationType.Info, false)]
        [Tooltip("the duration of the initial fade in (in seconds)")]
        public float IntroFadeDuration = 1f;
        [Tooltip("the duration of the fade to black at the end of the level (in seconds)")]
        public float OutroFadeDuration = 1f;
        [Tooltip("the ID to use when triggering the event (should match the ID on the fader you want to use)")]
        public int FaderID = 0;
        [Tooltip("the curve to use for in and out fades")]
        public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        [Tooltip("the duration between a death of the main character and its respawn")]
        public float RespawnDelay = 2f;

        [Header("Respawn Loop")]
        [Tooltip("the delay, in seconds, before displaying the death screen once the player is dead")]
        public float DelayBeforeDeathScreen = 1f;

        [Header("Bounds")]
        [Tooltip("if this is true, this level will use the level bounds defined on this LevelManager. Set it to false when using the Rooms system.")]
        public bool UseLevelBounds = true;

        [Header("Scene Loading")]
        [Tooltip("the method to use to load the destination level")]
        public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
        [Tooltip("the name of the MMSceneLoadingManager scene you want to use")]
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
        public string LoadingSceneName = "LoadingScreen";
        [Tooltip("the settings to use when loading the scene in additive mode")]
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
        public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings;
        public Bounds LevelBounds
        {
            get { return (_collider == null) ? new Bounds() : _collider.bounds; }
        }

        public Collider BoundsCollider { get; protected set; }
        public TimeSpan RunningTime
        {
            get { return DateTime.UtcNow - _started; }
        }
        public List<CheckPoint> Checkpoints { get; protected set; }
        public List<Character> Players { get; protected set; }

        protected DateTime _started;
        protected int _savedPoints;
        protected Collider _collider;
        protected override void Awake()
        {
            base.Awake();
            _collider = this.GetComponent<Collider>();
        }
        protected virtual void Start()
        {
            BoundsCollider = _collider;
            InstantiatePlayableCharacters();

            if (UseLevelBounds)
            {
                MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider);
            }

            if (Players == null || Players.Count == 0)
            {
                return;
            }

            Initialization();

            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);
            if (Players.Count == 1)
            {
                SpawnSingleCharacter();
            }
            else
            {
                SpawnMultipleCharacters();
            }

            CheckpointAssignment();
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
            MMGameEvent.Trigger("Load");

            MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
            MMGameEvent.Trigger("CameraBound");
        }
        protected virtual void SpawnMultipleCharacters()
        {
        }
        protected virtual void InstantiatePlayableCharacters()
        {
            Players = new List<Character>();
            if (PlayerPrefabs == null)
            {
                return;
            }
        }
        protected virtual void CheckpointAssignment()
        {
            IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>().OfType<Respawnable>();
            AutoRespawn autoRespawn;

            foreach (Respawnable listener in listeners)
            {
                for (int i = Checkpoints.Count - 1; i >= 0; i--)
                {
                    autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();

                    if (autoRespawn == null)
                    {
                        Checkpoints[i].AssignObjectToCheckPoint(listener);
                        continue;
                    }
                    else
                    {
                        if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
                        {
                            Checkpoints[i].AssignObjectToCheckPoint(listener);
                            continue;
                        }
                        else
                        {
                            if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
                            {
                                Checkpoints[i].AssignObjectToCheckPoint(listener);
                                continue;
                            }

                            continue;
                        }
                    }
                }
            }
        }
        protected virtual void Initialization()
        {
            Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
            _savedPoints = GameManager.Instance.Points;
            _started = DateTime.UtcNow;
        }
        protected virtual void SpawnSingleCharacter()
        {
            PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);

            if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
            {
                Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], point.FacingDirection);
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
                return;
            }

            if (InitialSpawnPoint != null)
            {
                InitialSpawnPoint.SpawnPlayer(Players[0]);
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
                return;
            }
        }
        public virtual void GotoLevel(string levelName)
        {
            TriggerEndLevelEvents();
            StartCoroutine(GotoLevelCo(levelName));
        }
        public virtual void TriggerEndLevelEvents()
        {
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
            MMGameEvent.Trigger("Save");
        }
        protected virtual IEnumerator GotoLevelCo(string levelName)
        {
            if (Players != null && Players.Count > 0)
            {
                foreach (Character player in Players)
                {
                    player.Disable();
                }
            }

            MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);

            if (Time.timeScale > 0.0f)
            {
                yield return new WaitForSeconds(OutroFadeDuration);
            }
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

            string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

            switch (LoadingSceneMode)
            {
                case MMLoadScene.LoadingSceneModes.UnityNative:
                    SceneManager.LoadScene(destinationScene);
                    break;
                case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
                    MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
                    break;
                case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
                    MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
                    break;
            }
        }
        public virtual void PlayerDead(Character playerCharacter)
        {
            StartCoroutine(PlayerDeadCo());
        }

        public virtual void PlayerWin(Character playerCharacter)
        {
            StartCoroutine(PlayerWinCo());
        }
        protected virtual IEnumerator PlayerDeadCo()
        {
            yield return new WaitForSeconds(DelayBeforeDeathScreen);

            GUIManager.Instance.SetDeathScreen(true);
        }

        protected virtual IEnumerator PlayerWinCo()
        {
            yield return new WaitForSeconds(DelayBeforeDeathScreen);

            GUIManager.Instance.SetWinScreen(true);
        }
        protected virtual void Respawn()
        {
            if (Players.Count < 2)
            {
                StartCoroutine(SoloModeRestart());
            }
        }
        protected virtual IEnumerator SoloModeRestart()
        {
            if ((PlayerPrefabs.Count() <= 0) && (SceneCharacters.Count <= 0))
            {
                yield break;
            }
            if (GameManager.Instance.MaximumLives > 0)
            {
                GameManager.Instance.LoseLife();
                if (GameManager.Instance.CurrentLives <= 0)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);

                    if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
                    {
                        MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
                    }
                }
            }

            MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

            MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
            yield return new WaitForSeconds(OutroFadeDuration);

            yield return new WaitForSeconds(RespawnDelay);

            GUIManager.Instance.SetPauseScreen(false);
            GUIManager.Instance.SetDeathScreen(false);
            MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

            MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            if (CurrentCheckpoint == null)
            {
                CurrentCheckpoint = InitialSpawnPoint;
            }

            if (Players[0] == null)
            {
                InstantiatePlayableCharacters();
            }

            if (CurrentCheckpoint != null)
            {
                CurrentCheckpoint.SpawnPlayer(Players[0]);
            }
            else
            {
                Debug.LogWarning("LevelManager : no checkpoint or initial spawn point has been defined, can't respawn the Player.");
            }

            _started = DateTime.UtcNow;
            TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
        }
        public virtual void ToggleCharacterPause()
        {
            foreach (Character player in Players)
            {
                CharacterPause characterPause = player.FindAbility<CharacterPause>();

                if (characterPause == null)
                {
                    break;
                }

                if (GameManager.Instance.Paused)
                {
                    characterPause.PauseCharacter();
                }
                else
                {
                    characterPause.UnPauseCharacter();
                }
            }
        }
        public virtual void FreezeCharacters()
        {
            foreach (Character player in Players)
            {
                player.Freeze();
            }
        }
        public virtual void UnFreezeCharacters()
        {
            foreach (Character player in Players)
            {
                player.UnFreeze();
            }
        }
        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
        {
            if (newCheckPoint.ForceAssignation)
            {
                CurrentCheckpoint = newCheckPoint;
                return;
            }

            if (CurrentCheckpoint == null)
            {
                CurrentCheckpoint = newCheckPoint;
                return;
            }

            if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
            {
                CurrentCheckpoint = newCheckPoint;
            }
        }
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            switch (engineEvent.EventType)
            {
                case TopDownEngineEventTypes.PlayerDeath:
                    PlayerDead(engineEvent.OriginCharacter);
                    break;
                case TopDownEngineEventTypes.RespawnStarted:
                    Respawn();
                    break;
                case TopDownEngineEventTypes.LevelComplete:
                    PlayerWin(engineEvent.OriginCharacter);
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
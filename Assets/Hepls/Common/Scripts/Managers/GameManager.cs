using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    public enum TopDownEngineEventTypes
	{
		SpawnCharacterStarts,
		LevelStart,
		LevelComplete,
		LevelEnd,
		Pause,
		UnPause,
		PlayerDeath,
        SpawnComplete,
        RespawnStarted,
        RespawnComplete,
        StarPicked,
		GameOver,
        CharacterSwap,
        CharacterSwitch,
        Repaint,
        TogglePause,
        LoadNextScene
    }
	public struct TopDownEngineEvent
	{
		public TopDownEngineEventTypes EventType;
        public Character OriginCharacter;
		public TopDownEngineEvent(TopDownEngineEventTypes eventType, Character originCharacter)
		{
			EventType = eventType;
            OriginCharacter = originCharacter;
		}

        static TopDownEngineEvent e;
        public static void Trigger(TopDownEngineEventTypes eventType, Character originCharacter)
        {
            e.EventType = eventType;
            e.OriginCharacter = originCharacter;
            MMEventManager.TriggerEvent(e);
        }
    }
	public enum PointsMethods
	{
		Add,
		Set
	}
	public struct TopDownEnginePointEvent
	{
		public PointsMethods PointsMethod;
		public int Points;
        public TopDownEnginePointEvent(PointsMethods pointsMethod, int points)
		{
			PointsMethod = pointsMethod;
			Points = points;
        }

        static TopDownEnginePointEvent e;
        public static void Trigger(PointsMethods pointsMethod, int points)
        {
            e.PointsMethod = pointsMethod;
            e.Points = points;
            MMEventManager.TriggerEvent(e);
        }
    }
	public enum PauseMethods
	{
		PauseMenu,
		NoPauseMenu
	}
	public class PointsOfEntryStorage
	{
		public string LevelName;
		public int PointOfEntryIndex;
		public Character.FacingDirections FacingDirection;

		public PointsOfEntryStorage(string levelName, int pointOfEntryIndex, Character.FacingDirections facingDirection)
		{
			LevelName = levelName;
			FacingDirection = facingDirection;
			PointOfEntryIndex = pointOfEntryIndex;
		}
	}
	[AddComponentMenu("TopDown Engine/Managers/Game Manager")]
	public class GameManager : 	MMPersistentSingleton<GameManager>, 
								MMEventListener<MMGameEvent>, 
								MMEventListener<TopDownEngineEvent>, 
								MMEventListener<TopDownEnginePointEvent>
	{
		[Tooltip("the target frame rate for the game")]
		public int TargetFrameRate = 300;
        [Header("Lives")]
		[Tooltip("the maximum amount of lives the character can currently have")]
		public int MaximumLives = 0;
		[Tooltip("the current number of lives ")]
		public int CurrentLives = 0;

        [Header("Bindings")]
		[Tooltip("the name of the scene to redirect to when all lives are lost")]
		public string GameOverScene;

        [Header("Points")]
        [MMReadOnly]
		[Tooltip("the current number of game points")]
		public int Points;

        [Header("Pause")]
		[Tooltip("if this is true, the game will automatically pause when opening an inventory")]
		public bool PauseGameWhenInventoryOpens = true;
        public bool Paused { get; set; }
		public bool StoredLevelMapPosition{ get; set; }
		public Vector2 LevelMapPosition { get; set; }
		public Character PersistentCharacter { get; set; }
		[Tooltip("the list of points of entry and exit")]
		public List<PointsOfEntryStorage> PointsOfEntry;
        public Character StoredCharacter { get; set; }
		protected bool _inventoryOpen = false;
		protected bool _pauseMenuOpen = false;
		protected InventoryInputManager _inventoryInputManager;
        protected int _initialMaximumLives;
        protected int _initialCurrentLives;
        protected override void Awake()
		{
			base.Awake ();
			PointsOfEntry = new List<PointsOfEntryStorage> ();
		}
	    protected virtual void Start()
	    {
			Application.targetFrameRate = TargetFrameRate;
            _initialCurrentLives = CurrentLives;
            _initialMaximumLives = MaximumLives;
        }
		public virtual void Reset()
		{
			Points = 0;
			Time.timeScale = 1f;
			Paused = false;
		}
        public virtual void LoseLife()
        {
            CurrentLives--;
        }
        public virtual void GainLives(int lives)
        {
            CurrentLives += lives;
            if (CurrentLives > MaximumLives)
            {
                CurrentLives = MaximumLives;
            }
        }
        public virtual void AddLives(int lives, bool increaseCurrent)
        {
            MaximumLives += lives;
            if (increaseCurrent)
            {
                CurrentLives += lives;
            }
        }
        public virtual void ResetLives()
        {
            CurrentLives = _initialCurrentLives;
            MaximumLives = _initialMaximumLives;
        }
        public virtual void AddPoints(int pointsToAdd)
		{
			Points += pointsToAdd;
            GUIManager.Instance.RefreshPoints();
        }
		public virtual void SetPoints(int points)
		{
			Points = points;
            GUIManager.Instance.RefreshPoints();
        }
		protected virtual void SetActiveInventoryInputManager(bool status)
		{
			_inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager> ();
			if (_inventoryInputManager != null)
			{
				_inventoryInputManager.enabled = status;
			}
		}
		public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
		{	
			if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen)
			{
				return;
			}
			if (Time.timeScale>0.0f)
            {
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
                Instance.Paused=true;
				if ((GUIManager.Instance!= null) && (pauseMethod == PauseMethods.PauseMenu))
				{
					GUIManager.Instance.SetPauseScreen(true);	
					_pauseMenuOpen = true;
					SetActiveInventoryInputManager (false);
				}
				if (pauseMethod == PauseMethods.NoPauseMenu)
				{
					_inventoryOpen = true;
				}
			}
			else
			{
				UnPause(pauseMethod);
			}		
			LevelManager.Instance.ToggleCharacterPause();
		}
        public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            Instance.Paused = false;
			if ((GUIManager.Instance!= null) && (pauseMethod == PauseMethods.PauseMenu))
			{ 
				GUIManager.Instance.SetPauseScreen(false);
				_pauseMenuOpen = false;
				SetActiveInventoryInputManager (true);
			}
			if (_inventoryOpen)
			{
				_inventoryOpen = false;
            }
            LevelManager.Instance.ToggleCharacterPause();
        }
        public virtual void StorePointsOfEntry(string levelName, int entryIndex, Character.FacingDirections facingDirection)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						point.PointOfEntryIndex = entryIndex;
						return;
					}
				}	
			}

			PointsOfEntry.Add (new PointsOfEntryStorage (levelName, entryIndex, facingDirection));
		}
		public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						return point;
					}
				}
			}
			return null;
		}
		public virtual void ClearPointOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						PointsOfEntry.Remove (point);
					}
				}
			}
		}
		public virtual void ClearAllPointsOfEntry()
		{
			PointsOfEntry.Clear ();
		}
		public virtual void ResetAllSaves()
        {
            MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
            MMSaveLoadManager.DeleteSaveFolder("TopDownEngine");
            MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
        }
        public virtual void StoreSelectedCharacter(Character selectedCharacter)
        {
            StoredCharacter = selectedCharacter;
        }
        public virtual void ClearSelectedCharacter()
        {
            StoredCharacter = null;
        }
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "inventoryOpens":
                    if (PauseGameWhenInventoryOpens)
                    {
                        Pause(PauseMethods.NoPauseMenu);
                    }					
					break;

				case "inventoryCloses":
                    if (PauseGameWhenInventoryOpens)
                    {
                        Pause(PauseMethods.NoPauseMenu);
                    }
					break;
			}
		}
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
                case TopDownEngineEventTypes.TogglePause:
                    if (Paused)
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
                    }
                    else
                    {
                        TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
                    }
                    break;
				case TopDownEngineEventTypes.Pause:
					Pause ();
					break;

				case TopDownEngineEventTypes.UnPause:
					UnPause ();
					break;
			}
		}
        public virtual void OnMMEvent(TopDownEnginePointEvent pointEvent)
		{
            switch (pointEvent.PointsMethod)
            {
                case PointsMethods.Set:
                    SetPoints(pointEvent.Points);
                    break;

                case PointsMethods.Add:
                    AddPoints(pointEvent.Points);
                    break;
            }
        }
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<TopDownEngineEvent> ();
			this.MMEventStartListening<TopDownEnginePointEvent> ();
		}
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<TopDownEngineEvent> ();
			this.MMEventStopListening<TopDownEnginePointEvent> ();
		}
	}
}
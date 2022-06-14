using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/GUIManager")]
    public class GUIManager : MMSingleton<GUIManager> 
	{
		[Tooltip("the main canvas")]
		public Canvas MainCanvas;
		[Tooltip("the game object that contains the heads up display (avatar, health, points...)")]
		public GameObject HUD;
		[Tooltip("the health bars to update")]
		public MMProgressBar[] HealthBars;
		[Tooltip("the dash bars to update")]
		public MMRadialProgressBar[] DashBars;
		[Tooltip("the panels and bars used to display current weapon ammo")]
		public AmmoDisplay[] AmmoDisplays;
		[Tooltip("the pause screen game object")]
		public GameObject PauseScreen;
		[Tooltip("the death screen")]
		public GameObject DeathScreen;
		[Tooltip("the win screen")]
		public GameObject WinScreen;
		[Tooltip("The mobile buttons")]
		public CanvasGroup Buttons;
		[Tooltip("The mobile arrows")]
		public CanvasGroup Arrows;
		[Tooltip("The mobile movement joystick")]
		public CanvasGroup Joystick;
		[Tooltip("the points counter")]
		public Text PointsText;
        [Tooltip("the pattern to apply to format the display of points")]
        public string PointsTextPattern = "000000";


        protected float _initialJoystickAlpha;
		protected float _initialButtonsAlpha;
        protected bool _initialized = false;
		protected override void Awake()
		{
			base.Awake();

            Initialization();
		}

        protected virtual void Initialization()
        {
            if (_initialized)
            {
                return;
            }

            if (Joystick != null)
            {
                _initialJoystickAlpha = Joystick.alpha;
            }
            if (Buttons != null)
            {
                _initialButtonsAlpha = Buttons.alpha;
            }

            _initialized = true;
        }
	    protected virtual void Start()
		{
			RefreshPoints();
            SetPauseScreen(false);
            SetDeathScreen(false);
		}
	    public virtual void SetHUDActive(bool state)
	    {
	        if (HUD!= null)
	        { 
	            HUD.SetActive(state);
	        }
	        if (PointsText!= null)
	        { 
	            PointsText.enabled = state;
	        }
	    }
	    public virtual void SetAvatarActive(bool state)
	    {
	        if (HUD != null)
	        {
	            HUD.SetActive(state);
	        }
	    }
		public virtual void SetMobileControlsActive(bool state, InputManager.MovementControls movementControl = InputManager.MovementControls.Joystick)
        {
            Initialization();
            
            if (Joystick != null)
			{
				Joystick.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Joystick)
				{
					Joystick.alpha=_initialJoystickAlpha;
				}
				else
				{
					Joystick.alpha=0;
					Joystick.gameObject.SetActive (false);
				}
			}

			if (Arrows != null)
			{
				Arrows.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Arrows)
				{
					Arrows.alpha=_initialJoystickAlpha;
				}
				else
				{
					Arrows.alpha=0;
					Arrows.gameObject.SetActive (false);
				}
			}

			if (Buttons != null)
			{
				Buttons.gameObject.SetActive(state);
				if (state)
				{
					Buttons.alpha=_initialButtonsAlpha;
				}
				else
				{
					Buttons.alpha=0;
					Buttons.gameObject.SetActive (false);
				}
			}
		}
        public virtual void SetPauseScreen(bool state)
        {
            if (PauseScreen != null)
            {
                PauseScreen.SetActive(state);
                EventSystem.current.sendNavigationEvents = state;
            }
        }
        public virtual void SetDeathScreen(bool state)
        {
            if (DeathScreen != null)
            {
                DeathScreen.SetActive(state);
                EventSystem.current.sendNavigationEvents = state;
            }
        }
        
        public virtual void SetWinScreen(bool state)
        {
	        if (WinScreen != null)
	        {
		        WinScreen.SetActive(state);
		        EventSystem.current.sendNavigationEvents = state;
	        }
        }
        public virtual void SetDashBar(bool state, string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar jetpackBar in DashBars)
			{
				if (jetpackBar != null)
		        { 
		        	if (jetpackBar.PlayerID == playerID)
		        	{
						jetpackBar.gameObject.SetActive(state);
		        	}					
		        }
			}	        
	    }
		public virtual void SetAmmoDisplays(bool state, string playerID, int ammoDisplayID)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay != null)
				{ 
					if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
					{
						ammoDisplay.gameObject.SetActive(state);
					}					
				}
			}
		}
		public virtual void RefreshPoints()
		{
	        if (PointsText!= null)
	        { 
	    		PointsText.text = GameManager.Instance.Points.ToString(PointsTextPattern);
	        }
	    }
	    public virtual void UpdateHealthBar(float currentHealth,float minHealth,float maxHealth,string playerID)
	    {
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0)	{ return; }

	    	foreach (MMProgressBar healthBar in HealthBars)
	    	{
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID)
				{
					healthBar.UpdateBar(currentHealth,minHealth,maxHealth);
				}
	    	}

	    }
		public virtual void UpdateDashBars(float currentFuel, float minFuel, float maxFuel,string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar dashbar in DashBars)
	    	{
				if (dashbar == null) { return; }
				if (dashbar.PlayerID == playerID)
				{
					dashbar.UpdateBar(currentFuel,minFuel,maxFuel);	
		    	}    
			}
	    }
		public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, string playerID, int ammoDisplayID, bool displayTotal)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay == null) { return; }
				if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
                {
					ammoDisplay.UpdateAmmoDisplays (magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, displayTotal);
				}    
			}
		}
	}
}
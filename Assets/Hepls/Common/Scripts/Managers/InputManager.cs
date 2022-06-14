using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Managers/Input Manager")]
	public class InputManager : MMSingleton<InputManager>
	{
		[Header("Status")]
		[Tooltip("set this to false to prevent the InputManager from reading input")]
		public bool InputDetectionActive = true;
        
		[Header("Player binding")]
		[MMInformation("The first thing you need to set on your InputManager is the PlayerID. This ID will be used to bind the input manager to your character(s). You'll want to go with Player1, Player2, Player3 or Player4.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("a string identifying the target player(s). You'll need to set this exact same string on your Character, and set its type to Player")]
		public string PlayerID = "Player1";
		public enum InputForcedModes { None, Mobile, Desktop }
		public enum MovementControls { Joystick, Arrows }
		[Header("Mobile controls")]
		[MMInformation("If you check Auto Mobile Detection, the engine will automatically switch to mobile controls when your build target is Android or iOS. You can also force mobile or desktop (keyboard, gamepad) controls using the dropdown below.\nNote that if you don't need mobile controls and/or GUI this component can also work on its own, just put it on an empty GameObject instead.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("if this is set to true, the InputManager will try to detect what mode it should be in, based on the current target device")]
		public bool AutoMobileDetection = true;
		[Tooltip("use this to force desktop (keyboard, pad) or mobile (touch) mode")]
		public InputForcedModes InputForcedMode;
		[Tooltip("if this is true, the weapon mode will be forced to the selected WeaponForcedMode")]
		public bool ForceWeaponMode = false;
        [MMCondition("ForceWeaponMode", true)]
		[Tooltip("use this to force a control mode for weapons")]
		public WeaponAim.AimControls WeaponForcedMode;
		[Tooltip("if this is true, mobile controls will be hidden in editor mode, regardless of the current build target or the forced mode")]
		public bool HideMobileControlsInEditor = false;
		[Tooltip("use this to specify whether you want to use the default joystick or arrows to move your character")]
		public MovementControls MovementControl = MovementControls.Joystick;
		public bool IsMobile { get; protected set; }

		[Header("Movement settings")]
		[MMInformation("Turn SmoothMovement on to have inertia in your controls (meaning there'll be a small delay between a press/release of a direction and your character moving/stopping). You can also define here the horizontal and vertical thresholds.",MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("If set to true, acceleration / deceleration will take place when moving / stopping")]
		public bool SmoothMovement=true;
		[Tooltip("the minimum horizontal and vertical value you need to reach to trigger movement on an analog controller (joystick for example)")]
		public Vector2 Threshold = new Vector2(0.1f, 0.4f);

        [Header("Camera Rotation")]
        [MMInformation("Here you can decide whether or not camera rotation should impact your input. That can be useful in, for example, a 3D isometric game, if you want 'up' to mean some other direction than Vector3.up/forward.", MMInformationAttribute.InformationType.Info, false)]
		[Tooltip("if this is true, any directional input coming into this input manager will be rotated to align with the current camera orientation")]
        public bool RotateInputBasedOnCameraDirection = false;
        public MMInput.IMButton JumpButton { get; protected set; }
		public MMInput.IMButton RunButton { get; protected set; }
		public MMInput.IMButton DashButton { get; protected set; }
		public MMInput.IMButton CrouchButton { get; protected set; }
		public MMInput.IMButton ShootButton { get; protected set; }
        public MMInput.IMButton InteractButton { get; protected set; }
        public MMInput.IMButton SecondaryShootButton { get; protected set; }
        public MMInput.IMButton ReloadButton { get; protected set; }
        public MMInput.IMButton PauseButton { get; protected set; }
        public MMInput.IMButton TimeControlButton { get; protected set; }
        public MMInput.IMButton SwitchCharacterButton { get; protected set; }
        public MMInput.IMButton SwitchWeaponButton { get; protected set; }
		public MMInput.ButtonStates ShootAxis { get; protected set; }
        public MMInput.ButtonStates SecondaryShootAxis { get; protected set; }
        public Vector2 PrimaryMovement { get { return _primaryMovement; } }
        public Vector2 SecondaryMovement { get { return _secondaryMovement; } }
        public Vector2 LastNonNullPrimaryMovement { get; set; }
        public Vector2 LastNonNullSecondaryMovement { get; set; }
        public float CameraRotationInput { get { return _cameraRotationInput; } }

        protected Camera _targetCamera;
        protected bool _camera3D;
        protected float _cameraAngle;
        protected List<MMInput.IMButton> ButtonList;
        protected Vector2 _primaryMovement = Vector2.zero;
        protected Vector2 _secondaryMovement = Vector2.zero;
        protected float _cameraRotationInput = 0f;
        protected string _axisHorizontal;
		protected string _axisVertical;
		protected string _axisSecondaryHorizontal;
		protected string _axisSecondaryVertical;
		protected string _axisShoot;
        protected string _axisShootSecondary;
        protected string _axisCamera;
        protected override void Awake()
		{
            base.Awake();
            Initialization();
        }
        protected virtual void Initialization()
        {
            ControlsModeDetection();
            InitializeButtons();
            InitializeAxis();
        }
		public virtual void ControlsModeDetection()
		{
			if (GUIManager.Instance!=null)
			{
				GUIManager.Instance.SetMobileControlsActive(false);
				IsMobile=false;
				if (AutoMobileDetection)
				{
					#if UNITY_ANDROID || UNITY_IPHONE
					GUIManager.Instance.SetMobileControlsActive(true,MovementControl);
					IsMobile = true;
					#endif
				}
				if (InputForcedMode==InputForcedModes.Mobile)
				{
					GUIManager.Instance.SetMobileControlsActive(true,MovementControl);
					IsMobile = true;
				}
				if (InputForcedMode==InputForcedModes.Desktop)
				{
					GUIManager.Instance.SetMobileControlsActive(false);
					IsMobile = false;					
				}
				if (HideMobileControlsInEditor)
				{
					#if UNITY_EDITOR
					GUIManager.Instance.SetMobileControlsActive(false);
					IsMobile = false;	
					#endif
				}
			}
		}
		protected virtual void InitializeButtons()
		{
			ButtonList = new List<MMInput.IMButton> ();
			ButtonList.Add(JumpButton = new MMInput.IMButton (PlayerID, "Jump", JumpButtonDown, JumpButtonPressed, JumpButtonUp));
			ButtonList.Add(RunButton  = new MMInput.IMButton (PlayerID, "Run", RunButtonDown, RunButtonPressed, RunButtonUp));
            ButtonList.Add(InteractButton = new MMInput.IMButton(PlayerID, "Interact", InteractButtonDown, InteractButtonPressed, InteractButtonUp));
            ButtonList.Add(DashButton  = new MMInput.IMButton (PlayerID, "Dash", DashButtonDown, DashButtonPressed, DashButtonUp));
			ButtonList.Add(CrouchButton  = new MMInput.IMButton (PlayerID, "Crouch", CrouchButtonDown, CrouchButtonPressed, CrouchButtonUp));
            ButtonList.Add(SecondaryShootButton = new MMInput.IMButton(PlayerID, "SecondaryShoot", SecondaryShootButtonDown, SecondaryShootButtonPressed, SecondaryShootButtonUp));
            ButtonList.Add(ShootButton = new MMInput.IMButton (PlayerID, "Shoot", ShootButtonDown, ShootButtonPressed, ShootButtonUp)); 
			ButtonList.Add(ReloadButton = new MMInput.IMButton (PlayerID, "Reload", ReloadButtonDown, ReloadButtonPressed, ReloadButtonUp));
			ButtonList.Add(SwitchWeaponButton = new MMInput.IMButton (PlayerID, "SwitchWeapon", SwitchWeaponButtonDown, SwitchWeaponButtonPressed, SwitchWeaponButtonUp));
            ButtonList.Add(PauseButton = new MMInput.IMButton(PlayerID, "Pause", PauseButtonDown, PauseButtonPressed, PauseButtonUp));
            ButtonList.Add(TimeControlButton = new MMInput.IMButton(PlayerID, "TimeControl", TimeControlButtonDown, TimeControlButtonPressed, TimeControlButtonUp));
            ButtonList.Add(SwitchCharacterButton = new MMInput.IMButton(PlayerID, "SwitchCharacter", SwitchCharacterButtonDown, SwitchCharacterButtonPressed, SwitchCharacterButtonUp));
        }
        protected virtual void InitializeAxis()
		{
			_axisHorizontal = PlayerID+"_Horizontal";
			_axisVertical = PlayerID+"_Vertical";
			_axisSecondaryHorizontal = PlayerID+"_SecondaryHorizontal";
			_axisSecondaryVertical = PlayerID+"_SecondaryVertical";
			_axisShoot = PlayerID+"_ShootAxis";
            _axisShootSecondary = PlayerID + "_SecondaryShootAxis";
            _axisCamera = PlayerID + "_CameraRotationAxis";
        }
		protected virtual void LateUpdate()
		{
			ProcessButtonStates();
		}
		protected virtual void Update()
		{		
			if (!IsMobile && InputDetectionActive)
			{	
				SetMovement();	
				SetSecondaryMovement ();
				SetShootAxis ();
                SetCameraRotationAxis();
				GetInputButtons ();
                GetLastNonNullValues();
			}									
		}
        protected virtual void GetLastNonNullValues()
        {
            if (_primaryMovement.magnitude > Threshold.x)
            {
                LastNonNullPrimaryMovement = _primaryMovement;
            }
            if (_secondaryMovement.magnitude > Threshold.x)
            {
                LastNonNullSecondaryMovement = _secondaryMovement;
            }
        }
        protected virtual void GetInputButtons()
		{
			foreach(MMInput.IMButton button in ButtonList)
			{
				if (Input.GetButton(button.ButtonID))
				{
					button.TriggerButtonPressed ();
				}
				if (Input.GetButtonDown(button.ButtonID))
				{
					button.TriggerButtonDown ();
				}
				if (Input.GetButtonUp(button.ButtonID))
				{
					button.TriggerButtonUp ();
				}
			}
		}
		public virtual void ProcessButtonStates()
		{
			foreach (MMInput.IMButton button in ButtonList)
			{
                if (button.State.CurrentState == MMInput.ButtonStates.ButtonDown)
				{
					button.State.ChangeState(MMInput.ButtonStates.ButtonPressed);				
				}	
				if (button.State.CurrentState == MMInput.ButtonStates.ButtonUp)
				{
					button.State.ChangeState(MMInput.ButtonStates.Off);				
				}	
			}
		}
		public virtual void SetMovement()
		{
			if (!IsMobile && InputDetectionActive)
			{
				if (SmoothMovement)
				{
					_primaryMovement.x = Input.GetAxis(_axisHorizontal);
					_primaryMovement.y = Input.GetAxis(_axisVertical);		
				}
				else
				{
					_primaryMovement.x = Input.GetAxisRaw(_axisHorizontal);
					_primaryMovement.y = Input.GetAxisRaw(_axisVertical);
                }
                _primaryMovement = ApplyCameraRotation(_primaryMovement);
            }
		}
		public virtual void SetSecondaryMovement()
		{
			if (!IsMobile && InputDetectionActive)
			{
				if (SmoothMovement)
				{
					_secondaryMovement.x = Input.GetAxis(_axisSecondaryHorizontal);
					_secondaryMovement.y = Input.GetAxis(_axisSecondaryVertical);		
				}
				else
				{
					_secondaryMovement.x = Input.GetAxisRaw(_axisSecondaryHorizontal);
					_secondaryMovement.y = Input.GetAxisRaw(_axisSecondaryVertical);
                }
                _secondaryMovement = ApplyCameraRotation(_secondaryMovement);
            }
		}
		protected virtual void SetShootAxis()
		{
			if (!IsMobile && InputDetectionActive)
			{
				ShootAxis = MMInput.ProcessAxisAsButton (_axisShoot, Threshold.y, ShootAxis);
                SecondaryShootAxis = MMInput.ProcessAxisAsButton(_axisShootSecondary, -Threshold.y, SecondaryShootAxis, MMInput.AxisTypes.Negative);
            }
		}
        protected virtual void SetCameraRotationAxis()
        {
            _cameraRotationInput = Input.GetAxis(_axisCamera);
        }
		public virtual void SetMovement(Vector2 movement)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.x = movement.x;
				_primaryMovement.y = movement.y;
            }
            _primaryMovement = ApplyCameraRotation(_primaryMovement);
        }
		public virtual void SetSecondaryMovement(Vector2 movement)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.x = movement.x;
				_secondaryMovement.y = movement.y;
            }
            _secondaryMovement = ApplyCameraRotation(_secondaryMovement);
        }
		public virtual void SetHorizontalMovement(float horizontalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.x = horizontalInput;
			}
		}
		public virtual void SetVerticalMovement(float verticalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_primaryMovement.y = verticalInput;
			}
		}
		public virtual void SetSecondaryHorizontalMovement(float horizontalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.x = horizontalInput;
			}
		}
		public virtual void SetSecondaryVerticalMovement(float verticalInput)
		{
			if (IsMobile && InputDetectionActive)
			{
				_secondaryMovement.y = verticalInput;
			}
		}
        public virtual void SetCamera(Camera targetCamera, bool camera3D)
        {
            _targetCamera = targetCamera;
            _camera3D = camera3D;
        }
        public virtual Vector2 ApplyCameraRotation(Vector2 input)
        {
            if (RotateInputBasedOnCameraDirection)
            {
                if (_camera3D)
                {
                    _cameraAngle = _targetCamera.transform.localEulerAngles.y;
                    return MMMaths.RotateVector2(input, -_cameraAngle);
                }
                else
                {
                    _cameraAngle = _targetCamera.transform.localEulerAngles.z;
                    return MMMaths.RotateVector2(input, _cameraAngle);
                }
            }
            else
            {
                return input;
            }
        }

		public virtual void JumpButtonDown()		{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void JumpButtonPressed()		{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void JumpButtonUp()			{ JumpButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public virtual void DashButtonDown()		{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void DashButtonPressed()		{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void DashButtonUp()			{ DashButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public virtual void CrouchButtonDown()		{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void CrouchButtonPressed()	{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void CrouchButtonUp()		{ CrouchButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public virtual void RunButtonDown()			{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void RunButtonPressed()		{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void RunButtonUp()			{ RunButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

		public virtual void ReloadButtonDown()		{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void ReloadButtonPressed()	{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void ReloadButtonUp()		{ ReloadButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

        public virtual void InteractButtonDown() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void InteractButtonPressed() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void InteractButtonUp() { InteractButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void ShootButtonDown()		{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void ShootButtonPressed()	{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void ShootButtonUp()			{ ShootButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

        public virtual void SecondaryShootButtonDown() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SecondaryShootButtonPressed() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SecondaryShootButtonUp() { SecondaryShootButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void PauseButtonDown() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void PauseButtonPressed() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void PauseButtonUp() { PauseButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void TimeControlButtonDown() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void TimeControlButtonPressed() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void TimeControlButtonUp() { TimeControlButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchWeaponButtonDown()		{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonDown); }
		public virtual void SwitchWeaponButtonPressed()		{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonPressed); }
		public virtual void SwitchWeaponButtonUp()			{ SwitchWeaponButton.State.ChangeState (MMInput.ButtonStates.ButtonUp); }

        public virtual void SwitchCharacterButtonDown() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonDown); }
        public virtual void SwitchCharacterButtonPressed() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed); }
        public virtual void SwitchCharacterButtonUp() { SwitchCharacterButton.State.ChangeState(MMInput.ButtonStates.ButtonUp); }
    }
}
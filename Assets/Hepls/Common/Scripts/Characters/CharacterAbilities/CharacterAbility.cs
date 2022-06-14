using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Linq;

namespace MoreMountains.TopDownEngine
{
	public class CharacterAbility : MonoBehaviour 
	{
		[Tooltip("the sound fx to play when the ability starts")]
		public AudioClip AbilityStartSfx;
		[Tooltip("the sound fx to play while the ability is running")]
		public AudioClip AbilityInProgressSfx;
		[Tooltip("the sound fx to play when the ability stops")]
		public AudioClip AbilityStopSfx;
		[Tooltip("the feedbacks to play when the ability starts")]
		public MMFeedbacks AbilityStartFeedbacks;
		[Tooltip("the feedbacks to play when the ability stops")]
		public MMFeedbacks AbilityStopFeedbacks;
                
        [Header("Permission")]
        [Tooltip("if true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example")]
		public bool AbilityPermitted = true;
        [Tooltip("an array containing all the blocking movement states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while Idle or Swimming, for example.")]
        public CharacterStates.MovementStates[] BlockingMovementStates;
        [Tooltip("an array containing all the blocking condition states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while dead, for example.")]
        public CharacterStates.CharacterConditions[] BlockingConditionStates;

        public virtual bool AbilityAuthorized
        {
	        get
	        {
		        if (_character != null)
		        {
			        if ((BlockingMovementStates != null) && (BlockingMovementStates.Length > 0))
			        {
				        for (int i = 0; i < BlockingMovementStates.Length; i++)
				        {
					        if (BlockingMovementStates[i] == (_character.MovementState.CurrentState))
					        {
						        return false;
					        }    
				        }
			        }

			        if ((BlockingConditionStates != null) && (BlockingConditionStates.Length > 0))
			        {
				        for (int i = 0; i < BlockingConditionStates.Length; i++)
				        {
					        if (BlockingConditionStates[i] == (_character.ConditionState.CurrentState))
					        {
						        return false;
					        }    
				        }
			        }
		        }
		        return AbilityPermitted;
	        }
        }
		public bool AbilityInitialized { get { return _abilityInitialized; } }
        
		protected Character _character;
        protected TopDownController _controller;
        protected TopDownController2D _controller2D;
        protected TopDownController3D _controller3D;
        protected GameObject _model;
	    protected Health _health;
		protected CharacterMovement _characterMovement;
		protected InputManager _inputManager;
		protected Animator _animator = null;
		protected CharacterStates _state;
		protected SpriteRenderer _spriteRenderer;
		protected MMStateMachine<CharacterStates.MovementStates> _movement;
		protected MMStateMachine<CharacterStates.CharacterConditions> _condition;
		protected AudioSource _abilityInProgressSfx;
		protected bool _abilityInitialized = false;
		protected float _verticalInput;
		protected float _horizontalInput;
        protected bool _startFeedbackIsPlaying = false;
        public virtual string HelpBoxText() { return ""; }
		protected virtual void Awake()
		{
			PreInitialization ();
		}
		protected virtual void Start () 
		{
			Initialization();
		}
		protected virtual void PreInitialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            BindAnimator();
        }
		protected virtual void Initialization()
        {
            BindAnimator();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
            _controller2D = this.gameObject.GetComponentInParent<TopDownController2D>();
            _controller3D = this.gameObject.GetComponentInParent<TopDownController3D>();
            _model = _character.CharacterModel;
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_spriteRenderer = this.gameObject.GetComponentInParent<SpriteRenderer>();
            _health = _character._health;
			_inputManager = _character.LinkedInputManager;
			_state = _character.CharacterState;
			_movement = _character.MovementState;
			_condition = _character.ConditionState;
			_abilityInitialized = true;
        }
        protected virtual void BindAnimator()
        {
            if (_character._animator == null)
            {
                _character.AssignAnimator();
            }

            _animator = _character._animator;

            if (_animator != null)
            {
                InitializeAnimatorParameters();
            }
        }
        protected virtual void InitializeAnimatorParameters()
		{

		}
		protected virtual void InternalHandleInput()
		{
			if (_inputManager == null) { return; }
			_horizontalInput = _inputManager.PrimaryMovement.x;
			_verticalInput = _inputManager.PrimaryMovement.y;
			HandleInput();
		}
		protected virtual void HandleInput()
		{

        }
        public virtual void ResetInput()
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
        }
        public virtual void EarlyProcessAbility()
		{
			InternalHandleInput();
		}
		public virtual void ProcessAbility()
		{
			
		}
		public virtual void LateProcessAbility()
		{
			
		}
		public virtual void UpdateAnimator()
		{

		}
		public virtual void PermitAbility(bool abilityPermitted)
		{
			AbilityPermitted = abilityPermitted;
		}
		public virtual void Flip()
		{
			
		}
		public virtual void ResetAbility()
		{
			
		}
        public virtual void SetInputManager(InputManager newInputManager)
        {
            _inputManager = newInputManager;
        }
		protected virtual void PlayAbilityStartSfx()
		{
			if (AbilityStartSfx!=null)
			{
				AudioSource tmp = new AudioSource();
				MMSoundManagerSoundPlayEvent.Trigger(AbilityStartSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);	
			}
		}
		protected virtual void PlayAbilityUsedSfx()
		{
			if (AbilityInProgressSfx != null) 
			{	
				if (_abilityInProgressSfx == null)
				{
					_abilityInProgressSfx = MMSoundManagerSoundPlayEvent.Trigger(AbilityInProgressSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position, true);
				}
			}
		}
		protected virtual void StopAbilityUsedSfx()
		{
			if (_abilityInProgressSfx != null)
			{
				MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, 0, _abilityInProgressSfx);
				_abilityInProgressSfx = null;
			}
		}
		protected virtual void PlayAbilityStopSfx()
		{
			if (AbilityStopSfx!=null) 
			{	
				MMSoundManagerSoundPlayEvent.Trigger(AbilityStopSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}
		protected virtual void PlayAbilityStartFeedbacks()
        {
            AbilityStartFeedbacks?.PlayFeedbacks(this.transform.position);
            _startFeedbackIsPlaying = true;
        }
        public virtual void StopStartFeedbacks()
        {
            AbilityStartFeedbacks?.StopFeedbacks();
            _startFeedbackIsPlaying = false;
        }
		protected virtual void PlayAbilityStopFeedbacks()
        {
            AbilityStopFeedbacks?.PlayFeedbacks();
        }
        protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType, out int parameter)
		{
            parameter = Animator.StringToHash(parameterName);

            if (_animator == null) 
			{
				return;
			}
			if (_animator.MMHasParameterOfType(parameterName, parameterType))
			{
				if (_character != null)
				{
					_character._animatorParameters.Add(parameter);	
				}
			}
		}
		protected virtual void OnRespawn()
		{
		}
		protected virtual void OnDeath()
		{
			StopAbilityUsedSfx ();
            StopStartFeedbacks();
        }
        protected virtual void OnHit()
        {

        }
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Health> ();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRespawn;
				_health.OnDeath += OnDeath;
                _health.OnHit += OnHit;
			}
		}
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRespawn;
				_health.OnDeath -= OnDeath;
                _health.OnHit -= OnHit;
            }	
		}
	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Movement")] 
	public class CharacterMovement : CharacterAbility 
	{
        public enum Movements { Free, Strict2DirectionsHorizontal, Strict2DirectionsVertical, Strict4Directions, Strict8Directions }
        public float MovementSpeed { get; set; }
        public bool MovementForbidden { get; set; }

        [Header("Direction")]
        [Tooltip("whether the character can move freely, in 2D only, in 4 or 8 cardinal directions")]
        public Movements Movement = Movements.Free;

        [Header("Settings")]
        [Tooltip("whether or not movement input is authorized at that time")]
        public bool InputAuthorized = true;
        [Tooltip("whether or not input should be analog")]
        public bool AnalogInput = false;
        [Tooltip("whether or not input should be set from another script")]
        public bool ScriptDrivenInput = false;

        [Header("Speed")]
        [Tooltip("the speed of the character when it's walking")]
        public float WalkSpeed = 6f;
		[Tooltip("whether or not this component should set the controller's movement")]
        public bool ShouldSetMovement = true;
        [Tooltip("the speed threshold after which the character is not considered idle anymore")]
        public float IdleThreshold = 0.05f;

		[Header("Acceleration")]
        [Tooltip("the acceleration to apply to the current speed / 0f : no acceleration, instant full speed")]
        public float Acceleration = 10f;
        [Tooltip("the deceleration to apply to the current speed / 0f : no deceleration, instant stop")]
        public float Deceleration = 10f;
        [Tooltip("whether or not to interpolate movement speed")]
        public bool InterpolateMovementSpeed = false;
        public float MovementSpeedMultiplier { get; set; }

		[Header("Walk Feedback")]
        [Tooltip("the particles to trigger while walking")]
        public ParticleSystem[] WalkParticles;

		[Header("Touch The Ground Feedback")]
        [Tooltip("the particles to trigger when touching the ground")]
        public ParticleSystem[] TouchTheGroundParticles;
        [Tooltip("the sfx to trigger when touching the ground")]
        public AudioClip[] TouchTheGroundSfx;

        protected float _movementSpeed;
		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected Vector3 _movementVector;
		protected Vector2 _currentInput = Vector2.zero;
		protected Vector2 _normalizedInput;
		protected Vector2 _lerpedInput = Vector2.zero;
		protected float _acceleration = 0f;
		protected bool _walkParticlesPlaying = false;

        protected const string _speedAnimationParameterName = "Speed";
        protected const string _walkingAnimationParameterName = "Walking";
        protected const string _idleAnimationParameterName = "Idle";
        protected int _speedAnimationParameter;
        protected int _walkingAnimationParameter;
        protected int _idleAnimationParameter;
        protected override void Initialization()
		{
			base.Initialization ();
			MovementSpeed = WalkSpeed;
            _movement.ChangeState(CharacterStates.MovementStates.Idle);
			MovementSpeedMultiplier = 1f;
            MovementForbidden = false;

            foreach (ParticleSystem system in TouchTheGroundParticles)
			{
                if (system != null)
                {
                    system.Stop();
                }				
			}
			foreach (ParticleSystem system in WalkParticles)
			{
                if (system != null)
                {
                    system.Stop();
                }				
			}
		}
		public override void ProcessAbility()
	    {
			base.ProcessAbility();

			if (!AbilityAuthorized)
			{
				return;
			}
			
            HandleDirection();
            HandleFrozen();
			HandleMovement();
			Feedbacks ();
	    }
		protected override void HandleInput()
		{
			if (ScriptDrivenInput)
			{
				return;
			}

			if (InputAuthorized)
            {
                _horizontalMovement = _horizontalInput;
                _verticalMovement = _verticalInput;
            }
			else
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }
		}
		public virtual void SetMovement(Vector2 value)
		{
			_horizontalMovement = value.x;
			_verticalMovement = value.y;
        }
        public virtual void SetHorizontalMovement(float value)
        {
            _horizontalMovement = value;
        }
        public virtual void SetVerticalMovement(float value)
        {
            _verticalMovement = value;
        }
        protected virtual void HandleDirection()
        {
            switch (Movement)
            {
                case Movements.Free:
                    break;
                case Movements.Strict2DirectionsHorizontal:
                    _verticalMovement = 0f;
                    break;
                case Movements.Strict2DirectionsVertical:
                    _horizontalMovement = 0f;
                    break;
                case Movements.Strict4Directions:
                    if (Mathf.Abs(_horizontalMovement) > Mathf.Abs(_verticalMovement))
                    {
                        _verticalMovement = 0f;
                    }
                    else
                    {
                        _horizontalMovement = 0f;
                    }
                    break;
                case Movements.Strict8Directions:
                    _verticalMovement = Mathf.Round(_verticalMovement);
                    _horizontalMovement = Mathf.Round(_horizontalMovement);
                    break;
            }
        }
        protected virtual void HandleMovement()
		{
            if ((_movement.CurrentState != CharacterStates.MovementStates.Walking) && _startFeedbackIsPlaying)
            {
                StopStartFeedbacks();
            }
            if (_movement.CurrentState != CharacterStates.MovementStates.Walking && _abilityInProgressSfx != null)
			{
				StopAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Walking && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}
			if ( !AbilityAuthorized
				|| (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) )
            {
                return;				
			}
            
			CheckJustGotGrounded();

            if (MovementForbidden)
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
            }
            if (!_controller.Grounded
                && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
                && (
                    (_movement.CurrentState == CharacterStates.MovementStates.Walking)
                    || (_movement.CurrentState == CharacterStates.MovementStates.Idle)
                ))
            {
                _movement.ChangeState(CharacterStates.MovementStates.Falling);
            }

            if (_controller.Grounded && (_movement.CurrentState == CharacterStates.MovementStates.Falling))
            {
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
            }

            if ( _controller.Grounded
				&& (_controller.CurrentMovement.magnitude > IdleThreshold)
				&& ( _movement.CurrentState == CharacterStates.MovementStates.Idle))
			{				
				_movement.ChangeState(CharacterStates.MovementStates.Walking);	
				PlayAbilityStartSfx();	
				PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
            }
			if ((_movement.CurrentState == CharacterStates.MovementStates.Walking) 
				&& (_controller.CurrentMovement.magnitude <= IdleThreshold))
			{
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
				PlayAbilityStopSfx();
                PlayAbilityStopFeedbacks();
            }

            if (ShouldSetMovement)
            {
                SetMovement ();	
			}
		}
        protected virtual void HandleFrozen()
        {
            if (_condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
            {
                _horizontalMovement = 0f;
                _verticalMovement = 0f;
                SetMovement();
            }
        }
		protected virtual void SetMovement()
		{
            _movementVector = Vector3.zero;
			_currentInput = Vector2.zero;

			_currentInput.x = _horizontalMovement;
			_currentInput.y = _verticalMovement;
            
            _normalizedInput = _currentInput.normalized;

            float interpolationSpeed = 1f;
            
			if ((Acceleration == 0) || (Deceleration == 0))
			{
				_lerpedInput = _currentInput;
			}
			else
			{
				if (_normalizedInput.magnitude == 0)
				{
					_acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
                    _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
                    interpolationSpeed = Deceleration;
				}
				else
				{
                    _acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
                    _lerpedInput = AnalogInput ? Vector2.ClampMagnitude (_currentInput, _acceleration) : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
                    interpolationSpeed = Acceleration;
                }
			}		
			
			_movementVector.x = _lerpedInput.x;
            _movementVector.y = 0f;
			_movementVector.z = _lerpedInput.y;

            if (InterpolateMovementSpeed)
            {
                _movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * MovementSpeedMultiplier, interpolationSpeed * Time.deltaTime);
            }
            else
            {
                _movementSpeed = MovementSpeed * MovementSpeedMultiplier;
            }

			_movementVector *= _movementSpeed;

            

			if (_movementVector.magnitude > MovementSpeed)
			{
				_movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
			}

            if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
            {
                _movementVector = Vector3.zero;
            }
            
			_controller.SetMovement (_movementVector);

		}
		protected virtual void CheckJustGotGrounded()
		{
			if (_controller.JustGotGrounded)
			{
                _movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
		}
		protected virtual void Feedbacks ()
		{
			if (_controller.Grounded)
			{
				if (_controller.CurrentMovement.magnitude > IdleThreshold)	
				{			
					foreach (ParticleSystem system in WalkParticles)
					{				
						if (!_walkParticlesPlaying && (system != null))
						{
							system.Play();		
						}
						_walkParticlesPlaying = true;
					}	
				}
				else
				{
					foreach (ParticleSystem system in WalkParticles)
					{						
						if (_walkParticlesPlaying && (system != null))
						{
							system.Stop();		
							_walkParticlesPlaying = false;
						}
					}
				}
			}
			else
			{
				foreach (ParticleSystem system in WalkParticles)
				{						
					if (_walkParticlesPlaying && (system != null))
					{
						system.Stop();		
						_walkParticlesPlaying = false;
					}
				}
			}

			if (_controller.JustGotGrounded)
			{
				foreach (ParticleSystem system in TouchTheGroundParticles)
				{
                    if (system != null)
                    {
                        system.Clear();
                        system.Play();
                    }					
				}
				foreach (AudioClip clip in TouchTheGroundSfx)
				{
					MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}
			}
		}
		public virtual void ResetSpeed()
		{
			MovementSpeed = WalkSpeed;
		}
        protected override void OnRespawn()
        {
            ResetSpeed();
            MovementForbidden = false;
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            if (WalkParticles.Length > 0)
            {
                foreach (ParticleSystem walkParticle in WalkParticles)
                {
                    if (walkParticle != null)
                    {
                        walkParticle.Stop();
                    }
                }
            }
        }
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
			RegisterAnimatorParameter (_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
			RegisterAnimatorParameter (_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
		}
		public override void UpdateAnimator()
		{
            MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Abs(_controller.CurrentMovement.magnitude),_character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),_character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
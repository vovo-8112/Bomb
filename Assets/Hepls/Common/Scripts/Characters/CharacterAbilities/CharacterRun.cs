using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Run")] 
	public class CharacterRun : CharacterAbility
	{
		public override string HelpBoxText() { return "This component allows your character to change speed (defined here) when pressing the run button."; }

		[Header("Speed")]
		[Tooltip("the speed of the character when it's running")]
		public float RunSpeed = 16f;

        [Header("AutoRun")]
		[Tooltip("whether or not run should auto trigger if you move the joystick far enough")]
		public bool AutoRun = false;
		[Tooltip("the input threshold on the joystick (normalized)")]
		public float AutoRunThreshold = 0.6f;

        protected const string _runningAnimationParameterName = "Running";
        protected int _runningAnimationParameter;
        protected bool _runningStarted = false;
        protected override void HandleInput()
		{
            if (AutoRun)
            {
                if (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold)
                {
                    _inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }
            }

            if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonDown || _inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
			{
				RunStart();
			}				
			if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				RunStop();
			}
            else
            {
                if (AutoRun)
                {
                    if (_inputManager.PrimaryMovement.magnitude <= AutoRunThreshold)
                    {
                        _inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                        RunStop();
                    }
                }
            }          
        }
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleRunningExit();
		}
		protected virtual void HandleRunningExit()
		{
			if (!_controller.Grounded
                && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
                && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
                StopFeedbacks();
                StopSfx ();
			}
			if ((Mathf.Abs(_controller.CurrentMovement.magnitude) < RunSpeed / 10) && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState (CharacterStates.MovementStates.Idle);
                StopFeedbacks();
                StopSfx ();
			}
			if (!_controller.Grounded && _abilityInProgressSfx != null)
            {
                StopFeedbacks();
                StopSfx ();
			}
		}
		public virtual void RunStart()
		{		
			if ( !AbilityAuthorized
				|| (!_controller.Grounded)
				|| (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
				|| (_movement.CurrentState != CharacterStates.MovementStates.Walking) )
			{
				return;
			}
			if (_characterMovement != null)
			{
				_characterMovement.MovementSpeed = RunSpeed;
			}
			if (_movement.CurrentState != CharacterStates.MovementStates.Running)
			{
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
                PlayAbilityStartFeedbacks();
                _runningStarted = true;
            }

			_movement.ChangeState(CharacterStates.MovementStates.Running);
		}
        public virtual void RunStop()
        {
            if (_runningStarted)
            {
                if ((_characterMovement != null))
                {
                    _characterMovement.ResetSpeed();
                    _movement.ChangeState(CharacterStates.MovementStates.Idle);
                }
                StopFeedbacks();
                StopSfx();
                _runningStarted = false;
            }            
        }
		protected virtual void StopFeedbacks()
        {
            if (_startFeedbackIsPlaying)
            {
                StopStartFeedbacks();
                PlayAbilityStopFeedbacks();
            }
        }
        protected virtual void StopSfx()
		{
			StopAbilityUsedSfx();
			PlayAbilityStopSfx();
		}
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_runningAnimationParameterName, AnimatorControllerParameterType.Bool, out _runningAnimationParameter);
		}
		public override void UpdateAnimator()
		{
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _runningAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Running),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
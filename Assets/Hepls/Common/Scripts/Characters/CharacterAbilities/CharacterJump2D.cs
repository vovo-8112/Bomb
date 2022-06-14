using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Jump 2D")]
    public class CharacterJump2D : CharacterAbility 
	{
        [Tooltip("the duration of the jump")]
        public float JumpDuration = 1f;
        [Tooltip("whether or not jump should be proportional to press (if yes, releasing the button will stop the jump)")]
        public bool JumpProportionalToPress = true;
        [Tooltip("the minimum amount of time after the jump starts before releasing the jump has any effect")]
        public float MinimumPressTime = 0.4f;
        [Tooltip("the feedback to play when the jump starts")]
        public MMFeedbacks JumpStartFeedback;
        [Tooltip("the feedback to play when the jump stops")]
        public MMFeedbacks JumpStopFeedback;

        protected CharacterButtonActivation _characterButtonActivation;
        protected bool _jumpStopped = false;
        protected float _jumpStartedAt = 0f;
        protected bool _buttonReleased = false;
        protected const string _jumpingAnimationParameterName = "Jumping";
        protected const string _hitTheGroundAnimationParameterName = "HitTheGround";
        protected int _jumpingAnimationParameter;
        protected int _hitTheGroundAnimationParameter;
        protected override void Initialization()
		{
			base.Initialization ();
			_characterButtonActivation = _character?.FindAbility<CharacterButtonActivation> ();
            JumpStartFeedback?.Initialization(this.gameObject);
            JumpStopFeedback?.Initialization(this.gameObject);
		}
		protected override void HandleInput()
		{
			base.HandleInput();
            if (!AbilityAuthorized
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
            {
                return;
            }
            if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
                JumpStart();
            }
            if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
            {
                _buttonReleased = true;
            }
        }
        public override void ProcessAbility()
        {
            if (_movement.CurrentState == CharacterStates.MovementStates.Jumping)
            {
                if (!_jumpStopped)
                {
                    if (Time.time - _jumpStartedAt >= JumpDuration)
                    {
                        JumpStop();
                    }
                    else
                    {
                        _movement.ChangeState(CharacterStates.MovementStates.Jumping);
                    }
                }
                if (_buttonReleased
                   && !_jumpStopped
                   && JumpProportionalToPress
                   && (Time.time - _jumpStartedAt > MinimumPressTime))
                {
                    JumpStop();
                }
            }
        }
		public virtual void JumpStart()
		{
			if (!EvaluateJumpConditions())
			{
				return;
			}
			_movement.ChangeState(CharacterStates.MovementStates.Jumping);	
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jump);
            JumpStartFeedback?.PlayFeedbacks(this.transform.position);
            PlayAbilityStartFeedbacks();

            _jumpStopped = false;
            _jumpStartedAt = Time.time;
            _buttonReleased = false;
        }
        public virtual void JumpStop()
        {
            _jumpStopped = true;
            _movement.ChangeState(CharacterStates.MovementStates.Idle);
            _buttonReleased = false;
            JumpStopFeedback?.PlayFeedbacks(this.transform.position);
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
        }
		protected virtual bool EvaluateJumpConditions()
		{
			if (!AbilityAuthorized)
			{
				return false;
			}
			if (_characterButtonActivation != null)
			{
				if (_characterButtonActivation.AbilityAuthorized
					&& _characterButtonActivation.InButtonActivatedZone)
				{
					return false;
				}
			}
			return true;
		}
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jumpingAnimationParameter);
			RegisterAnimatorParameter (_hitTheGroundAnimationParameterName, AnimatorControllerParameterType.Bool, out _hitTheGroundAnimationParameter);
		}
		public override void UpdateAnimator()
		{
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jumping),_character._animatorParameters, _character.RunAnimatorSanityChecks);
            MMAnimatorExtensions.UpdateAnimatorBool (_animator, _hitTheGroundAnimationParameter, _controller.JustGotGrounded, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}

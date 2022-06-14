using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStartFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Fall Down Holes 2D")]
    public class CharacterFallDownHoles2D : CharacterAbility
    {
        [Tooltip("the feedback to play when falling")]
        public MMFeedbacks FallingFeedback;

        protected Collider2D _holesTest;
        protected const string _fallingDownHoleAnimationParameterName = "FallingDownHole";
        protected int _fallingDownHoleAnimationParameter;
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            CheckForHoles();
        }
        protected virtual void CheckForHoles()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            
            if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            {
                return;
            }

            if (_controller2D.OverHole && !_controller2D.Grounded)
            { 
                if ((_movement.CurrentState != CharacterStates.MovementStates.Jumping)
                    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing)
                    && (_condition.CurrentState != CharacterStates.CharacterConditions.Dead))
                {
                    _movement.ChangeState(CharacterStates.MovementStates.FallingDownHole);
                    FallingFeedback?.PlayFeedbacks(this.transform.position);
                    PlayAbilityStartFeedbacks();
                    _health.Kill();
                }
            }
        }
		protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_fallingDownHoleAnimationParameterName, AnimatorControllerParameterType.Bool, out _fallingDownHoleAnimationParameter);
        }
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _fallingDownHoleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.FallingDownHole), _character._animatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}

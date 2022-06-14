using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Button Activation")] 
	public class CharacterButtonActivation : CharacterAbility 
	{
		public override string HelpBoxText() { return "This component allows your character to interact with button powered objects (dialogue zones, switches...). "; }
        public bool InButtonActivatedZone {get;set;}
        public bool InButtonAutoActivatedZone { get; set; }
        [MMReadOnly]
        [Tooltip("the current button activated zone this character is in")]
        public ButtonActivated ButtonActivatedZone;

		protected bool _activating = false;
        protected const string _activatingAnimationParameterName = "Activating";
        protected int _activatingAnimationParameter;
		protected override void Initialization()
		{
			base.Initialization();
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
            InButtonAutoActivatedZone = false;

        }
		protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            if (InButtonActivatedZone && (ButtonActivatedZone != null))
            {
                bool buttonPressed = false;
                switch (ButtonActivatedZone.InputType)
                {
                    case ButtonActivated.InputTypes.Default:
                        buttonPressed = (_inputManager.InteractButton.State.CurrentState == MMInput.ButtonStates.ButtonDown);
                        break;
                    case ButtonActivated.InputTypes.Button:
                        buttonPressed = (Input.GetButtonDown(_character.PlayerID + "_" + ButtonActivatedZone.InputButton));
                        break;
                    case ButtonActivated.InputTypes.Key:
                        buttonPressed = (Input.GetKeyDown(ButtonActivatedZone.InputKey));
                        break;
                }

                if (buttonPressed)
                {
                    ButtonActivation();
                }
            }
		}
		protected virtual void ButtonActivation()
		{
			if ((InButtonActivatedZone)
			    && (ButtonActivatedZone!=null)
				&& (_condition.CurrentState == CharacterStates.CharacterConditions.Normal || _condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
			    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing))
			{
				if (ButtonActivatedZone.CanOnlyActivateIfGrounded && !_controller.Grounded)
				{
					return;
				}
                if (ButtonActivatedZone.AutoActivation)
                {
                    return;
                }
                MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.ButtonActivation);

				ButtonActivatedZone.TriggerButtonAction();
                PlayAbilityStartFeedbacks();
                _activating = true;
			}
		}
        protected override void OnDeath()
        {
            base.OnDeath();
            InButtonActivatedZone = false;
            ButtonActivatedZone = null;
            InButtonAutoActivatedZone = false;
        }
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_activatingAnimationParameterName, AnimatorControllerParameterType.Bool, out _activatingAnimationParameter);
		}
		public override void UpdateAnimator()
		{
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _activatingAnimationParameter, _activating, _character._animatorParameters, _character.RunAnimatorSanityChecks);
            if (_activating && (ButtonActivatedZone != null) && (ButtonActivatedZone.AnimationTriggerParameterName != ""))
            {
                _animator.SetTrigger(ButtonActivatedZone.AnimationTriggerParameterName);
            }
            _activating = false;
        }
	}
}

using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [MMHiddenProperties("AbilityStopFeedbacks")]
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Pause")]
    public class CharacterPause : CharacterAbility
    {
        public override string HelpBoxText() { return "Allows this character (and the player controlling it) to press the pause button to pause the game."; }
        protected override void HandleInput()
        {
            if (_inputManager.PauseButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                TriggerPause();
            }
        }
        protected virtual void TriggerPause()
        {
            if (_condition.CurrentState == CharacterStates.CharacterConditions.Dead)
            {
                return;
            }
            if (!AbilityAuthorized)
            {
                return;
            }
            PlayAbilityStartFeedbacks();
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
        }
        public virtual void PauseCharacter()
        {
            if (!this.enabled)
            {
                return;
            }
            _condition.ChangeState(CharacterStates.CharacterConditions.Paused);
        }
        public virtual void UnPauseCharacter()
        {
            if (!this.enabled)
            {
                return;
            }
            _condition.RestorePreviousState();
        }
    }
}
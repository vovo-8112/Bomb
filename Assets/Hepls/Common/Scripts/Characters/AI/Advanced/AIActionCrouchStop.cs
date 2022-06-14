using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionCrouchStop")]
    public class AIActionCrouchStop : AIAction
    {
        protected CharacterCrouch _characterCrouch;
        protected Character _character;
        protected override void Initialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterCrouch = _character?.FindAbility<CharacterCrouch>();
        }
        public override void PerformAction()
        {
            if ((_character == null) || (_characterCrouch == null))
            {
                return;
            }

            if ((_character.MovementState.CurrentState == CharacterStates.MovementStates.Crouching)
                || (_character.MovementState.CurrentState == CharacterStates.MovementStates.Crawling))
            {
                _characterCrouch.StopForcedCrouch();
            }
        }
    }
}

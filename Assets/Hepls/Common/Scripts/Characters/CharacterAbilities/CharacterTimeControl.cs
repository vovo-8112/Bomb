using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Time Control")]
    public class CharacterTimeControl : CharacterAbility
    {
        [Tooltip("the time scale to switch to when the time control button gets pressed")]
        public float TimeScale = 0.5f;
        [Tooltip("the duration for which to keep the timescale changed")]
        public float Duration = 1f;
        [Tooltip("whether or not the timescale should get lerped")]
        public bool LerpTimeScale = true;
        [Tooltip("the speed at which to lerp the timescale")]
        public float LerpSpeed = 5f;
        [Tooltip("the cooldown for this ability")]
        public MMCooldown Cooldown;

        protected bool _timeControlled = false;
        protected override void HandleInput()
        {
            base.HandleInput();
            if (!AbilityAuthorized)
            {
                return;
            }
            if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                TimeControlStart();
            }
            if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
            {
                TimeControlStop();
            }
        }
        protected override void Initialization()
        {
            base.Initialization();
            Cooldown.Initialization();
        }
        public virtual void TimeControlStart()
        {
            if (Cooldown.Ready())
            {
                PlayAbilityStartFeedbacks();
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, true);
                Cooldown.Start();
                _timeControlled = true;
            }            
        }
        public virtual void TimeControlStop()
        {
            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();
            Cooldown.Stop();
        }
        public override void ProcessAbility()
        {
            base.ProcessAbility();
            Cooldown.Update();

            if ((Cooldown.CooldownState != MMCooldown.CooldownStates.Consuming) && _timeControlled)
            {
                _timeControlled = false;
                MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            }
        }
    }
}

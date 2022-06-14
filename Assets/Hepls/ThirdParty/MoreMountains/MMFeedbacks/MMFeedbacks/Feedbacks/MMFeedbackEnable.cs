using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to change the state of a behaviour on a target gameobject from active to inactive (or the opposite), on init, play, stop or reset. " +
        "For each of these you can specify if you want to force a state (enabled or disabled), or toggle it (enabled becomes disabled, disabled becomes enabled).")]
    [FeedbackPath("GameObject/Enable Behaviour")]
    public class MMFeedbackEnable : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif
        public enum PossibleStates { Enabled, Disabled, Toggle }

        [Header("Set Active")]
        [Tooltip("the gameobject we want to change the active state of")]
        public Behaviour TargetBehaviour;
        [Tooltip("whether or not we should alter the state of the target object on init")]
        public bool SetStateOnInit = false;
        [MMFCondition("SetStateOnInit", true)]
        [Tooltip("how to change the state on init")]
        public PossibleStates StateOnInit = PossibleStates.Disabled;
        [Tooltip("whether or not we should alter the state of the target object on play")]
        public bool SetStateOnPlay = false;
        [MMFCondition("SetStateOnPlay", true)]
        [Tooltip("how to change the state on play")]
        public PossibleStates StateOnPlay = PossibleStates.Disabled;
        [Tooltip("whether or not we should alter the state of the target object on stop")]
        public bool SetStateOnStop = false;
        [Tooltip("how to change the state on stop")]
        [MMFCondition("SetStateOnStop", true)]
        public PossibleStates StateOnStop = PossibleStates.Disabled;
        [Tooltip("whether or not we should alter the state of the target object on reset")]
        public bool SetStateOnReset = false;
        [Tooltip("how to change the state on reset")]
        [MMFCondition("SetStateOnReset", true)]
        public PossibleStates StateOnReset = PossibleStates.Disabled;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active && (TargetBehaviour != null))
            {
                if (SetStateOnInit)
                {
                    SetStatus(StateOnInit);
                }
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetBehaviour != null))
            {
                if (SetStateOnPlay)
                {
                    SetStatus(StateOnPlay);
                }
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);

            if (Active && (TargetBehaviour != null))
            {
                if (SetStateOnStop)
                {
                    SetStatus(StateOnStop);
                }
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();

            if (InCooldown)
            {
                return;
            }

            if (Active && (TargetBehaviour != null))
            {
                if (SetStateOnReset)
                {
                    SetStatus(StateOnReset);
                }
            }
        }
        protected virtual void SetStatus(PossibleStates state)
        {
            switch (state)
            {
                case PossibleStates.Enabled:
                    TargetBehaviour.enabled = NormalPlayDirection ? true : false;
                    break;
                case PossibleStates.Disabled:
                    TargetBehaviour.enabled = NormalPlayDirection ? false : true;
                    break;
                case PossibleStates.Toggle:
                    TargetBehaviour.enabled = !TargetBehaviour.enabled;
                    break;
            }
        }
    }
}

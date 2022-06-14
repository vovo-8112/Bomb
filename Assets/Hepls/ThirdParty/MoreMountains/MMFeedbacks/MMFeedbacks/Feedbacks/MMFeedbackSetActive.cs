using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to change the state of the target gameobject from active to inactive (or the opposite), on init, play, stop or reset. For each of these you can specify if you want to force a state (active or inactive), or toggle it (active becomes inactive, inactive becomes active).")]
    [FeedbackPath("GameObject/Set Active")]
    public class MMFeedbackSetActive : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif
        public enum PossibleStates { Active, Inactive, Toggle }
        
        [Header("Set Active")]
        [Tooltip("the gameobject we want to change the active state of")]
        public GameObject TargetGameObject;
        
        [Header("States")]
        [Tooltip("whether or not we should alter the state of the target object on init")]
        public bool SetStateOnInit = false;
        [MMFCondition("SetStateOnInit", true)]
        [Tooltip("how to change the state on init")]
        public PossibleStates StateOnInit = PossibleStates.Inactive;
        [Tooltip("whether or not we should alter the state of the target object on play")]
        public bool SetStateOnPlay = false;
        [Tooltip("how to change the state on play")]
        [MMFCondition("SetStateOnPlay", true)]
        public PossibleStates StateOnPlay = PossibleStates.Inactive;
        [Tooltip("whether or not we should alter the state of the target object on stop")]
        public bool SetStateOnStop = false;
        [Tooltip("how to change the state on stop")]
        [MMFCondition("SetStateOnStop", true)]
        public PossibleStates StateOnStop = PossibleStates.Inactive;
        [Tooltip("whether or not we should alter the state of the target object on reset")]
        public bool SetStateOnReset = false;
        [Tooltip("how to change the state on reset")]
        [MMFCondition("SetStateOnReset", true)]
        public PossibleStates StateOnReset = PossibleStates.Inactive;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active && (TargetGameObject != null))
            {
                if (SetStateOnInit)
                {
                    SetStatus(StateOnInit);
                }
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetGameObject != null))
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

            if (Active && (TargetGameObject != null))
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

            if (Active && (TargetGameObject != null))
            {
                if (SetStateOnReset)
                {
                    SetStatus(StateOnReset);
                }
            }
        }
        protected virtual void SetStatus(PossibleStates state)
        {
            bool newState = false;
            switch (state)
            {
                case PossibleStates.Active:
                    newState = NormalPlayDirection ? true : false;
                    break;
                case PossibleStates.Inactive:
                    newState = NormalPlayDirection ? false : true;
                    break;
                case PossibleStates.Toggle:
                    newState = !TargetGameObject.activeInHierarchy;
                    break;
            }
            TargetGameObject.SetActive(newState);
        }
    }
}

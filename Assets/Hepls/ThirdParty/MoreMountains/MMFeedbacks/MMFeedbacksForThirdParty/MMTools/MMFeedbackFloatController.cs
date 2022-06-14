using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger a one time play on a target FloatController.")]
    [FeedbackPath("GameObject/FloatController")]
    public class MMFeedbackFloatController : MMFeedback
    {
        public enum Modes { OneTime, ToDestination }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif

        [Header("Float Controller")]
        [Tooltip("the mode this controller is in")]
        public Modes Mode = Modes.OneTime;
        [Tooltip("the float controller to trigger a one time play on")]
        public FloatController TargetFloatController;
        [Tooltip("whether this should revert to original at the end")]
        public bool RevertToInitialValueAfterEnd = false;
        [Tooltip("the duration of the One Time shake")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeDuration = 1f;
        [Tooltip("the amplitude of the One Time shake (this will be multiplied by the curve's height)")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeAmplitude = 1f;
        [Tooltip("the low value to remap the normalized curve value to")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeRemapMin = 0f;
        [Tooltip("the high value to remap the normalized curve value to")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public float OneTimeRemapMax = 1f;
        [Tooltip("the curve to apply to the one time shake")]
        [MMFEnumCondition("Mode", (int)Modes.OneTime)]
        public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to move this float controller to")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationValue = 1f;
        [Tooltip("the duration over which to move the value")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationDuration = 1f;
        [Tooltip("the curve over which to move the value in ToDestination mode")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        public override float FeedbackDuration
        {
            get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
            set { OneTimeDuration = value; ToDestinationDuration = value; }
        }

        protected float _oneTimeDurationStorage;
        protected float _oneTimeAmplitudeStorage;
        protected float _oneTimeRemapMinStorage;
        protected float _oneTimeRemapMaxStorage;
        protected AnimationCurve _oneTimeCurveStorage;
        protected float _toDestinationValueStorage;
        protected float _toDestinationDurationStorage;
        protected AnimationCurve _toDestinationCurveStorage;
        protected bool _revertToInitialValueAfterEndStorage;
        protected override void CustomInitialization(GameObject owner)
        {
            if (Active && (TargetFloatController != null))
            {
                _oneTimeDurationStorage = TargetFloatController.OneTimeDuration;
                _oneTimeAmplitudeStorage = TargetFloatController.OneTimeAmplitude;
                _oneTimeCurveStorage = TargetFloatController.OneTimeCurve;
                _oneTimeRemapMinStorage = TargetFloatController.OneTimeRemapMin;
                _oneTimeRemapMaxStorage = TargetFloatController.OneTimeRemapMax;
                _toDestinationCurveStorage = TargetFloatController.ToDestinationCurve;
                _toDestinationDurationStorage = TargetFloatController.ToDestinationDuration;
                _toDestinationValueStorage = TargetFloatController.ToDestinationValue;
                _revertToInitialValueAfterEndStorage = TargetFloatController.RevertToInitialValueAfterEnd;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetFloatController != null))
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                TargetFloatController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;

                if (Mode == Modes.OneTime)
                {
                    TargetFloatController.OneTimeDuration = FeedbackDuration;
                    TargetFloatController.OneTimeAmplitude = OneTimeAmplitude;
                    TargetFloatController.OneTimeCurve = OneTimeCurve;
                    if (NormalPlayDirection)
                    {
                        TargetFloatController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
                        TargetFloatController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;
                    }
                    else
                    {
                        TargetFloatController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
                        TargetFloatController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;   
                    }
                    TargetFloatController.OneTime();
                }
                if (Mode == Modes.ToDestination)
                {
                    TargetFloatController.ToDestinationCurve = ToDestinationCurve;
                    TargetFloatController.ToDestinationDuration = FeedbackDuration;
                    TargetFloatController.ToDestinationValue = ToDestinationValue;
                    TargetFloatController.ToDestination();
                }
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();
            if (Active && (TargetFloatController != null))
            {
                TargetFloatController.OneTimeDuration = _oneTimeDurationStorage;
                TargetFloatController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
                TargetFloatController.OneTimeCurve = _oneTimeCurveStorage;
                TargetFloatController.OneTimeRemapMin = _oneTimeRemapMinStorage;
                TargetFloatController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
                TargetFloatController.ToDestinationCurve = _toDestinationCurveStorage;
                TargetFloatController.ToDestinationDuration = _toDestinationDurationStorage;
                TargetFloatController.ToDestinationValue = _toDestinationValueStorage;
                TargetFloatController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                if (TargetFloatController != null)
                {
                    TargetFloatController.Stop();
                }
            }
        }
    }
}

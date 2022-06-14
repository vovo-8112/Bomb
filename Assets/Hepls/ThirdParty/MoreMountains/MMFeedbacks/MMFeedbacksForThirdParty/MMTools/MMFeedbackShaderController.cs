using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback lets you trigger a one time play on a target ShaderController.")]
    [FeedbackPath("Renderer/ShaderController")]
    public class MMFeedbackShaderController : MMFeedback
    {
        public enum Modes { OneTime, ToDestination }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
        #endif

        [Header("Float Controller")]
        [Tooltip("the mode this controller is in")]
        public Modes Mode = Modes.OneTime;
        [Tooltip("the float controller to trigger a one time play on")]
        public ShaderController TargetShaderController;
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
        [Tooltip("the new value towards which to move the current value")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationValue = 1f;
        [Tooltip("the duration over which to interpolate the target value")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public float ToDestinationDuration = 1f;
        [Tooltip("the color to aim for (when targetting a Color property")]
        [MMFEnumCondition("Mode", (int)Modes.ToDestination)]
        public Color ToDestinationColor = Color.red;
        [Tooltip("the curve over which to interpolate the value")]
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
            if (Active && (TargetShaderController != null))
            {
                _oneTimeDurationStorage = TargetShaderController.OneTimeDuration;
                _oneTimeAmplitudeStorage = TargetShaderController.OneTimeAmplitude;
                _oneTimeCurveStorage = TargetShaderController.OneTimeCurve;
                _oneTimeRemapMinStorage = TargetShaderController.OneTimeRemapMin;
                _oneTimeRemapMaxStorage = TargetShaderController.OneTimeRemapMax;
                _toDestinationCurveStorage = TargetShaderController.ToDestinationCurve;
                _toDestinationDurationStorage = TargetShaderController.ToDestinationDuration;
                _toDestinationValueStorage = TargetShaderController.ToDestinationValue;
                _revertToInitialValueAfterEndStorage = TargetShaderController.RevertToInitialValueAfterEnd;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetShaderController != null))
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                
                TargetShaderController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;
                if (Mode == Modes.OneTime)
                {
                    TargetShaderController.OneTimeDuration = FeedbackDuration;
                    TargetShaderController.OneTimeAmplitude = OneTimeAmplitude;
                    TargetShaderController.OneTimeCurve = OneTimeCurve;
                    if (NormalPlayDirection)
                    {
                        TargetShaderController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
                        TargetShaderController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;    
                    }
                    else
                    {
                        TargetShaderController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
                        TargetShaderController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;
                    }
                    TargetShaderController.OneTime();
                }
                if (Mode == Modes.ToDestination)
                {
                    TargetShaderController.ToColor = ToDestinationColor;
                    TargetShaderController.ToDestinationCurve = ToDestinationCurve;
                    TargetShaderController.ToDestinationDuration = FeedbackDuration;
                    TargetShaderController.ToDestinationValue = ToDestinationValue;
                    TargetShaderController.ToDestination();
                }                
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                if (TargetShaderController != null)
                {
                    TargetShaderController.Stop();
                }
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();
            if (Active && (TargetShaderController != null))
            {
                TargetShaderController.OneTimeDuration = _oneTimeDurationStorage;
                TargetShaderController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
                TargetShaderController.OneTimeCurve = _oneTimeCurveStorage;
                TargetShaderController.OneTimeRemapMin = _oneTimeRemapMinStorage;
                TargetShaderController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
                TargetShaderController.ToDestinationCurve = _toDestinationCurveStorage;
                TargetShaderController.ToDestinationDuration = _toDestinationDurationStorage;
                TargetShaderController.ToDestinationValue = _toDestinationValueStorage;
                TargetShaderController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
            }
        }

    }
}

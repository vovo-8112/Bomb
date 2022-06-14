using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to control bloom intensity and threshold over time. It requires you have in your scene an object with a PostProcessVolume " +
            "with Bloom active, and a MMBloomShaker component.")]
    [FeedbackPath("PostProcess/Bloom")]
    public class MMFeedbackBloom : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Bloom")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the feedback, in seconds")]
        public float ShakeDuration = 0.2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;
        [Tooltip("whether or not to add to the initial intensity")]
        public bool RelativeValues = true;

        [Header("Intensity")]
        [Tooltip("the curve to animate the intensity on")]
        public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapIntensityOne = 1f;

        [Header("Threshold")]
        [Tooltip("the curve to animate the threshold on")]
        public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapThresholdZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapThresholdOne = 0f;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMBloomShakeEvent.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
                    RelativeValues, intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMBloomShakeEvent.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
                    RelativeValues, stop:true);
            }
        }
    }
}

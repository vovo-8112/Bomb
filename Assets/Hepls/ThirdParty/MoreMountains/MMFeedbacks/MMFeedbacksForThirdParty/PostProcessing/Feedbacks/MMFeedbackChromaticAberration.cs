using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackPath("PostProcess/Chromatic Aberration")]
    [FeedbackHelp("This feedback allows you to control chromatic aberration intensity over time. It requires you have in your scene an object with a PostProcessVolume " +
            "with Chromatic Aberration active, and a MMChromaticAberrationShaker component.")]
    public class MMFeedbackChromaticAberration : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Chromatic Aberration")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 0.2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 1f)]
        public float RemapIntensityZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 1f)]
        public float RemapIntensityOne = 1f;

        [Header("Intensity")]
        [Tooltip("the curve to animate the intensity on")]
        public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the multiplier to apply to the intensity curve")]
        [Range(0f, 1f)]
        public float Amplitude = 1.0f;
        [Tooltip("whether or not to add to the initial intensity")]
        public bool RelativeIntensity = false;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMChromaticAberrationShakeEvent.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
                    Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMChromaticAberrationShakeEvent.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, stop:true);
            }
        }
    }
}

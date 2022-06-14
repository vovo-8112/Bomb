using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackPath("PostProcess/Color Grading")]
    [FeedbackHelp("This feedback allows you to control color grading post exposure, hue shift, saturation and contrast over time. " +
            "It requires you have in your scene an object with a PostProcessVolume " +
            "with Color Grading active, and a MMColorGradingShaker component.")]
    public class MMFeedbackColorGrading : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Color Grading")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float ShakeDuration = 1f;
        [Tooltip("whether or not to add to the initial intensity")]
        public bool RelativeIntensity = true;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Post Exposure")]
        [Tooltip("the curve used to animate the focus distance value on")]
        public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapPostExposureZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapPostExposureOne = 1f;

        [Header("Hue Shift")]
        [Tooltip("the curve used to animate the aperture value on")]
        public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-180f, 180f)]
        public float RemapHueShiftZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-180f, 180f)]
        public float RemapHueShiftOne = 180f;

        [Header("Saturation")]
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-100f, 100f)]
        public float RemapSaturationZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-100f, 100f)]
        public float RemapSaturationOne = 100f;

        [Header("Contrast")]
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(-100f, 100f)]
        public float RemapContrastZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(-100f, 100f)]
        public float RemapContrastOne = 100f;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
                    ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
                    ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
                    ShakeContrast, RemapContrastZero, RemapContrastOne, 
                    FeedbackDuration,                     
                    RelativeIntensity, intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
                    ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
                    ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
                    ShakeContrast, RemapContrastZero, RemapContrastOne, 
                    FeedbackDuration,                     
                    stop:true);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to control depth of field focus distance, aperture and focal length over time. " +
            "It requires you have in your scene an object with a PostProcessVolume " +
            "with Depth of Field active, and a MMDepthOfFieldShaker component.")]
    [FeedbackPath("PostProcess/Depth Of Field")]
    public class MMFeedbackDepthOfField : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
        #endif

        [Header("Depth Of Field")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float ShakeDuration = 2f;
        [Tooltip("whether or not to add to the initial values")]
        public bool RelativeValues = true;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Focus Distance")]
        [Tooltip("the curve used to animate the focus distance value on")]
        public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapFocusDistanceZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapFocusDistanceOne = 3f;

        [Header("Aperture")]
        [Tooltip("the curve used to animate the aperture value on")]
        public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0.1f, 32f)]
        public float RemapApertureZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0.1f, 32f)]
        public float RemapApertureOne = 0f;

        [Header("Focal Length")]
        [Tooltip("the curve used to animate the focal length value on")]
        public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Tooltip("the value to remap the curve's 0 to")]
        [Range(0f, 300f)]
        public float RemapFocalLengthZero = 0f;
        [Tooltip("the value to remap the curve's 1 to")]
        [Range(0f, 300f)]
        public float RemapFocalLengthOne = 0f;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value;  } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMDepthOfFieldShakeEvent.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
                    ShakeAperture, RemapApertureZero, RemapApertureOne,
                    ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
                    RelativeValues, intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMDepthOfFieldShakeEvent.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
                    ShakeAperture, RemapApertureZero, RemapApertureOne,
                    ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
                    RelativeValues, stop:true);
            }
        }
    }
}

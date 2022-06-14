using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Audio Filter Low Pass")]
    [FeedbackHelp("This feedback lets you control a low pass audio filter over time. You'll need a MMAudioFilterLowPassShaker on your filter.")]
    public class MMFeedbackAudioFilterLowPass : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Low Pass Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Low Pass")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeLowPass = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeLowPass = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
        [Range(10f, 22000f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapLowPassZero = 0f;
        [Range(10f, 22000f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapLowPassOne = 10000f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioFilterLowPassShakeEvent.Trigger(ShakeLowPass, FeedbackDuration, RemapLowPassZero, RemapLowPassOne, RelativeLowPass,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioFilterLowPassShakeEvent.Trigger(ShakeLowPass, FeedbackDuration, RemapLowPassZero, RemapLowPassOne, stop:true);
            }
        }
    }
}

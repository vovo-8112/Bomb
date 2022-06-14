using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Audio Filter High Pass")]
    [FeedbackHelp("This feedback lets you control a high pass audio filter over time. You'll need a MMAudioFilterHighPassShaker on your filter.")]
    public class MMFeedbackAudioFilterHighPass : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("High Pass Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("High Pass")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeHighPass = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeHighPass = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        [Range(10f, 22000f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapHighPassZero = 0f;
        [Range(10f, 22000f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapHighPassOne = 10000f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioFilterHighPassShakeEvent.Trigger(ShakeHighPass, FeedbackDuration, RemapHighPassZero, RemapHighPassOne, RelativeHighPass,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioFilterHighPassShakeEvent.Trigger(ShakeHighPass, FeedbackDuration, RemapHighPassZero, RemapHighPassOne, stop:true);
            }
        }
    }
}

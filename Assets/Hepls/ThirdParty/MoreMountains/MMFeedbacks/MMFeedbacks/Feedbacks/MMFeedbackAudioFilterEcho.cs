using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Audio Filter Echo")]
    [FeedbackHelp("This feedback lets you control an echo audio filter's wet mix value over time. You'll need a MMAudioFilterEchoShaker on your filter.")]
    public class MMFeedbackAudioFilterEcho : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Echo Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Echo")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeEcho = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeEcho = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        [Range(0f, 1f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapEchoZero = 0f;
        [Range(0f, 1f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapEchoOne = 1f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioFilterEchoShakeEvent.Trigger(ShakeEcho, FeedbackDuration, RemapEchoZero, RemapEchoOne, RelativeEcho,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioFilterEchoShakeEvent.Trigger(ShakeEcho, FeedbackDuration, RemapEchoZero, RemapEchoOne, RelativeEcho, stop:true);
            }
        }
    }
}

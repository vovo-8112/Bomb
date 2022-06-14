using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/AudioSource Stereo Pan")]
    [FeedbackHelp("This feedback lets you control the stereo pan of a target AudioSource over time.")]
    public class MMFeedbackAudioSourceStereoPan : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("StereoPan Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("StereoPan")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeStereoPan = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeStereoPan = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
        [Range(-1f, 1f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapStereoPanZero = 0f;
        [Range(-1f, 1f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapStereoPanOne = 1f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, RelativeStereoPan,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, stop:true);
            }
        }
    }
}

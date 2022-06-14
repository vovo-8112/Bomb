using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/AudioSource Pitch")]
    [FeedbackHelp("This feedback lets you control the pitch of a target AudioSource over time.")]
    public class MMFeedbackAudioSourcePitch : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Pitch Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Pitch")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativePitch = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve PitchTween = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(0.5f, 0f), new Keyframe(1, 1f));
        [Range(-3f, 3f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapPitchZero = 0f;
        [Range(-3f, 3f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapPitchOne = 1f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioSourcePitchShakeEvent.Trigger(PitchTween, FeedbackDuration, RemapPitchZero, RemapPitchOne, RelativePitch,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioSourcePitchShakeEvent.Trigger(PitchTween, FeedbackDuration, RemapPitchZero, RemapPitchOne, RelativePitch, stop:true);
            }
        }
    }
}

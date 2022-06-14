﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/Audio Filter Reverb")]
    [FeedbackHelp("This feedback lets you control a low pass audio filter over time. You'll need a MMAudioFilterReverbShaker on your filter.")]
    public class MMFeedbackAudioFilterReverb : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

        [Header("Reverb Feedback")]
        [Tooltip("the channel to emit on")]
        public int Channel = 0;
        [Tooltip("the duration of the shake, in seconds")]
        public float Duration = 2f;
        [Tooltip("whether or not to reset shaker values after shake")]
        public bool ResetShakerValuesAfterShake = true;
        [Tooltip("whether or not to reset the target's values after shake")]
        public bool ResetTargetValuesAfterShake = true;

        [Header("Reverb")]
        [Tooltip("whether or not to add to the initial value")]
        public bool RelativeReverb = false;
        [Tooltip("the curve used to animate the intensity value on")]
        public AnimationCurve ShakeReverb = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
        [Range(-10000f, 2000f)]
        [Tooltip("the value to remap the curve's 0 to")]
        public float RemapReverbZero = -10000f;
        [Range(-10000f, 2000f)]
        [Tooltip("the value to remap the curve's 1 to")]
        public float RemapReverbOne = 2000f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                MMAudioFilterReverbShakeEvent.Trigger(ShakeReverb, FeedbackDuration, RemapReverbZero, RemapReverbOne, RelativeReverb,
                    intensityMultiplier, Channel, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            if (Active)
            {
                MMAudioFilterReverbShakeEvent.Trigger(ShakeReverb, FeedbackDuration, RemapReverbZero, RemapReverbOne, stop:true);
            }
        }
    }
}

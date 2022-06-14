using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackPath("Audio/MMSoundManager Track Fade")]
    [FeedbackHelp("This feedback will let you fade all the sounds on a specific track at once. You will need a MMSoundManager in your scene for this to work.")]
    public class MMFeedbackMMSoundManagerTrackFade : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif
        public override float FeedbackDuration { get { return FadeDuration; } }
        
        [Header("MMSoundManager Track Fade")]
        [Tooltip("the track to fade the volume on")]
        public MMSoundManager.MMSoundManagerTracks Track;
        [Tooltip("the duration of the fade, in seconds")]
        public float FadeDuration = 1f;
        [Tooltip("the volume to reach at the end of the fade")]
        [Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
        public float FinalVolume = MMSoundManagerSettings._minimalVolume;
        [Tooltip("the tween to operate the fade on")]
        public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMSoundManagerTrackFadeEvent.Trigger(Track, FadeDuration, FinalVolume, FadeTween);
            }
        }
    }
}

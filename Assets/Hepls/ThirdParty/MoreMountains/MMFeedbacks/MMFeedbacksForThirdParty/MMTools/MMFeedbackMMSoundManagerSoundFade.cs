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
    [FeedbackPath("Audio/MMSoundManager Sound Fade")]
    [FeedbackHelp("This feedback lets you trigger fades on a specific sound via the MMSoundManager. You will need a MMSoundManager in your scene for this to work.")]
    public class MMFeedbackMMSoundManagerSoundFade : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif

        [Header("MMSoundManager Sound Fade")]
        [Tooltip("the ID of the sound you want to fade. Has to match the ID you specified when playing the sound initially")]
        public int SoundID = 0;
        [Tooltip("the duration of the fade, in seconds")]
        public float FadeDuration = 1f;
        [Tooltip("the volume towards which to fade")]
        [Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
        public float FinalVolume = MMSoundManagerSettings._minimalVolume;
        [Tooltip("the tween to apply over the fade")]
        public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
        protected AudioSource _targetAudioSource;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMSoundManagerSoundFadeEvent.Trigger(SoundID, FadeDuration, FinalVolume, FadeTween);
            }
        }
    }
}

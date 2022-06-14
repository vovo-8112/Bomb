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
    [FeedbackPath("Audio/MMSoundManager Sound Control")]
    [FeedbackHelp("This feedback will let you control a specific sound (or sounds), targeted by SoundID, which has to match the SoundID of the sound you intially played. You will need a MMSoundManager in your scene for this to work.")]
    public class MMFeedbackMMSoundManagerSoundControl : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        #endif

        [Header("MMSoundManager Sound Control")]
        [Tooltip("the action to trigger on the specified sound")]
        public MMSoundManagerSoundControlEventTypes ControlMode = MMSoundManagerSoundControlEventTypes.Pause;
        [Tooltip("the ID of the sound, has to match the one you specified when playing it")]
        public int SoundID = 0;

        protected AudioSource _targetAudioSource;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMSoundManagerSoundControlEvent.Trigger(ControlMode, SoundID);
            }
        }
    }
}

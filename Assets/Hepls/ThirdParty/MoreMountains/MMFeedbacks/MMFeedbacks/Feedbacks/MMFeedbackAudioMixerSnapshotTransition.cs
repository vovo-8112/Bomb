using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you transition to a target AudioMixer Snapshot over a specified time")]
    [FeedbackPath("Audio/AudioMixer Snapshot Transition")]
    public class MMFeedbackAudioMixerSnapshotTransition : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
        #endif
        
        [Header("AudioMixer Snapshot")]
        [Tooltip("the target audio mixer snapshot we want to transition to")]
        public AudioMixerSnapshot TargetSnapshot;
        [Tooltip("the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse")]
        public AudioMixerSnapshot OriginalSnapshot;
        [Tooltip("the duration, in seconds, over which to transition to the selected snapshot")]
        public float TransitionDuration = 1f;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (TargetSnapshot == null)
            {
                return;
            }

            if (!NormalPlayDirection)
            {
                if (OriginalSnapshot != null)
                {
                    OriginalSnapshot.TransitionTo(TransitionDuration);     
                }
                else
                {
                    TargetSnapshot.TransitionTo(TransitionDuration);
                }
            }
            else
            {
                TargetSnapshot.TransitionTo(TransitionDuration);     
            }
        }
    }
}

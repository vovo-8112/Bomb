using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will freeze the timescale for the specified duration (in seconds). I usually go with 0.01s or 0.02s, but feel free to tweak it to your liking. It requires a MMTimeManager in your scene to work.")]
    [FeedbackPath("Time/Freeze Frame")]
    public class MMFeedbackFreezeFrame : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
        #endif

        [Header("Freeze Frame")]
        [Tooltip("the duration of the freeze frame")]
        public float FreezeFrameDuration = 0.02f;
        [Tooltip("the minimum value the timescale should be at for this freeze frame to happen. This can be useful to avoid triggering freeze frames when the timescale is already frozen.")]
        public float MinimumTimescaleThreshold = 0.1f;
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FreezeFrameDuration); } set { FreezeFrameDuration = value; } }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                if (Time.timeScale < MinimumTimescaleThreshold)
                {
                    return;
                }
                
                MMFreezeFrameEvent.Trigger(FeedbackDuration);
            }
        }
    }
}

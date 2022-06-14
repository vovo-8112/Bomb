using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will move the current 'head' of an MMFeedbacks sequence back to another feedback above in the list. " +
        "What feedback the head lands on depends on your settings : you can decide to have it loop at last pause, " +
        "or at the last LoopStart feedback in the list (or both). Furthermore, you can decide to have it loop multiple times and cause a pause when met.")]
    [FeedbackPath("Loop/Looper")]
    public class MMFeedbackLooper : MMFeedbackPause
    {
        [Header("Loop conditions")]
        [Tooltip("if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first pause found above it (going from this feedback to the top), or to the start if none is found")]
        public bool LoopAtLastPause = true;
        [Tooltip("if this is true, this feedback, when met, will cause the MMFeedbacks to reposition its 'head' to the first LoopStart feedback found above it (going from this feedback to the top), or to the start if none is found")]
        public bool LoopAtLastLoopStart = true;

        [Header("Loop")]
        [Tooltip("if this is true, the looper will loop forever")]
        public bool InfiniteLoop = false;
        [Tooltip("how many times this loop should run")]
        public int NumberOfLoops = 2;
        [Tooltip("the amount of loops left (updated at runtime)")]
        [MMFReadOnly]
        public int NumberOfLoopsLeft = 1;
        [Tooltip("whether we are in an infinite loop at this time or not")]
        [MMFReadOnly]
        public bool InInfiniteLoop = false;
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperColor; } }
        #endif
        public override bool LooperPause { get { return true; } }
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            InInfiniteLoop = InfiniteLoop;
            NumberOfLoopsLeft = NumberOfLoops;
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                NumberOfLoopsLeft--;
                StartCoroutine(PlayPause());
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
        {
            base.CustomStopFeedback(position, feedbacksIntensity);
            InInfiniteLoop = false;
        }
        protected override void CustomReset()
        {
            base.CustomReset();
            InInfiniteLoop = InfiniteLoop;
            NumberOfLoopsLeft = NumberOfLoops;
        }
    }
}

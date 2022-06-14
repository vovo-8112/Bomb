using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to bind any type of Unity events to this feebdack's Play, Stop, Initialization and Reset methods.")]
    [FeedbackPath("Events/Events")]
    public class MMFeedbackEvents : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
        #endif

        [Header("Events")]
        [Tooltip("the events to trigger when the feedback is played")]
        public UnityEvent PlayEvents;
        [Tooltip("the events to trigger when the feedback is stopped")]
        public UnityEvent StopEvents;
        [Tooltip("the events to trigger when the feedback is initialized")]
        public UnityEvent InitializationEvents;
        [Tooltip("the events to trigger when the feedback is reset")]
        public UnityEvent ResetEvents;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if (Active && (InitializationEvents != null))
            {
                InitializationEvents.Invoke();
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (PlayEvents != null))
            {
                PlayEvents.Invoke();                
            }
        }
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (StopEvents != null))
            {
                StopEvents.Invoke();
            }
        }
        protected override void CustomReset()
        {
            base.CustomReset();
            if (Active && (ResetEvents != null))
            {
                ResetEvents.Invoke();
            }
        }
    }
}

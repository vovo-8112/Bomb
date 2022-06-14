using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to trigger any MMFeedbacks on the specified Channel within a certain range. You'll need an MMFeedbacksShaker on them.")]
    [FeedbackPath("GameObject/MMFeedbacks")]
    public class MMFeedbackFeedbacks : MMFeedback
    {
        public override float FeedbackDuration { get { return 0f; } }
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif

        [Header("MMFeedbacks")]
        [Tooltip("the channel to broadcast on")]
        public int Channel = 0;
        [Tooltip("whether or not to use a range")]
        public bool UseRange = false;
        [Tooltip("the range of the event, in units")]
        public float EventRange = 100f;
        [Tooltip("the transform to use to broadcast the event as origin point")]
        public Transform EventOriginTransform;
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            
            if (EventOriginTransform == null)
            {
                EventOriginTransform = this.transform;
            }
        }
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active)
            {
                MMFeedbacksShakeEvent.Trigger(Channel, UseRange, EventRange, EventOriginTransform.position);
            }
        }
    }
}

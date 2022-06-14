using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you control the RaycastTarget parameter of a target image, turning it on or off on play")]
    [FeedbackPath("UI/Image RaycastTarget")]
    public class MMFeedbackImageRaycastTarget : MMFeedback
    {
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
        #endif
        
        [Header("Image")]
        [Tooltip("the target Image we want to control the RaycastTarget parameter on")]
        public Image TargetImage;
        [Tooltip("if this is true, when played, the target image will become a raycast target")]
        public bool ShouldBeRaycastTarget = true;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (TargetImage == null)
            {
                return;
            }

            TargetImage.raycastTarget = NormalPlayDirection ? ShouldBeRaycastTarget : !ShouldBeRaycastTarget;
        }
    }
}

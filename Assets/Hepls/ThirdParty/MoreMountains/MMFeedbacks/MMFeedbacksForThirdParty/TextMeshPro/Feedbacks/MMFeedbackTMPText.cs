using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback will let you change the text of a target TMP text component")]
    [FeedbackPath("TextMesh Pro/TMP Text")]
    public class MMFeedbackTMPText : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
        #endif
        
        [Header("TextMesh Pro")]
        [Tooltip("the target TMP_Text component we want to change the text on")]
        public TMP_Text TargetTMPText;
        [Tooltip("the new text to replace the old one with")]
        [TextArea]
        public string NewText = "Hello World";
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active)
            {
                return;
            }

            if (TargetTMPText == null)
            {
                return;
            }

            TargetTMPText.text = NewText;
        }
    }
}

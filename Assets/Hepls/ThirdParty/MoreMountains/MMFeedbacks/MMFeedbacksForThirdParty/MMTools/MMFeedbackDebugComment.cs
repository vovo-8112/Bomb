using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback doesn't do anything by default, it's just meant as a comment, you can store text in it for future reference, maybe to remember how you setup a particular MMFeedbacks. Optionnally it can also output that comment to the console on Play.")]
    [FeedbackPath("Debug/Comment")]
    public class MMFeedbackDebugComment : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
        #endif
        [Tooltip("the comment / note associated to this feedback")]
        [TextArea(10,30)] 
        public string Comment;
        [Tooltip("if this is true, the comment will be output to the console on Play")]
        public bool LogComment = false;
        [Tooltip("the color of the message when in DebugLogTime mode")]
        [MMCondition("LogComment", true)]
        public Color DebugColor = Color.gray;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && LogComment)
            {
                Debug.Log(Comment);
            }
        }
    }
}

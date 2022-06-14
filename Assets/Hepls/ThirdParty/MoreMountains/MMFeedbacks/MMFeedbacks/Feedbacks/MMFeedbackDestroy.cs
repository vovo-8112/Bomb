using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    [FeedbackHelp("This feedback allows you to destroy a target gameobject, either via Destroy, DestroyImmediate, or SetActive:False")]
    [FeedbackPath("GameObject/Destroy")]
    public class MMFeedbackDestroy : MMFeedback
    {
        #if UNITY_EDITOR
            public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
        #endif
        public enum Modes { Destroy, DestroyImmediate, Disable }

        [Header("Destroy")]
        [Tooltip("the gameobject we want to change the active state of")]
        public GameObject TargetGameObject;
        [Tooltip("the selected destruction mode")]
        public Modes Mode;
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (Active && (TargetGameObject != null))
            {
                ProceedWithDestruction(TargetGameObject);
            }
        }
        protected virtual void ProceedWithDestruction(GameObject go)
        {
            switch (Mode)
            {
                case Modes.Destroy:
                    Destroy(go);
                    break;
                case Modes.DestroyImmediate:
                    DestroyImmediate(go);
                    break;
                case Modes.Disable:
                    go.SetActive(false);
                    break;
            }
        }
    }
}

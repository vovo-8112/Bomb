using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    [AddComponentMenu("")]
    public class MMFeedbacksEnabler : MonoBehaviour
    {
        public MMFeedbacks TargetMMFeedbacks { get; set; }
        protected virtual void OnEnable()
        {
            if ((TargetMMFeedbacks != null) && !TargetMMFeedbacks.enabled && TargetMMFeedbacks.AutoPlayOnEnable)
            {
                TargetMMFeedbacks.enabled = true;
            }
        }
    }    
}


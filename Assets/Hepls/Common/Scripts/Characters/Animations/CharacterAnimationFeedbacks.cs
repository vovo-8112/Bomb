using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class CharacterAnimationFeedbacks : MonoBehaviour
    {
        [Tooltip("a feedback that will play every time a foot touches the ground while walking")]
        public MMFeedbacks WalkFeedbacks;
        [Tooltip("a feedback that will play every time a foot touches the ground while running")]
        public MMFeedbacks RunFeedbacks;
        public virtual void WalkStep()
        {
            WalkFeedbacks?.PlayFeedbacks();
        }
        public virtual void RunStep()
        {
            RunFeedbacks?.PlayFeedbacks();
        }
    }
}

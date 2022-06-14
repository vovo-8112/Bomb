using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMMFeedbacks")]
    public class AIActionMMFeedbacks : AIAction
    {
        [Tooltip("The MMFeedbacks to play when this action gets performed by the AIBrain")]
        public MMFeedbacks TargetFeedbacks;
        [Tooltip("If this is false, the feedback will be played every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state")]
        public bool OnlyPlayWhenEnteringState = true;

        protected bool _played = false;
        public override void PerformAction()
        {
            PlayFeedbacks();
        }
        protected virtual void PlayFeedbacks()
        {
            if (OnlyPlayWhenEnteringState && _played)
            {
                return;
            }

            if (TargetFeedbacks != null)
            {
                TargetFeedbacks.PlayFeedbacks();
                _played = true;
            }
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _played = false;
        }
    }
}

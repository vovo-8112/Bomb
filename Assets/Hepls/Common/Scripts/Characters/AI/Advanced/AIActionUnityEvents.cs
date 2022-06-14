using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionUnityEvents")]
    public class AIActionUnityEvents : AIAction
    {
        [Tooltip("The UnityEvent to trigger when this action gets performed by the AIBrain")]
        public UnityEvent TargetEvent;
        [Tooltip("If this is false, the Unity Event will be triggered every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state")]
        public bool OnlyPlayWhenEnteringState = true;

        protected bool _played = false;
        public override void PerformAction()
        {
            TriggerEvent();
        }
        protected virtual void TriggerEvent()
        {
            if (OnlyPlayWhenEnteringState && _played)
            {
                return;
            }

            if (TargetEvent != null)
            {
                TargetEvent.Invoke();
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

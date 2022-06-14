using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionGrounded")]
    public class AIDecisionGrounded : AIDecision
    {
        [Tooltip("The duration, in seconds, after entering the state this Decision is in during which we'll ignore being grounded")]
        public float GroundedBufferDelay = 0.2f;

        protected TopDownController _topDownController;
        protected float _startTime = 0f;
        public override void Initialization()
        {
            _topDownController = this.gameObject.GetComponentInParent<TopDownController>();
        }
        public override bool Decide()
        {
            return EvaluateGrounded();
        }
        protected virtual bool EvaluateGrounded()
        {
            if (Time.time - _startTime < GroundedBufferDelay)
            {
                return false;
            }
            return (_topDownController.Grounded);
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _startTime = Time.time;
        }
    }
}

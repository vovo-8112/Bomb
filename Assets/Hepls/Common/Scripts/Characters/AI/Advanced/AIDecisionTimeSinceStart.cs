using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTimeSinceStart")]
    public class AIDecisionTimeSinceStart : AIDecision
    {
        [Tooltip("The duration (in seconds) after which to return true")]
        public float AfterTime;

        protected float _startTime;
        public override void Initialization()
        {
            _startTime = Time.time;
        }
        public override bool Decide()
        {
            return EvaluateTime();
        }
        protected virtual bool EvaluateTime()
        {
            return (Time.time - _startTime >= AfterTime);
        }
    }
}

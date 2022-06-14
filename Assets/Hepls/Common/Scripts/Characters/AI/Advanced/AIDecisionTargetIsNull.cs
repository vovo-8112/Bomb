using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetIsNull")]
    public class AIDecisionTargetIsNull : AIDecision
    {
        public override bool Decide()
        {
            return CheckIfTargetIsNull();
        }
        protected virtual bool CheckIfTargetIsNull()
        {
            if (_brain.Target == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

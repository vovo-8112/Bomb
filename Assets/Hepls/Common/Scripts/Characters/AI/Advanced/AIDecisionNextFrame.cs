using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionNextFrame")]
    public class AIDecisionNextFrame : AIDecision
    {
        public override bool Decide()
        {
            return true;
        }
    }
}

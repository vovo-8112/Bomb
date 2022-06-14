using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetConeOfVision2D")]
    [RequireComponent(typeof(MMConeOfVision2D))]
    public class AIDecisionDetectTargetConeOfVision2D : AIDecision
    {
        protected MMConeOfVision2D _coneOfVision;
        public override void Initialization()
        {
            base.Initialization();
            _coneOfVision = this.gameObject.GetComponent<MMConeOfVision2D>();
        }
        public override bool Decide()
        {
            return DetectTarget();
        }
        protected virtual bool DetectTarget()
        {
            if (_coneOfVision.VisibleTargets.Count == 0)
            {
                _brain.Target = null;
                return false;
            }
            else
            {
                _brain.Target = _coneOfVision.VisibleTargets[0];
                return true;
            }
        }
    }
}

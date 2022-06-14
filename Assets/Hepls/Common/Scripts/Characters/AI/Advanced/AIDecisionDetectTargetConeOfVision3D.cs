using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetConeOfVision3D")]
    [RequireComponent(typeof(MMConeOfVision))]
    public class AIDecisionDetectTargetConeOfVision3D : AIDecision
    {
        public bool SetTargetToNullIfNotFound = true;

        protected MMConeOfVision _coneOfVision;
        public override void Initialization()
        {
            base.Initialization();
            _coneOfVision = this.gameObject.GetComponent<MMConeOfVision>();
        }
        public override bool Decide()
        {
            return DetectTarget();
        }
        protected virtual bool DetectTarget()
        {
            if (_coneOfVision.VisibleTargets.Count == 0)
            {
                if (SetTargetToNullIfNotFound)
                {
                    _brain.Target = null;
                }                
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

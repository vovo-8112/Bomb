using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetFacingAI2D")]
    public class AIDecisionTargetFacingAI2D : AIDecision
    {
        protected CharacterOrientation2D _orientation2D;
        public override bool Decide()
        {
            return EvaluateTargetFacingDirection();
        }
        protected virtual bool EvaluateTargetFacingDirection()
        {
            if (_brain.Target == null)
            {
                return false;
            }

            _orientation2D = _brain.Target.gameObject.GetComponent<Character>()?.FindAbility<CharacterOrientation2D>();
            if (_orientation2D != null)
            {
                if (_orientation2D.IsFacingRight && (this.transform.position.x > _orientation2D.transform.position.x))
                {
                    return true;
                }
                if (!_orientation2D.IsFacingRight && (this.transform.position.x < _orientation2D.transform.position.x))
                {
                    return true;
                }
            }            

            return false;
        }
    }
}

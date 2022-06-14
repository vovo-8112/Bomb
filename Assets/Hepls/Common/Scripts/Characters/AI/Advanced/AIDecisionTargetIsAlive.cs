using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetIsAlive")]
    public class AIDecisionTargetIsAlive : AIDecision
    {
        protected Character _character;
        public override bool Decide()
        {
            return CheckIfTargetIsAlive();
        }
        protected virtual bool CheckIfTargetIsAlive()
        {
            if (_brain.Target == null)
            {
                return false;
            }

            _character = _brain.Target.gameObject.MMGetComponentNoAlloc<Character>();
            if (_character != null)
            {
                if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
                {
                    return false;
                }
                else
                { 
                    return true;
                }
            }            

            return false;
        }
    }
}

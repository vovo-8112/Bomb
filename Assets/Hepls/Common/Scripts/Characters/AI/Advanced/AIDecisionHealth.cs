using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionHealth")]
    public class AIDecisionHealth : AIDecision
    {
        public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreatherThan, StrictlyGreaterThan }
        [Tooltip("the comparison mode with which we'll evaluate the HealthValue")]
        public ComparisonModes TrueIfHealthIs;
        [Tooltip("the Health value to compare to")]
        public int HealthValue;
        [Tooltip("whether we want this comparison to be done only once or not")]
        public bool OnlyOnce = true;

        protected Health _health;
        protected bool _once = false;
        public override void Initialization()
        {
            _health = _brain.gameObject.GetComponentInParent<Health>();
        }
        public override bool Decide()
        {
            return EvaluateHealth();
        }
        protected virtual bool EvaluateHealth()
        {
            bool returnValue = false;

            if (OnlyOnce && _once)
            {
                return false;
            }

            if (_health == null)
            {
                Debug.LogWarning("You've added an AIDecisionHealth to " + this.gameObject.name + "'s AI Brain, but this object doesn't have a Health component.");
                return false;
            }

            if (!_health.isActiveAndEnabled)
            {
                return false;
            }
            
            if (TrueIfHealthIs == ComparisonModes.StrictlyLowerThan)
            {
                returnValue = (_health.CurrentHealth < HealthValue);
            }

            if (TrueIfHealthIs == ComparisonModes.LowerThan)
            {
                returnValue = (_health.CurrentHealth <= HealthValue);
            }

            if (TrueIfHealthIs == ComparisonModes.Equals)
            {
                returnValue = (_health.CurrentHealth == HealthValue);
            }

            if (TrueIfHealthIs == ComparisonModes.GreatherThan)
            {
                returnValue = (_health.CurrentHealth >= HealthValue);
            }

            if (TrueIfHealthIs == ComparisonModes.StrictlyGreaterThan)
            {
                returnValue = (_health.CurrentHealth > HealthValue);
            }

            if (returnValue)
            {
                _once = true;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

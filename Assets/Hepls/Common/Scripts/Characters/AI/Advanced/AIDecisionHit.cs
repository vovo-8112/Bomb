using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionHit")]
    public class AIDecisionHit : AIDecision
    {
        [Tooltip("The number of hits required to return true")]
        public int NumberOfHits = 1;

        protected int _hitCounter;
        protected Health _health;
        public override void Initialization()
        {
            _health = _brain.gameObject.GetComponentInParent<Health>();
            _hitCounter = 0;
        }
        public override bool Decide()
        {
            return EvaluateHits();
        }
        protected virtual bool EvaluateHits()
        {
            return (_hitCounter >= NumberOfHits);
        }
        public override void OnEnterState()
        {
            base.OnEnterState();
            _hitCounter = 0;
        }
        public override void OnExitState()
        {
            base.OnExitState();
            _hitCounter = 0;
        }
        protected virtual void OnHit()
        {
            _hitCounter++;
        }
        protected virtual void OnEnable()
        {
            if (_health == null)
            {
                _health = this.gameObject.GetComponent<Health>();
            }

            if (_health != null)
            {
                _health.OnHit += OnHit;
            }
        }
        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHit -= OnHit;
            }
        }
    }
}

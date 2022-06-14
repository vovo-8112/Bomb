using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveTowardsTarget2D")]
    public class AIActionMoveTowardsTarget2D : AIAction
    {
        [Tooltip("the minimum distance from the target this Character can reach.")]
        public float MinimumDistance = 1f;

        protected CharacterMovement _characterMovement;
        protected int _numberOfJumps = 0;
        protected override void Initialization()
        {
            _characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
        }
        public override void PerformAction()
        {
            Move();
        }
        protected virtual void Move()
        {
            if (_brain.Target == null)
            {
                return;
            }
            
            if (this.transform.position.x < _brain.Target.position.x)
            {
                _characterMovement.SetHorizontalMovement(1f);
            }
            else
            {
                _characterMovement.SetHorizontalMovement(-1f);
            }

            if (this.transform.position.y < _brain.Target.position.y)
            {
                _characterMovement.SetVerticalMovement(1f);
            }
            else
            {
                _characterMovement.SetVerticalMovement(-1f);
            }
            
            if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumDistance)
            {
                _characterMovement.SetHorizontalMovement(0f);
            }

            if (Mathf.Abs(this.transform.position.y - _brain.Target.position.y) < MinimumDistance)
            {
                _characterMovement.SetVerticalMovement(0f);
            }
        }
        public override void OnExitState()
        {
            base.OnExitState();

            _characterMovement?.SetHorizontalMovement(0f);
            _characterMovement?.SetVerticalMovement(0f);
        }
    }
}

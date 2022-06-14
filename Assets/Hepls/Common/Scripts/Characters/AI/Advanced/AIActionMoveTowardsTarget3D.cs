using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionMoveTowardsTarget3D")]
    public class AIActionMoveTowardsTarget3D : AIAction
    {
        [Tooltip("the minimum distance from the target this Character can reach.")]
        public float MinimumDistance = 1f;

        protected Vector3 _directionToTarget;
        protected CharacterMovement _characterMovement;
        protected int _numberOfJumps = 0;
        protected Vector2 _movementVector;
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
            
            _directionToTarget = _brain.Target.position - this.transform.position;
            _movementVector.x = _directionToTarget.x;
            _movementVector.y = _directionToTarget.z;
            _characterMovement.SetMovement(_movementVector);


            if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumDistance)
            {
                _characterMovement.SetHorizontalMovement(0f);
            }

            if (Mathf.Abs(this.transform.position.z - _brain.Target.position.z) < MinimumDistance)
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

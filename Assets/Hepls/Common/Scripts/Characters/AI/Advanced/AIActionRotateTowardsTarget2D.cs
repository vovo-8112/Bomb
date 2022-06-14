using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRotateTowardsTarget2D")]
    public class AIActionRotateTowardsTarget2D : AIAction
    {
        [Header("Lock Rotation")]
        [Tooltip("whether or not to lock the X rotation. If set to false, the model will rotate on the x axis, to aim up or down")]
        public bool LockRotationX = false;

        protected CharacterRotation2D _characterRotation2D;
        protected Vector3 _targetPosition;
        protected override void Initialization()
        {
            _characterRotation2D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterRotation2D>();
        }
        public override void PerformAction()
        {
            Rotate();
        }
        protected virtual void Rotate()
        {
            if (_brain.Target == null)
            {
                return;
            }
            _targetPosition = _brain.Target.transform.position;
            if (LockRotationX)
            {
                _targetPosition.y = this.transform.position.y;
            }
            _characterRotation2D.ForcedRotationDirection = (_targetPosition - this.transform.position).normalized;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionPathfinderToTarget3D")]
    public class AIActionPathfinderToTarget3D : AIAction
    {
        protected CharacterMovement _characterMovement;
        protected CharacterPathfinder3D _characterPathfinder3D;
        protected override void Initialization()
        {
            _characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
            _characterPathfinder3D = this.gameObject.GetComponentInParent<CharacterPathfinder3D>();
        }
        public override void PerformAction()
        {
            Move();
        }
        protected virtual void Move()
        {
            if (_brain.Target == null)
            {
                _characterPathfinder3D.SetNewDestination(null);
                return;
            }
            else
            {
                _characterPathfinder3D.SetNewDestination(_brain.Target.transform);
            }
        }
        public override void OnExitState()
        {
            base.OnExitState();
            
            _characterPathfinder3D?.SetNewDestination(null);
            _characterMovement?.SetHorizontalMovement(0f);
            _characterMovement?.SetVerticalMovement(0f);
        }
    }
}

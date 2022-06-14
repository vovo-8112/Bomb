using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionPathfinderToPatrol3D")]
    public class AIActionPathfinderToPatrol3D : AIAction
    {
        protected CharacterMovement _characterMovement;
        protected CharacterPathfinder3D _characterPathfinder3D;
        protected Transform _backToPatrolTransform;
        protected AIActionMovePatrol3D _aiActionMovePatrol3D;
        protected override void Initialization()
        {
            _characterMovement = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
            _characterPathfinder3D = this.gameObject.GetComponentInParent<CharacterPathfinder3D>();
            _aiActionMovePatrol3D = this.gameObject.GetComponent<AIActionMovePatrol3D>();

            GameObject backToPatrolBeacon = new GameObject();
            backToPatrolBeacon.name = this.gameObject.name + "BackToPatrolBeacon";
            _backToPatrolTransform = backToPatrolBeacon.transform;
        }
        public override void PerformAction()
        {
            Move();
        }
        protected virtual void Move()
        {
            if (_aiActionMovePatrol3D == null)
            {
                return;
            }


            _backToPatrolTransform.position = _aiActionMovePatrol3D.LastReachedPatrolPoint;
            _characterPathfinder3D.SetNewDestination(_backToPatrolTransform);
            _brain.Target = _backToPatrolTransform;
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

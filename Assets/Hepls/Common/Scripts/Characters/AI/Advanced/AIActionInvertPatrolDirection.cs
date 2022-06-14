using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionInvertPatrolDirection")]
    public class AIActionInvertPatrolDirection : AIAction
    {
        [Header("Invert Patrol Action Bindings")]
        [Tooltip("the AIActionMovePatrol2D to invert the patrol direction on")]
        public AIActionMovePatrol2D _movePatrol2D;
        [Tooltip("the AIActionMovePatrol3D to invert the patrol direction on")]
        public AIActionMovePatrol3D _movePatrol3D;
        protected override void Initialization()
        {
            if (_movePatrol2D == null)
            {
                _movePatrol2D = this.gameObject.GetComponentInParent<AIActionMovePatrol2D>();    
            }
            if (_movePatrol3D == null)
            {
                _movePatrol3D = this.gameObject.GetComponentInParent<AIActionMovePatrol3D>();    
            }
        }
        public override void PerformAction()
        {
            _movePatrol2D?.ChangeDirection();
            _movePatrol3D?.ChangeDirection();
        }
    }
}

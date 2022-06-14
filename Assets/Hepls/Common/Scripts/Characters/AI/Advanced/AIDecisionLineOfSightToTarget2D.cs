using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionLineOfSightToTarget2D")]
    public class AIDecisionLineOfSightToTarget2D : AIDecision
    {
        [Tooltip("the layermask to consider as obstacles when trying to determine whether a line of sight is present")]
        public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the offset to apply (from the collider's center) when casting a ray from the agent to its target")]
        public Vector3 LineOfSightOffset = new Vector3(0, 0, 0);

        protected Vector2 _directionToTarget;
        protected Collider2D _collider;
        protected Vector2 _raycastOrigin;
        public override void Initialization()
        {
            _collider = this.gameObject.GetComponentInParent<Collider2D>();
        }
        public override bool Decide()
        {
            return CheckLineOfSight();
        }
        protected virtual bool CheckLineOfSight()
        {
            if (_brain.Target == null)
            {
                return false;
            }

            _raycastOrigin = _collider.bounds.center + LineOfSightOffset / 2;
            _directionToTarget = (Vector2)_brain.Target.transform.position - _raycastOrigin;
            
            RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _directionToTarget.normalized, _directionToTarget.magnitude, ObstacleLayerMask, Color.yellow, true);
            if (hit.collider == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetRadius2D")]
    public class AIDecisionDetectTargetRadius2D : AIDecision
    {
        [Tooltip("the radius to search our target in")]
        public float Radius = 3f;
        [Tooltip("the center of the search circle")]
        public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);
        [Tooltip("the layer(s) to search our target on")]
        public LayerMask TargetLayer;
        [Tooltip("whether or not to look for obstacles")]
        public bool ObstacleDetection = true;
        [Tooltip("the layer(s) to look for obstacles on")]
        public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("if this is true, this AI will be able to consider itself (or its children) a target")] 
        public bool CanTargetSelf = false;

        protected Collider2D _collider;
        protected Vector2 _facingDirection;
        protected Vector2 _raycastOrigin;
        protected Character _character;
        protected CharacterOrientation2D _orientation2D;
        protected Color _gizmoColor = Color.yellow;
        protected bool _init = false;
        protected Vector2 _boxcastDirection;
        protected Collider2D[] _results;
        protected Collider2D _hit;
        public override void Initialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            _orientation2D = _character?.FindAbility<CharacterOrientation2D>();
            _collider = this.gameObject.GetComponentInParent<Collider2D>();
            _gizmoColor.a = 0.25f;
            _init = true;
            _results = new Collider2D[10];
        }
        public override bool Decide()
        {
            return DetectTarget();
        }
        protected virtual bool DetectTarget()
        {
            _hit = null;
            
            if (_orientation2D != null)
            {
                _facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
                _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
                _raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;
            }
            else
            {
                _raycastOrigin = transform.position +  DetectionOriginOffset;
            }

            int numberOfResults = Physics2D.OverlapCircleNonAlloc(_raycastOrigin, Radius, _results, TargetLayer);     
            
            if (numberOfResults > 0)
            {
                _hit = _results[0];
                if (!CanTargetSelf)
                {
                    int counter = 0;
                    numberOfResults = 0;
                    bool found = false;
                    while (!found && (counter < 10))
                    {
                        _hit = _results[counter];
                        if ((_hit != null) && (_hit.gameObject != this.gameObject) && (!_hit.transform.IsChildOf(this.transform)))
                        {
                            found = true;
                            numberOfResults = 1;
                        }
                        counter++;
                    }
                }
            }
            
            if (numberOfResults == 0)
            {
                return false;
            }
            else
            {
                if (!ObstacleDetection)
                {
                    _brain.Target = _hit.gameObject.transform;
                    return true;
                }
                _boxcastDirection = (Vector2)(_hit.gameObject.MMGetComponentNoAlloc<Collider2D>().bounds.center - _collider.bounds.center);
                RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, _boxcastDirection.normalized, _boxcastDirection.magnitude, ObstacleMask);
                if (!hit)
                {
                    _brain.Target = _hit.gameObject.transform;
                    return true;
                }
                else
                {
                    return false;
                }                
            }
        }
        protected virtual void OnDrawGizmosSelected()
        {
            _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
            _raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_raycastOrigin, Radius);
            if (_init)
            {
                Gizmos.color = _gizmoColor;
                Gizmos.DrawSphere(_raycastOrigin, Radius);
            }            
        }
    }
}

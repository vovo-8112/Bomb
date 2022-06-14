using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetRadius3D")]
    public class AIDecisionDetectTargetRadius3D : AIDecision
    {
        [Tooltip("the radius to search our target in")]
        public float Radius = 3f;
        [Tooltip("the offset to apply (from the collider's center)")]
        public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);
        [Tooltip("the layer(s) to search our target on")]
        public LayerMask TargetLayerMask;
        [Tooltip("the layer(s) to block the sight")]
        public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;
        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float TargetCheckFrequency = 1f;
        [Tooltip("if this is true, this AI will be able to consider itself (or its children) a target")] 
        public bool CanTargetSelf = false;

        protected Collider _collider;
        protected Vector3 _raycastOrigin;
        protected Character _character;
        protected Color _gizmoColor = Color.yellow;
        protected bool _init = false;
        protected Vector3 _raycastDirection;
        protected float _lastTargetCheckTimestamp = 0f;
        protected Collider[] _hits;
        protected Collider _hit;
        protected bool _lastReturnValue = false;
        public override void Initialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            _collider = this.gameObject.GetComponentInParent<Collider>();
            _gizmoColor.a = 0.25f;
            _init = true;
            _lastReturnValue = false;
            _hits = new Collider[10];
        }
        public override bool Decide()
        {
            return DetectTarget();
        }
        protected virtual bool DetectTarget()
        {
            if (Time.time - _lastTargetCheckTimestamp < TargetCheckFrequency)
            {
                return _lastReturnValue;
            }

            _lastTargetCheckTimestamp = Time.time;

            _raycastOrigin = _collider.bounds.center + DetectionOriginOffset / 2;
            int numberOfCollidersFound = Physics.OverlapSphereNonAlloc(_raycastOrigin, Radius, _hits, TargetLayerMask);

            if (numberOfCollidersFound > 0)
            {
                _hit = _hits[0];
                if (!CanTargetSelf)
                {
                    int counter = 0;
                    numberOfCollidersFound = 0;
                    bool found = false;
                    while (!found && (counter < 10))
                    {
                        _hit = _hits[counter];
                        if ((_hit != null) && (_hit.gameObject != this.gameObject) && (!_hit.transform.IsChildOf(this.transform)))
                        {
                            found = true;
                            numberOfCollidersFound = 1;
                        }
                        counter++;
                    }
                }
            }
            
            if (numberOfCollidersFound > 0)
            {
                _raycastDirection = _hit.transform.position - _raycastOrigin;
                RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, Vector3.Distance(_hit.transform.position, _raycastOrigin), ObstacleMask.value, Color.yellow, true);
                if (hit.collider == null)
                {
                    _brain.Target = _hit.transform;
                    _lastReturnValue = true;
                }
                else
                {
                    _lastReturnValue = false;
                }
            }
            else
            {
                _lastReturnValue = false;
            }

            return _lastReturnValue;
        }
        protected virtual void OnDrawGizmosSelected()
        {
            _raycastOrigin = transform.position + DetectionOriginOffset / 2;

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

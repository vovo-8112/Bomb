using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetLine3D")]
    public class AIDecisionDetectTargetLine3D : AIDecision
    {
        public enum DetectMethods { Ray, WideRay }
        [Tooltip("the selected detection method : ray is a single ray, wide ray is more expensive but also more accurate")]
        public DetectMethods DetectMethod = DetectMethods.Ray;
        [Tooltip("the width of the ray to cast (if we're in WideRay mode only")]
        public float RayWidth = 1f;
        [Tooltip("the distance up to which we'll cast our rays")]
        public float DetectionDistance = 10f;
        [Tooltip("the offset to apply to the ray(s)")]
        public Vector3 DetectionOriginOffset = new Vector3(0,0,0);
        [Tooltip("the layer(s) on which we want to search a target on")]
        public LayerMask TargetLayer;
        [Tooltip("the layer(s) on which obstacles are set. Obstacles will block the ray")]
        public LayerMask ObstaclesLayer = LayerManager.ObstaclesLayerMask;
        [Tooltip("a transform to use as the rotation reference for detection raycasts. If you have a rotating model for example, you'll want to set it as your reference transform here.")]
        public Transform ReferenceTransform;
        [Tooltip("if this is true, this decision will force the weapon to aim in the detection direction")]
        public bool ForceAimToDetectionDirection = false;

        protected Vector3 _direction;
        protected float _distanceToTarget;
        protected Vector3 _raycastOrigin;
        protected Character _character;
        protected Color _gizmosColor = Color.yellow;
        protected Vector3 _gizmoCenter;
        protected Vector3 _gizmoSize;
        protected bool _init = false;
        protected CharacterHandleWeapon _characterHandleWeapon;
        public override void Initialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();
            _characterHandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
            _gizmosColor.a = 0.25f;
            _init = true;
            if (ReferenceTransform == null)
            {
                ReferenceTransform = this.transform;
            }
        }
        public override bool Decide()
        {
            return DetectTarget();
        }
        protected virtual bool DetectTarget()
        {
            bool hit = false;
            _distanceToTarget = 0;
            Transform target = null;
            RaycastHit raycast;

            _direction = ReferenceTransform.forward;
            _raycastOrigin = ReferenceTransform.position + DetectionOriginOffset ;

            if (DetectMethod == DetectMethods.Ray)
            {
                raycast = MMDebug.Raycast3D(_raycastOrigin, _direction, DetectionDistance, TargetLayer, MMColors.Gold, true);
                
            }
            else
            {
                hit = Physics.BoxCast(_raycastOrigin, Vector3.one * (RayWidth * 0.5f), _direction, out raycast, ReferenceTransform.rotation, DetectionDistance, TargetLayer);
            }
                
            if (raycast.collider != null)
            {
                hit = true;
                _distanceToTarget = Vector3.Distance(_raycastOrigin, raycast.point);
                target = raycast.collider.gameObject.transform;
            }

            if (hit)
            {
                float distance = Vector3.Distance(target.transform.position, _raycastOrigin);
                RaycastHit raycastObstacle = MMDebug.Raycast3D(_raycastOrigin, (target.transform.position - _raycastOrigin).normalized, distance, ObstaclesLayer, Color.gray, true);
                
                if ((raycastObstacle.collider != null) && (_distanceToTarget > raycastObstacle.distance))
                {
                    _brain.Target = null;
                    return false;
                }
                else
                {
                    _brain.Target = target;
                    return true;
                }
            }

            ForceDirection();
            
            _brain.Target = null;
            return false;           
        }
        protected virtual void ForceDirection()
        {
            if (!ForceAimToDetectionDirection)
            {
                return;
            }
            if (_characterHandleWeapon == null)
            {
                return;
            }
            if (_characterHandleWeapon.CurrentWeapon == null)
            {
                return;
            }
            _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>()?.SetCurrentAim(ReferenceTransform.forward);
        }
        protected virtual void OnDrawGizmos()
        {
            if (DetectMethod != DetectMethods.WideRay)
            {
                return;
            }

            Gizmos.color = _gizmosColor;
            

            
            _gizmoCenter = DetectionOriginOffset + Vector3.forward * DetectionDistance / 2f;
            _gizmoSize.x = RayWidth;
            _gizmoSize.y = RayWidth;
            _gizmoSize.z = DetectionDistance;
            
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            if (ReferenceTransform != null)
            {
                Gizmos.matrix = ReferenceTransform.localToWorldMatrix;
            }
            
            
            Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
            
            
        }
        
        
    }
}

using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(WeaponAim3D))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim 3D")]
    public class WeaponAutoAim3D : WeaponAutoAim
    {
        protected Vector3 _aimDirection;
        protected Collider[] _hit;
        protected Vector3 _raycastDirection;
        protected Collider _potentialHit;
        protected TopDownController3D _topDownController3D;
        protected Vector3 _origin;
        
        public Vector3 Origin
        {
            get
            {
                _origin = this.transform.position;
                if (_topDownController3D != null)
                {
                    _origin += Quaternion.FromToRotation(Vector3.forward, _topDownController3D.CurrentDirection.normalized) * DetectionOriginOffset;
                }
                return _origin;
            }
        }
        protected override void Initialization()
        {
            base.Initialization();
            _hit = new Collider[10];
            if (_weapon.Owner != null)
            {
                _topDownController3D = _weapon.Owner.GetComponent<TopDownController3D>();
            }
        }
        protected override bool ScanForTargets()
        {
            Target = null;
            
            float nearestDistance = float.MaxValue;
            float distance;

            int numberOfHits = Physics.OverlapSphereNonAlloc(Origin, ScanRadius, _hit, TargetsMask);

            if (numberOfHits > 0)
            {
                for (int i = 0; i < numberOfHits; i++)
                {
                    distance = (_raycastOrigin - _hit[i].transform.position).sqrMagnitude;
                    if (distance < nearestDistance)
                    {
                        _potentialHit = _hit[i];
                        nearestDistance = distance;
                    }
                }
                _raycastDirection = _potentialHit.transform.position - _raycastOrigin;
                RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, Vector3.Distance(_potentialHit.transform.position, _raycastOrigin), ObstacleMask.value, Color.yellow, true);
                if (hit.collider == null)
                {
                    Target = _potentialHit.transform;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }
        protected override void SetAim()
        {
            _aimDirection = (Target.transform.position - _raycastOrigin).normalized;
            _weaponAim.SetCurrentAim(_aimDirection);
        }
        protected override void DetermineRaycastOrigin()
        {
            _raycastOrigin = Origin;
        }
        
        protected override void OnDrawGizmos()
        {
            if (DrawDebugRadius)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Origin, ScanRadius);
            }
        }
    }
}

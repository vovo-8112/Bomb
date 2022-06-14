using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(WeaponAim2D))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim 2D")]
    public class WeaponAutoAim2D : WeaponAutoAim
    {
        protected CharacterOrientation2D _orientation2D;
        protected List<Collider2D> _detectionColliders;
        protected Vector2 _facingDirection;
        protected Vector3 _boxcastDirection;
        protected Vector3 _aimDirection;
        protected ContactFilter2D _contactFilter;
        protected Collider2D _potentialHit;
        protected bool _initialized = false;
        protected override void Initialization()
        {
            base.Initialization();
            _orientation2D = _weapon.Owner.GetComponent<Character>()?.FindAbility<CharacterOrientation2D>();
            _contactFilter = new ContactFilter2D();
            _contactFilter.layerMask = TargetsMask;
            _contactFilter.useLayerMask = true;
            _detectionColliders = new List<Collider2D>();
            _initialized = true;
        }
        protected override bool ScanForTargets()
        {
            if (!_initialized)
            {
                Initialization();
            }

            Target = null;

            int count = Physics2D.OverlapCircle(_weapon.Owner.transform.position, ScanRadius, _contactFilter, _detectionColliders);
            if (count == 0)
            {
                return false;
            }
            else
            {
                float nearestDistance = float.MaxValue;
                float distance;
                foreach (Collider2D collider in _detectionColliders)
                {
                    distance = (_weapon.Owner.transform.position - collider.transform.position).sqrMagnitude;
                    if (distance < nearestDistance)
                    {
                        _potentialHit = collider;
                        nearestDistance = distance;
                    }
                }

                _boxcastDirection = (Vector2)(_potentialHit.bounds.center - _raycastOrigin);
                RaycastHit2D hit = Physics2D.BoxCast(_raycastOrigin, LineOfFireBoxcastSize, 0f, _boxcastDirection.normalized, _boxcastDirection.magnitude, ObstacleMask);
                if (!hit)
                {
                    Target = _potentialHit.transform;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        protected override void SetAim()
        {
            _aimDirection = (Target.transform.position - _raycastOrigin).normalized;
            _weaponAim.SetCurrentAim(_aimDirection);
        }
        protected override void DetermineRaycastOrigin()
        {
            if (_orientation2D != null)
            {
                _facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
                _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
                _raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;
            }
            else
            {
                _raycastOrigin = transform.position + DetectionOriginOffset;
            }
        }
    }
}

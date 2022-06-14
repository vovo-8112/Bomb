using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Weapon))]
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Prevent Shooting when Close to Walls 3D")]
    public class WeaponPreventShootingWhenCloseToWalls3D : MonoBehaviour
    {
        [Tooltip("the angle to consider when deciding whether or not there's a wall in front of the weapon (usually 5 degrees is fine)")]
        public float Angle = 5f;
        [Tooltip("the max distance to the wall we want to prevent shooting from")]
        public float Distance = 2f;
        [Tooltip("the offset to apply to the detection (in addition and relative to the weapon's position)")]
        public Vector3 RaycastOriginOffset = Vector3.zero;
        [Tooltip("the layers to consider as obstacles")]
        public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;

        protected RaycastHit _hitLeft;
        protected RaycastHit _hitMiddle;
        protected RaycastHit _hitRight;
        protected Weapon _weapon;
        protected CharacterHandleWeapon _characterHandleWeapon;
        protected bool _shootStopped = false;
        protected virtual void Awake()
        {
            _weapon = this.GetComponent<Weapon>();
            _shootStopped = false;
        }
        protected virtual void Update()
        {
            if (_weapon == null)
            {
                return;
            }
            if (_weapon.Owner == null)
            {
                return;
            }
            if (_characterHandleWeapon == null)
            {
                _characterHandleWeapon = _weapon.Owner.GetComponent<Character>()?.FindAbility<CharacterHandleWeapon>();
            }
            if (_characterHandleWeapon == null)
            {
                return;
            }
            if (CheckForObstacles())
            {
                _characterHandleWeapon.ShootStop();
                _characterHandleWeapon.AbilityPermitted = false;
                _shootStopped = true;
            }
            else
            {
                if (_shootStopped)
                {
                    _shootStopped = false;
                    _characterHandleWeapon.AbilityPermitted = true;
                }
            }
            
        }
        protected virtual bool CheckForObstacles()
        {
            _hitLeft = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, Quaternion.Euler(0f, -Angle/2f, 0f) * this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);
            _hitMiddle = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);
            _hitRight = MMDebug.Raycast3D(this.transform.position + this.transform.rotation * RaycastOriginOffset, Quaternion.Euler(0f, Angle / 2f, 0f) * this.transform.forward, Distance, ObstacleLayerMask, Color.yellow, true);

            if ((_hitLeft.collider == null) && (_hitMiddle.collider == null) && (_hitRight.collider == null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

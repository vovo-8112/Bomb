using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("TopDown Engine/Environment/Dash Zone 3D")]
    public class DashZone3D : MonoBehaviour
    {
        [Header("Bindings")]
        [Tooltip("the collider of the obstacle you want to dash over")]
        public Collider CoverObstacleCollider;
        [Tooltip("the (optional) exit dash zone on the other side of the collider")]
        public List<DashZone3D> ExitDashZones;

        [Header("DashSettings")]
        [Tooltip("the distance of the dash triggered when entering the zone")]
        public float DashDistance = 3f;
        [Tooltip("the duration of the dash")]
        public float DashDuration;
        [Tooltip("the curve to apply to the dash")]
        public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        [Header("Settings")]
        [Tooltip("the max angle at which the character should approach the obstacle for the dash to happen")]
        public float MaxFacingAngle = 90f;
        [Tooltip("the duration in seconds before re-enabling all triggers in the zone")]
        public float TriggerResetDuration = 1f;
        [Tooltip("if this is false, the dash won't happen")]
        public bool DashAuthorized = true;

        protected CharacterDash3D _characterDash3D;
        protected CharacterHandleWeapon _characterHandleWeapon;
        protected WeaponAim3D _weaponAim3D;
        protected CharacterOrientation3D _characterOrientation3D;
        protected CharacterOrientation3D.RotationModes _rotationMode;
        protected WeaponAim.AimControls _weaponAimControl;
        protected Character _character;
        protected Collider _collider;
        protected WaitForSeconds _dashWaitForSeconds;
        protected WaitForSeconds _triggerResetForSeconds;
        protected Vector3 _direction1;
        protected Vector3 _direction2;
        protected bool _dashInProgress = false;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _collider = this.gameObject.GetComponent<Collider>();
            _collider.isTrigger = true;
            _dashWaitForSeconds = new WaitForSeconds(DashDuration);
            _triggerResetForSeconds = new WaitForSeconds(TriggerResetDuration);
        }
        protected virtual void OnTriggerEnter(Collider collider)
        {
            TestForDash(collider);
        }
        protected virtual void OnTriggerStay(Collider collider)
        {
            TestForDash(collider);
        }
        protected virtual void TestForDash(Collider collider)
        {
            if ((_dashInProgress == true) || !DashAuthorized)
            {
                return;
            }
            _character = collider.gameObject.MMGetComponentNoAlloc<Character>();
            _characterDash3D = _character?.FindAbility<CharacterDash3D>();
            if (_characterDash3D == null)
            {
                return;
            }
            _characterOrientation3D = _character?.FindAbility<CharacterOrientation3D>();

            _direction1 = (_character.CharacterModel.transform.forward ).normalized;
            _direction1.y = 0f;
            _direction2 = (this.transform.forward).normalized;
            _direction2.y = 0f;
            
            float angle = Vector3.Angle(_direction1, _direction2);
            if (angle > MaxFacingAngle)
            {
                return;
            }
            _characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
            StartCoroutine(DashSequence());
        }
        protected virtual IEnumerator DashSequence()
        {
            _dashInProgress = true;
            _characterDash3D.DashDistance = DashDistance;
            _characterDash3D.DashCurve = DashCurve;
            _characterDash3D.DashDuration = DashDuration;
            _characterDash3D.DashDirection = this.transform.forward;
            _character.LinkedInputManager.InputDetectionActive = false;
            if (_characterOrientation3D != null)
            {
                _rotationMode = _characterOrientation3D.RotationMode;
                _characterOrientation3D.RotationMode = CharacterOrientation3D.RotationModes.MovementDirection;
                _characterOrientation3D.ForcedRotation = true;
                _characterOrientation3D.ForcedRotationDirection = ((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized;
            }
            if (_characterHandleWeapon != null)
            {
                if (_characterHandleWeapon.CurrentWeapon != null)
                {
                    _weaponAim3D = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>();
                    if (_weaponAim3D != null)
                    {
                        _weaponAimControl = _weaponAim3D.AimControl;
                        _weaponAim3D.AimControl = WeaponAim.AimControls.Script;
                        _weaponAim3D.SetCurrentAim(((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized);
                    }
                }
            }
            CoverObstacleCollider.enabled = false;
            SetColliderTrigger(false);
            foreach(DashZone3D dashZone in ExitDashZones)
            {
                dashZone.SetColliderTrigger(false);
            }
            _characterDash3D.DashStart();
            yield return _dashWaitForSeconds;
            _character.LinkedInputManager.InputDetectionActive = true;
            if (_characterOrientation3D != null)
            {
                _characterOrientation3D.ForcedRotation = false;
                _characterOrientation3D.RotationMode = _rotationMode;
            }            
            if (_weaponAim3D != null)
            {
                _weaponAim3D.AimControl = _weaponAimControl;
            }            
            CoverObstacleCollider.enabled = true;
            yield return _triggerResetForSeconds;
            SetColliderTrigger(true);
            foreach (DashZone3D dashZone in ExitDashZones)
            {
                dashZone.SetColliderTrigger(true);
            }

            _dashInProgress = false;
        }
        public virtual void SetColliderTrigger(bool status)
        {
            _collider.enabled = status;
        }
        
    }
}

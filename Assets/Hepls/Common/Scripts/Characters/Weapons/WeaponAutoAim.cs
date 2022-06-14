using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    [RequireComponent(typeof(Weapon))]
    public abstract class WeaponAutoAim : MonoBehaviour
    {
        [Header("Layer Masks")]
        [Tooltip("the layermask on which to look for aim targets")]
        public LayerMask TargetsMask;
        [Tooltip("the layermask on which to look for obstacles")]
        public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;

        [Header("Scan for Targets")]
        [Tooltip("the radius (in units) around the character within which to search for targets")]
        public float ScanRadius = 15f;
        [Tooltip("the size of the boxcast that will be performed to verify line of fire")]
        public Vector2 LineOfFireBoxcastSize = new Vector2(0.1f, 0.1f);
        [Tooltip("the duration (in seconds) between 2 scans for targets")]
        public float DurationBetweenScans = 1f;
        [Tooltip("an offset to apply to the weapon's position for scan ")]
        public Vector3 DetectionOriginOffset = Vector3.zero;

        [Header("Weapon Rotation")]
        [Tooltip("the rotation mode to apply when a target is found")]
        public WeaponAim.RotationModes RotationMode;
        
        [Header("Camera Target")]
        [Tooltip("whether or not this component should take control of the camera target when a camera is found")]
        public bool MoveCameraTarget = true;
        [Tooltip("the normalized distance (between 0 and 1) at which the camera target should be, on a line going from the weapon owner (0) to the auto aim target (1)")]
        [Range(0f, 1f)]
        public float CameraTargetDistance = 0.5f;
        [Tooltip("the maximum distance from the weapon owner at which the camera target can be")]
        [MMCondition("MoveCameraTarget", true)]
        public float CameraTargetMaxDistance = 10f;
        [Tooltip("the speed at which to move the camera target")]
        [MMCondition("MoveCameraTarget", true)]
        public float CameraTargetSpeed = 5f;

        [Header("Aim Marker")]
        [Tooltip("An AimMarker prefab to use to show where this auto aim weapon is aiming")]
        public AimMarker AimMarkerPrefab;

        [Header("Feedback")]
        [Tooltip("A feedback to play when a target is found and we didn't have one already")]
        public MMFeedbacks FirstTargetFoundFeedback;
        [Tooltip("a feedback to play when we already had a target and just found a new one")]
        public MMFeedbacks NewTargetFoundFeedback;
        [Tooltip("a feedback to play when no more targets are found, and we just lost our last target")]
        public MMFeedbacks NoMoreTargetsFeedback;

        [Header("Debug")]
        [Tooltip("the current target of the auto aim module")]
        [MMReadOnly]
        public Transform Target;
        [Tooltip("whether or not to draw a debug sphere around the weapon to show its aim radius")]
        public bool DrawDebugRadius = true;
        
        protected float _lastScanTimestamp = 0f;
        protected WeaponAim _weaponAim;
        protected WeaponAim.AimControls _originalAimControl;
        protected WeaponAim.RotationModes _originalRotationMode;
        protected Vector3 _raycastOrigin;
        protected Weapon _weapon;
        protected bool _originalMoveCameraTarget;
        protected Transform _targetLastFrame;
        protected AimMarker _aimMarker;
        protected virtual void Start()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _weaponAim = this.gameObject.GetComponent<WeaponAim>();
            _weapon = this.gameObject.GetComponent<Weapon>();
            if (_weaponAim == null)
            {
                Debug.LogWarning(this.name + " : the WeaponAutoAim on this object requires that you add either a WeaponAim2D or WeaponAim3D component to your weapon.");
                return;
            }

            _originalAimControl = _weaponAim.AimControl;
            _originalRotationMode = _weaponAim.RotationMode;
            _originalMoveCameraTarget = _weaponAim.MoveCameraTargetTowardsReticle;

            FirstTargetFoundFeedback?.Initialization(this.gameObject);
            NewTargetFoundFeedback?.Initialization(this.gameObject);
            NoMoreTargetsFeedback?.Initialization(this.gameObject);

            if (AimMarkerPrefab != null)
            {
                _aimMarker = Instantiate(AimMarkerPrefab);
                _aimMarker.name = this.gameObject.name + "_AimMarker";
                _aimMarker.Disable();
            }
        }
        protected virtual void Update()
        {
            if (_weaponAim == null)
            {
                return;
            }

            DetermineRaycastOrigin();
            ScanIfNeeded();
            HandleTarget();
            HandleMoveCameraTarget();
            HandleTargetChange();
            _targetLastFrame = Target;
        }
        protected abstract void DetermineRaycastOrigin();
        protected abstract bool ScanForTargets();
        protected abstract void SetAim();
        protected Vector3 _newCamTargetPosition;
        protected Vector3 _newCamTargetDirection;
        protected virtual void HandleTargetChange()
        {
            if (Target == _targetLastFrame)
            {
                return;
            }

            if (_aimMarker != null)
            {
                _aimMarker.SetTarget(Target);
            }

            if (Target == null)
            {
                NoMoreTargets();
                return;
            }

            if (_targetLastFrame == null)
            {
                FirstTargetFound();
                return;
            }

            if ((_targetLastFrame != null) && (Target != null))
            {
                NewTargetFound();
            }
        }
        protected virtual void NoMoreTargets()
        {
            NoMoreTargetsFeedback?.PlayFeedbacks();
        }
        protected virtual void FirstTargetFound()
        {
            FirstTargetFoundFeedback?.PlayFeedbacks();
        }
        protected virtual void NewTargetFound()
        {
            NewTargetFoundFeedback?.PlayFeedbacks();
        }
        protected virtual void HandleMoveCameraTarget()
        {
            if (!MoveCameraTarget || (Target == null))
            {
                return;
            }
            
            _newCamTargetPosition = Vector3.Lerp(_weapon.Owner.transform.position, Target.transform.position, CameraTargetDistance);
            _newCamTargetDirection = _newCamTargetPosition - this.transform.position;
            
            if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
            {
                _newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
            }

            _newCamTargetPosition = this.transform.position + _newCamTargetDirection;

            _newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position,
                _newCamTargetPosition,
                Time.deltaTime * CameraTargetSpeed);

            _weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
        }
        protected virtual void ScanIfNeeded()
        {
            if (Time.time - _lastScanTimestamp > DurationBetweenScans)
            {
                ScanForTargets();
                _lastScanTimestamp = Time.time;
            }
        }
        protected virtual void HandleTarget()
        {
            if (Target == null)
            {
                _weaponAim.AimControl = _originalAimControl;
                _weaponAim.RotationMode = _originalRotationMode;
                _weaponAim.MoveCameraTargetTowardsReticle = _originalMoveCameraTarget;
            }
            else
            {
                _weaponAim.AimControl = WeaponAim.AimControls.Script;
                _weaponAim.RotationMode = RotationMode;
                if (MoveCameraTarget)
                {
                    _weaponAim.MoveCameraTargetTowardsReticle = false;
                }
                SetAim();
            }
        }
        protected virtual void OnDrawGizmos()
        {
            if (DrawDebugRadius)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_raycastOrigin, ScanRadius);
            }
        }
        protected virtual void OnDisable()
        {
            if (_aimMarker != null)
            {
                _aimMarker.Disable();
            }
        }
    }
}

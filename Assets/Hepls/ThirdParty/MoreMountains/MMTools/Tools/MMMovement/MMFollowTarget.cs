using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Movement/MMFollowTarget")]
    public class MMFollowTarget : MonoBehaviour
    {
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }
        public enum FollowModes { RegularLerp, MMLerp, MMSpring }
        public enum PositionSpaces { World, Local }

        [Header("Follow Position")]
        public bool FollowPosition = true;
        [MMCondition("FollowPosition", true)]
        public bool FollowPositionX = true;
        [MMCondition("FollowPosition", true)]
        public bool FollowPositionY = true;
        [MMCondition("FollowPosition", true)]
        public bool FollowPositionZ = true;
        [MMCondition("FollowPosition", true)] 
        public PositionSpaces PositionSpace = PositionSpaces.World;

        [Header("Follow Rotation")]
        public bool FollowRotation = true;

        [Header("Follow Scale")]
        public bool FollowScale = true;
        [MMCondition("FollowScale", true)]
        public float FollowScaleFactor = 1f;

        [Header("Target")]
        public Transform Target;
        [MMCondition("FollowPosition", true)]
        public Vector3 Offset;
        [MMCondition("FollowPosition", true)]
        public bool AddInitialDistanceXToXOffset = false;
        [MMCondition("FollowPosition", true)]
        public bool AddInitialDistanceYToYOffset = false;
        [MMCondition("FollowPosition", true)]
        public bool AddInitialDistanceZToZOffset = false;

        [Header("Position Interpolation")]
        public bool InterpolatePosition = true;
        [MMCondition("InterpolatePosition", true)]
        public FollowModes FollowPositionMode = FollowModes.MMLerp;
        [MMCondition("InterpolatePosition", true)]
        public float FollowPositionSpeed = 10f;
        [MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)] 
        [Range(0.01f, 1.0f)]
        public float PositionSpringDamping = 0.3f;
        [MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)]
        public float PositionSpringFrequency = 3f;

        [Header("Rotation Interpolation")]
        public bool InterpolateRotation = true;
        [MMCondition("InterpolateRotation", true)]
        public FollowModes FollowRotationMode = FollowModes.MMLerp;
        [MMCondition("InterpolateRotation", true)]
        public float FollowRotationSpeed = 10f;

        [Header("Scale Interpolation")]
        public bool InterpolateScale = true;
        [MMCondition("InterpolateScale", true)]
        public FollowModes FollowScaleMode = FollowModes.MMLerp;
        [MMCondition("InterpolateScale", true)]
        public float FollowScaleSpeed = 10f;

        [Header("Mode")]
        public UpdateModes UpdateMode = UpdateModes.Update;
        
        [Header("Distances")]
        public bool UseMinimumDistanceBeforeFollow = false;
        public float MinimumDistanceBeforeFollow = 1f;
        public bool UseMaximumDistance = false;
        public float MaximumDistance = 1f;

        [Header("Anchor")]
        public bool AnchorToInitialPosition;
        [MMCondition("AnchorToInitialPosition", true)]
        public float MaxDistanceToAnchor = 1f;
        
        protected bool _localSpace { get { return PositionSpace == PositionSpaces.Local; } }

        protected Vector3 _velocity = Vector3.zero;
        protected Vector3 _newTargetPosition;        
        protected Vector3 _initialPosition;
        protected Vector3 _lastTargetPosition;
        protected Vector3 _direction;
        protected Vector3 _newPosition;
        protected Vector3 _newScale;
        protected Quaternion _newTargetRotation;
        protected Quaternion _initialRotation;
        protected virtual void Start()
        {
            Initialization();
        }
        public virtual void Initialization()
        {
            SetInitialPosition();
            SetOffset();
        }
        public virtual void StopFollowing()
        {
            FollowPosition = false;
        }
        public virtual void StartFollowing()
        {
            FollowPosition = true;
            SetInitialPosition();
        }
        protected virtual void SetInitialPosition()
        {
            _initialPosition = _localSpace ? this.transform.localPosition : this.transform.position;
            _initialRotation = this.transform.rotation;
            _lastTargetPosition = _localSpace ? this.transform.localPosition : this.transform.position;
        }
        protected virtual void SetOffset()
        {
            if (Target == null)
            {
                return;
            }
            Vector3 difference = this.transform.position - Target.transform.position;
            Offset.x = AddInitialDistanceXToXOffset ? difference.x : Offset.x;
            Offset.y = AddInitialDistanceYToYOffset ? difference.y : Offset.y;
            Offset.z = AddInitialDistanceZToZOffset ? difference.z : Offset.z;
        }
        protected virtual void Update()
        {
            if (Target == null)
            {
                return;
            }
            if (UpdateMode == UpdateModes.Update)
            {
                FollowTargetRotation();
                FollowTargetScale();
                FollowTargetPosition();
            }
        }
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                FollowTargetRotation();
                FollowTargetScale();
                FollowTargetPosition();
            }
        }
        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
                FollowTargetRotation();
                FollowTargetScale();
                FollowTargetPosition();
            }
        }
        protected virtual void FollowTargetPosition()
        {
            if (Target == null)
            {
                return;
            }

            if (!FollowPosition)
            {
                return;
            }

            _newTargetPosition = Target.position + Offset;
            if (!FollowPositionX) { _newTargetPosition.x = _initialPosition.x; }
            if (!FollowPositionY) { _newTargetPosition.y = _initialPosition.y; }
            if (!FollowPositionZ) { _newTargetPosition.z = _initialPosition.z; }

            float trueDistance = 0f;
            _direction = (_newTargetPosition - this.transform.position).normalized;
            trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);
            
            float interpolatedDistance = trueDistance;
            if (InterpolatePosition)
            {
                switch (FollowPositionMode)
                {
                    case FollowModes.MMLerp:
                        interpolatedDistance = MMMaths.Lerp(0f, trueDistance, FollowPositionSpeed, Time.deltaTime);
                        interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                        this.transform.Translate(_direction * interpolatedDistance, Space.World);
                        break;
                    case FollowModes.RegularLerp:
                        interpolatedDistance = Mathf.Lerp(0f, trueDistance, Time.deltaTime * FollowPositionSpeed);
                        interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                        this.transform.Translate(_direction * interpolatedDistance, Space.World);
                        break;
                    case FollowModes.MMSpring:
                        _newPosition = this.transform.position;
                        MMMaths.Spring(ref _newPosition, _newTargetPosition, ref _velocity, PositionSpringDamping, PositionSpringFrequency, FollowPositionSpeed, Time.deltaTime);
                        if (_localSpace)
                        {
                            this.transform.localPosition = _newPosition;   
                        }
                        else
                        {
                            this.transform.position = _newPosition;    
                        }
                        break;
                }                
            }
            else
            {
                interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
                this.transform.Translate(_direction * interpolatedDistance, Space.World);
            }

            if (AnchorToInitialPosition)
            {
                if (Vector3.Distance(this.transform.position, _initialPosition) > MaxDistanceToAnchor)
                {
                    if (_localSpace)
                    {
                        this.transform.localPosition = _initialPosition + Vector3.ClampMagnitude(this.transform.localPosition - _initialPosition, MaxDistanceToAnchor);   
                    }
                    else
                    {
                        this.transform.position = _initialPosition + Vector3.ClampMagnitude(this.transform.position - _initialPosition, MaxDistanceToAnchor);    
                    }
                }
            }
        }
        protected virtual float ApplyMinMaxDistancing(float trueDistance, float interpolatedDistance)
        {
            if (UseMinimumDistanceBeforeFollow && (trueDistance - interpolatedDistance < MinimumDistanceBeforeFollow))
            {
                interpolatedDistance = 0f;
            }

            if (UseMaximumDistance && (trueDistance - interpolatedDistance >= MaximumDistance))
            {
                interpolatedDistance = trueDistance - MaximumDistance;
            }

            return interpolatedDistance;
        }
        protected virtual void FollowTargetRotation()
        {
            if (Target == null)
            {
                return;
            }

            if (!FollowRotation)
            {
                return;
            }

            _newTargetRotation = Target.rotation;

            if (InterpolateRotation)
            {
                switch (FollowRotationMode)
                {
                    case FollowModes.MMLerp:
                        this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
                        break;
                    case FollowModes.RegularLerp:
                        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _newTargetRotation, Time.deltaTime * FollowRotationSpeed);
                        break;
                    case FollowModes.MMSpring:
                        this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
                        break;
                }
            }
            else
            {
                this.transform.rotation = _newTargetRotation;
            }
        }
        protected virtual void FollowTargetScale()
        {
            if (Target == null)
            {
                return;
            }

            if (!FollowScale)
            {
                return;
            }

            _newScale = Target.localScale * FollowScaleFactor;

            if (InterpolateScale)
            {
                switch (FollowScaleMode)
                {
                    case FollowModes.MMLerp:
                        this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newScale, FollowScaleSpeed, Time.deltaTime);
                        break;
                    case FollowModes.RegularLerp:
                        this.transform.localScale = Vector3.Lerp(this.transform.localScale, _newScale, Time.deltaTime * FollowScaleSpeed);
                        break;
                    case FollowModes.MMSpring:
                        this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newScale, FollowScaleSpeed, Time.deltaTime);
                        break;
                }
            }
            else
            {
                this.transform.localScale = _newScale;
            }
        }
        
        public virtual void ChangeFollowTarget(Transform newTarget) => Target = newTarget;
    }
}
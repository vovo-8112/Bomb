using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Tools
{
    public class MMFaceDirection : MonoBehaviour
    {
        public enum UpdateModes { Update, LateUpdate, FixedUpdate }
        public enum ForwardVectors { Forward, Up, Right }
        public enum FacingModes { MovementDirection, Target }
        
        [Header("Facing Mode")]
        public FacingModes FacingMode = FacingModes.MovementDirection;
        [MMEnumCondition("FacingMode", (int) FacingModes.Target)]
        public Transform FacingTarget;
        [MMEnumCondition("FacingMode", (int) FacingModes.MovementDirection)]
        public float MinimumMovementThreshold = 0.2f;
        
        [Header("Directions")]
        public ForwardVectors ForwardVector = ForwardVectors.Forward;
        public Vector3 DirectionRotationAngles = Vector3.zero;
        
        [Header("Timing")]
        public UpdateModes UpdateMode = UpdateModes.LateUpdate;
        public float InterpolationSpeed = 0.15f;
        
        protected Vector3 _direction;
        protected Vector3 _positionLastFrame;
        protected Transform _transform;
        protected Vector3 _upwards;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _transform = this.transform;
            _positionLastFrame = _transform.position;
            switch (ForwardVector)
            {
                case ForwardVectors.Forward:
                    _upwards = Vector3.forward;
                    break;
                case ForwardVectors.Up:
                    _upwards = Vector3.up;
                    break;
                case ForwardVectors.Right:
                    _upwards = Vector3.right;
                    break;
            }
        }
        protected virtual void FaceDirection()
        {
            if (FacingMode == FacingModes.Target)
            {
                _direction = FacingTarget.position - _transform.position;
                _direction = Quaternion.Euler(DirectionRotationAngles.x, DirectionRotationAngles.y, DirectionRotationAngles.z) * _direction;
                ApplyRotation();
            }
            
            if (FacingMode == FacingModes.MovementDirection)
            {
                _direction = (_transform.position - _positionLastFrame).normalized;
                _direction = Quaternion.Euler(DirectionRotationAngles.x, DirectionRotationAngles.y, DirectionRotationAngles.z) * _direction;
                
                if (Vector3.Distance(_transform.position, _positionLastFrame) > MinimumMovementThreshold)
                {
                    ApplyRotation();
                    _positionLastFrame = _transform.position;    
                }
            }
        }
        protected virtual void ApplyRotation()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction, _upwards), InterpolationSpeed * Time.time);
        }
        protected virtual void Update()
        {
            if (UpdateMode == UpdateModes.Update) { FaceDirection(); }
        }
        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate) { FaceDirection(); }
        }
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate) { FaceDirection(); }
        }
    }
}

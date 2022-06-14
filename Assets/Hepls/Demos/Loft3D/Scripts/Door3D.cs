using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Door3D : MonoBehaviour
    {
        [Header("Angles")]
        [Tooltip("the min angle the door can open at")]
        public float MinAngle = 90f;
        [Tooltip("the max angle the door can open at")]
        public float MaxAngle = 270f;
        [Tooltip("the min angle at which the door locks when open")]
        public float MinAngleLock = 90f;
        [Tooltip("the max angle at which the door locks when open")]
        public float MaxAngleLock = 270f;
        [Header("Safe Lock")]
        [Tooltip("the duration of the 'safe lock', a period during which the door is set to kinematic, to prevent glitches. That period ends after that safe lock duration, once the player has exited the door's area")]
        public float SafeLockDuration = 1f;

        [Header("Binding")]
        [Tooltip("the rigidbody associated to this door")]
        public Rigidbody Door;

        protected Vector3 _eulerAngles;
        protected Vector3 _initialPosition;
        protected Vector2 _initialDirection;
        protected Vector2 _currentDirection;
        protected float _lastContactTimestamp;
        protected Vector3 _minAngleRotation;
        protected Vector3 _maxAngleRotation;
        protected virtual void Start()
        {
            Door = Door.gameObject.GetComponent<Rigidbody>();
            _initialDirection.x = Door.transform.right.x;
            _initialDirection.y = Door.transform.right.z;

            _minAngleRotation = Vector3.zero;
            _minAngleRotation.y = MinAngleLock;
            _maxAngleRotation = Vector3.zero;
            _maxAngleRotation.y = MaxAngleLock;

            _initialPosition = Door.transform.position;
        }
        protected virtual void Update()
        {
            _currentDirection.x = Door.transform.right.x;
            _currentDirection.y = Door.transform.right.z;
            float Angle = MMMaths.AngleBetween(_initialDirection, _currentDirection);

            if ((Angle > MinAngle) && (Angle < MaxAngle) && (!Door.isKinematic))
            {
                if (Angle > 180)
                {
                    Door.transform.localRotation = Quaternion.Euler(_maxAngleRotation);
                }
                else
                {
                    Door.transform.localRotation = Quaternion.Euler(_minAngleRotation);
                }
                Door.transform.position = _initialPosition;
                Door.collisionDetectionMode = CollisionDetectionMode.Discrete;
                Door.isKinematic = true;
            }
            if ((Time.time - _lastContactTimestamp > SafeLockDuration) && (Door.isKinematic))
            {
                Door.isKinematic = false;
            }
        }
        protected virtual void OnTriggerStay(Collider collider)
        {
            _lastContactTimestamp = Time.time;
        }
    }
}
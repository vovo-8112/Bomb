using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Magnetic : MonoBehaviour
    {
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }

        [Header("Magnetic")]
        [Tooltip("the layermask this magnetic element is attracted to")]
        public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;
        [Tooltip("whether or not to start moving when something on the target layer mask enters this magnetic element's trigger")]
        public bool StartMagnetOnEnter = true;
        [Tooltip("whether or not to stop moving when something on the target layer mask exits this magnetic element's trigger")]
        public bool StopMagnetOnExit = false;
        [Tooltip("a unique ID for this type of magnetic objects. This can then be used by a MagneticEnabler to target only that specific ID. An ID of 0 will be picked by all MagneticEnablers automatically.")]
        public int MagneticTypeID = 0;

        [Header("Follow Position")]
        [Tooltip("whether or not the object is currently following its target's position")]
        public bool FollowPosition = true;

        [Header("Target")]
        [Tooltip("the target to follow, read only, for debug only")]
        [MMReadOnly]
        public Transform Target;
        [Tooltip("the offset to apply to the followed target")]
        [MMCondition("FollowPosition", true)]
        public Vector3 Offset;

        [Header("Position Interpolation")]
        [Tooltip("whether or not we need to interpolate the movement")]
        public bool InterpolatePosition = true;
        [MMCondition("InterpolatePosition", true)]
        [Tooltip("the speed at which to interpolate the follower's movement")]
        public float FollowPositionSpeed = 5f;
        [MMCondition("InterpolatePosition", true)]
        [Tooltip("the acceleration to apply to the object once it starts following")]
        public float FollowAcceleration = 0.75f;

        [Header("Mode")]
        [Tooltip("the update at which the movement happens")]
        public UpdateModes UpdateMode = UpdateModes.Update;

        [Header("State")]
        [Tooltip("an object this magnetic object should copy the active state on")]
        public GameObject CopyState;

        protected Collider2D _collider2D;
        protected Vector3 _velocity = Vector3.zero;
        protected Vector3 _newTargetPosition;
        protected Vector3 _lastTargetPosition;
        protected Vector3 _direction;
        protected Vector3 _newPosition;
        protected float _speed;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            _collider2D = this.gameObject.GetComponent<Collider2D>();
            if (_collider2D != null)
            {
                _collider2D.isTrigger = true;
            }
            StopFollowing();
            _speed = 0f;
        }
        protected virtual void OnTriggerEnter2D(Collider2D colliding)
        {
            OnTriggerEnterInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerExit2D(Collider2D colliding)
        {
            OnTriggerExitInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerEnter(Collider colliding)
        {
            OnTriggerEnterInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerExit(Collider colliding)
        {
            OnTriggerExitInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerEnterInternal(GameObject colliding)
        {
            if (!StartMagnetOnEnter)
            {
                return;
            }

            if (!TargetLayerMask.MMContains(colliding.layer))
            {
                return;
            }

            Target = colliding.transform;
            StartFollowing();
        }
        protected virtual void OnTriggerExitInternal(GameObject colliding)
        {
            if (!StopMagnetOnExit)
            {
                return;
            }

            if (!TargetLayerMask.MMContains(colliding.layer))
            {
                return;
            }

            StopFollowing();
        }
        protected virtual void Update()
        {
            if (CopyState != null)
            {
                this.gameObject.SetActive(CopyState.activeInHierarchy);
            }            

            if (Target == null)
            {
                return;
            }
            if (UpdateMode == UpdateModes.Update)
            {
                FollowTargetPosition();
            }
        }
        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                FollowTargetPosition();
            }
        }
        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
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

            float trueDistance = 0f;
            _direction = (_newTargetPosition - this.transform.position).normalized;
            trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);

            _speed = (_speed < FollowPositionSpeed) ? _speed + FollowAcceleration * Time.deltaTime : FollowPositionSpeed;

            float interpolatedDistance = trueDistance;
            if (InterpolatePosition)
            {
                interpolatedDistance = MMMaths.Lerp(0f, trueDistance, _speed, Time.deltaTime);
                this.transform.Translate(_direction * interpolatedDistance, Space.World);
            }
            else
            {
                this.transform.Translate(_direction * interpolatedDistance, Space.World);
            }
        }
        public virtual void StopFollowing()
        {
            FollowPosition = false;
        }
        public virtual void StartFollowing()
        {
            FollowPosition = true;
        }
        public virtual void SetTarget(Transform newTarget)
        {
            Target = newTarget;
        }
    }
}

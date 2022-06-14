using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class MagneticEnabler : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("the layermask this magnetic enabler looks at to enable magnetic elements")]
        public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;
        [Tooltip("a list of the magnetic type ID this enabler targets")]
        public List<int> MagneticTypeIDs;

        [Header("Overrides")]
        [Tooltip("if this is true, the follow position speed will be overridden with the one specified here")]
        public bool OverrideFollowPositionSpeed = false;
        [Tooltip("the speed with which to override the speed")]
        [MMCondition("OverrideFollowPositionSpeed", true)]
        public float FollowPositionSpeed = 5f;
        [Tooltip("if this is true, the acceleration will be overridden with the one specified here")]
        public bool OverrideFollowAcceleration = false;
        [Tooltip("the speed with which to override the acceleration")]
        [MMCondition("OverrideFollowAcceleration", true)]
        public float FollowAcceleration = 0.75f;

        protected Collider2D _collider2D;
        protected Magnetic _magnetic;
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
        }
        protected virtual void OnTriggerEnter2D(Collider2D colliding)
        {
            OnTriggerEnterInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerEnter(Collider colliding)
        {
            OnTriggerEnterInternal(colliding.gameObject);
        }
        protected virtual void OnTriggerEnterInternal(GameObject colliding)
        {
            if (!TargetLayerMask.MMContains(colliding.layer))
            {
                return;
            }

            _magnetic = colliding.MMGetComponentNoAlloc<Magnetic>();
            if (_magnetic == null)
            {
                return;
            }

            bool idFound = false;
            if (_magnetic.MagneticTypeID == 0)
            {
                idFound = true;
            }
            else
            {
                foreach (int id in MagneticTypeIDs)
                {
                    if (id == _magnetic.MagneticTypeID)
                    {
                        idFound = true;
                    }
                }
            }            

            if (!idFound)
            {
                return;
            }

            if (OverrideFollowAcceleration)
            {
                _magnetic.FollowAcceleration = FollowAcceleration;
            }

            if (OverrideFollowPositionSpeed)
            {
                _magnetic.FollowPositionSpeed = FollowPositionSpeed;
            }

            _magnetic.SetTarget(this.transform);
            _magnetic.StartFollowing();
        }
    }
}

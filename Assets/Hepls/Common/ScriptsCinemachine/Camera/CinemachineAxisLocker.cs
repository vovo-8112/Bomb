using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using Cinemachine;

namespace MoreMountains.TopDownEngine
{
    [ExecuteInEditMode]
    [SaveDuringPlay]
    [AddComponentMenu("")]
    public class CinemachineAxisLocker : CinemachineExtension
    {
        public enum Methods { ForcedPosition, InitialPosition, ColliderBoundsCenter, Collider2DBoundsCenter }
        [Tooltip("whether or not axis should be locked on X")]
        public bool LockXAxis = false;
        [Tooltip("whether or not axis should be locked on Y")]
        public bool LockYAxis = false;
        [Tooltip("whether or not axis should be locked on Z")]
        public bool LockZAxis = false;
        [Tooltip("the selected method to lock axis on ")]
        public Methods Method = Methods.InitialPosition;
        [MMEnumCondition("Method", (int)Methods.ForcedPosition)]
        [Tooltip("the position to lock axis based on")]
        public Vector3 ForcedPosition;
        [MMEnumCondition("Method", (int)Methods.ColliderBoundsCenter)]
        [Tooltip("the collider to lock axis on")]
        public Collider TargetCollider;
        [MMEnumCondition("Method", (int)Methods.Collider2DBoundsCenter)]
        [Tooltip("the 2D collider to lock axis on")]
        public Collider2D TargetCollider2D;

        protected Vector3 _forcedPosition;
        protected virtual void Start()
        {
            switch (Method)
            {
                case Methods.ForcedPosition:
                    _forcedPosition = ForcedPosition;
                    break;

                case Methods.InitialPosition:
                    _forcedPosition = this.transform.position;
                    break;

                case Methods.ColliderBoundsCenter:
                    _forcedPosition = TargetCollider.bounds.center;
                    break;

                case Methods.Collider2DBoundsCenter:
                    _forcedPosition = TargetCollider2D.bounds.center + (Vector3)TargetCollider2D.offset;
                    break;
            }
        }
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (enabled && stage == CinemachineCore.Stage.Body)
            {
                var pos = state.RawPosition;
                if (LockXAxis)
                {
                    pos.x = _forcedPosition.x;
                }
                if (LockYAxis)
                {
                    pos.y = _forcedPosition.y;
                }
                if (LockZAxis)
                {
                    pos.z = _forcedPosition.z;
                }
                state.RawPosition = pos;
            }
        }
    }
}

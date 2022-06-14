using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMRigidbodyCenterOfMass : MonoBehaviour
    {
        public enum AutomaticSetModes { Awake, Start, ScriptOnly }

        [Header("CenterOfMass")]
        public Vector3 CenterOfMassOffset;

        [Header("Automation")]
        public AutomaticSetModes AutomaticSetMode = AutomaticSetModes.Awake;
        public bool AutoDestroyComponentAfterSet = true;

        [Header("Test")]
        public float GizmoPointSize = 0.05f;
        [MMInspectorButton("SetCenterOfMass")]
        public bool SetCenterOfMassButton;

        protected Vector3 _gizmoCenter;
        protected Rigidbody _rigidbody;
        protected Rigidbody2D _rigidbody2D;
        protected virtual void Awake()
        {
            Initialization();

            if (AutomaticSetMode == AutomaticSetModes.Awake)
            {
                SetCenterOfMass();
            }
        }
        protected virtual void Start()
        {
            if (AutomaticSetMode == AutomaticSetModes.Start)
            {
                SetCenterOfMass();
            }
        }
        protected virtual void Initialization()
        {
            _rigidbody = this.gameObject.MMGetComponentNoAlloc<Rigidbody>();
            _rigidbody2D = this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>();
        }
        public virtual void SetCenterOfMass()
        {
            if (_rigidbody != null)
            {
                _rigidbody.centerOfMass = CenterOfMassOffset;
            }

            if (_rigidbody2D != null)
            {
                _rigidbody2D.centerOfMass = CenterOfMassOffset;
            }

            if (AutoDestroyComponentAfterSet)
            {
                Destroy(this);
            }
        }
        protected virtual void OnDrawGizmosSelected()
        {
            _gizmoCenter = this.transform.TransformPoint(CenterOfMassOffset);
            MMDebug.DrawGizmoPoint(_gizmoCenter, GizmoPointSize, Color.yellow);
        }
    }
}

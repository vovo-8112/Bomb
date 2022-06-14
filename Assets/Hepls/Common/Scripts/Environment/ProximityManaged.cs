using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    public class ProximityManaged : MonoBehaviour
    {
        [Header("Thresholds")]
        [Tooltip("the distance from the proximity center (the player) under which the object should be enabled")]
		public float EnableDistance = 35f;
        [Tooltip("the distance from the proximity center (the player) after which the object should be disabled")]
        public float DisableDistance = 45f;
        [MMReadOnly]
        [Tooltip("whether or not this object was disabled by the ProximityManager")]
        public bool DisabledByManager;

        [Header("Debug")]
        [Tooltip("a debug manager to add this object to, only used for debug")]
        public ProximityManager DebugProximityManager;
        [MMInspectorButton("DebugAddObject")]
        public bool AddButton;
        public virtual void DebugAddObject()
        {
            DebugProximityManager.AddControlledObject(this);
        }
    }
}

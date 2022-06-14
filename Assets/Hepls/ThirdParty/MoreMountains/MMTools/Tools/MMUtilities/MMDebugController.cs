using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [AddComponentMenu("More Mountains/Tools/Utilities/MMDebugController")]
    public class MMDebugController : MonoBehaviour
    {
        public bool DebugLogsEnabled = true;
        public bool DebugDrawEnabled = true;
        protected virtual void Awake()
        {
            MMDebug.SetDebugLogsEnabled(DebugLogsEnabled);
            MMDebug.SetDebugDrawEnabled(DebugDrawEnabled);
        }
    }
}

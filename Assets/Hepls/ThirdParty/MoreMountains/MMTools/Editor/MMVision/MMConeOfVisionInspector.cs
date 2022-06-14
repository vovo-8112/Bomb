using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEditor;

namespace MoreMountains.Tools
{
    [CustomEditor(typeof(MMConeOfVision))]
    public class MMConeOfVisionInspector : Editor
    {
        protected MMConeOfVision _coneOfVision;

        protected virtual void OnSceneGUI()
        {
            _coneOfVision = (MMConeOfVision)target;

            Handles.color = Color.yellow;
            Handles.DrawWireArc(_coneOfVision.Center, Vector3.up, Vector3.forward, 360f, _coneOfVision.VisionRadius);
            Vector3 visionAngleLeft = MMMaths.DirectionFromAngle(-_coneOfVision.VisionAngle / 2f, _coneOfVision.EulerAngles.y);
            Vector3 visionAngleRight = MMMaths.DirectionFromAngle(_coneOfVision.VisionAngle / 2f, _coneOfVision.EulerAngles.y);

            Handles.DrawLine(_coneOfVision.Center, _coneOfVision.Center + visionAngleLeft * _coneOfVision.VisionRadius);
            Handles.DrawLine(_coneOfVision.Center, _coneOfVision.Center + visionAngleRight * _coneOfVision.VisionRadius);

            foreach (Transform visibleTarget in _coneOfVision.VisibleTargets)
            {
                Handles.color = MMColors.Orange;
                Handles.DrawLine(_coneOfVision.Center, visibleTarget.position);
            }
        }
    }
}

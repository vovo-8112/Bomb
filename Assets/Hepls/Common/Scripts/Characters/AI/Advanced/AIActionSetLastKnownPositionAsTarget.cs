using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionSetLastKnownPositionAsTarget")]
    public class AIActionSetLastKnownPositionAsTarget : AIAction
    {
        protected Transform _targetTransform;
        protected override void Initialization()
        {
            GameObject newGo = new GameObject();
            newGo.name = "AIActionSetLastKnownPositionAsTarget_target";
            newGo.transform.SetParent(null);
            _targetTransform = newGo.transform;
        }
        public override void PerformAction()
        {
            _targetTransform.position = _brain._lastKnownTargetPosition;
            _brain.Target = _targetTransform;
        }
    }
}

using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Environment/Moving Platform 3D")]
    public class MovingPlatform3D : MMPathMovement
    {
        [Tooltip("The force to apply when pushing a character that'd be in the way of the moving platform")]
        public float PushForce = 5f;       
    }
}

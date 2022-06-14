using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMVector3Extensions
    {
        public static Vector3 MMSetX(this Vector3 vector, float newValue)
        {
            vector.x = newValue;
            return vector;
        }
        public static Vector3 MMSetY(this Vector3 vector, float newValue)
        {
            vector.y = newValue;
            return vector;
        }
        public static Vector3 MMSetZ(this Vector3 vector, float newValue)
        {
            vector.z = newValue;
            return vector;
        }
        public static Vector3 MMInvert(this Vector3 newValue)
        {
            return new Vector3
                (
                    1.0f / newValue.x,
                    1.0f / newValue.y,
                    1.0f / newValue.z
                );
        }
        public static Vector3 MMProject(this Vector3 vector, Vector3 projectedVector)
        {
            float _dot = Vector3.Dot(vector, projectedVector);
            return _dot * projectedVector;
        }
        public static Vector3 MMReject(this Vector3 vector, Vector3 rejectedVector)
        {
            return vector - vector.MMProject(rejectedVector);
        }
        public static Vector3 MMRound(this Vector3 vector)
        {
            vector.x = Mathf.Round(vector.x);
            vector.y = Mathf.Round(vector.y);
            vector.z = Mathf.Round(vector.z);
            return vector;
        }
    }
}

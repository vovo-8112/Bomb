using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMVector2Extensions
    {
        public static Vector2 MMRotate(this Vector2 vector, float angleInDegrees)
        {
            float sin = Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
            float tx = vector.x;
            float ty = vector.y;
            vector.x = (cos * tx) - (sin * ty);
            vector.y = (sin * tx) + (cos * ty);
            return vector;
        }
        public static Vector2 MMSetX(this Vector2 vector, float newValue)
        {
            vector.x = newValue;
            return vector;
        }
        public static Vector2 MMSetY(this Vector2 vector, float newValue)
        {
            vector.y = newValue;
            return vector;
        }
    }
}

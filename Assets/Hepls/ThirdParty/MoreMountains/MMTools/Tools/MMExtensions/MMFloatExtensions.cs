using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMFloatExtensions
    {
        public static float MMNormalizeAngle(this float angleInDegrees)
        {
            angleInDegrees = angleInDegrees % 360f;
            if (angleInDegrees < 0)
            {
                angleInDegrees += 360f;
            }
            return angleInDegrees;
        }
        public static float RoundDown(this float number, int decimalPlaces)
        {
            return Mathf.Floor(number * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        }
    }
}

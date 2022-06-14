using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMColorExtensions
    {
        public static float Sum(this Color color)
        {
            return color.r + color.g + color.b + color.a;
        }
        public static float MeanRGB(this Color color)
        {
            return (color.r + color.g + color.b) / 3f;
        }
        public static float Luminance(this Color color)
        {
            return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
        }
    }
}

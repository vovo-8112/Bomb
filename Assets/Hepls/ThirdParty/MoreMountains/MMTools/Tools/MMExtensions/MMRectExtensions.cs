using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class RectExtensions
    {
        public static bool MMIntersects(this Rect thisRectangle, Rect otherRectangle)
        {
            return !((thisRectangle.x > otherRectangle.xMax) || (thisRectangle.xMax < otherRectangle.x) || (thisRectangle.y > otherRectangle.yMax) || (thisRectangle.yMax < otherRectangle.y));
        }
    }
}

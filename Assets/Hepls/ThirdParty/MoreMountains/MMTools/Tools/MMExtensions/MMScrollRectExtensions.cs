using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class ScrollRectExtensions
    {
        public static void MMScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        public static void MMScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}

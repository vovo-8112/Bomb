using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public static class MMCoroutine
    {
        public static IEnumerator WaitForFrames(int frameCount)
        {
            while (frameCount > 0)
            {
                frameCount--;
                yield return null;
            }
        }
        public static IEnumerator WaitFor(float seconds)
        {
            for (float timer = 0f; timer < seconds; timer += Time.deltaTime)
            {
                yield return null;
            }
        }
        public static IEnumerator WaitForUnscaled(float seconds)
        {
            for (float timer = 0f; timer < seconds; timer += Time.unscaledDeltaTime)
            {
                yield return null;
            }
        }
    }
}


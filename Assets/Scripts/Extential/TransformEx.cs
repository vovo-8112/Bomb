using UnityEngine;

namespace Extential
{
    public static class ransformEx
    {
        public static Transform Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }

            return transform;
        }
    }
}
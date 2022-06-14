using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class TransformExtensions
    {
        public static void MMDestroyAllChildren(this Transform transform)
        {
            for (int t = transform.childCount - 1; t >= 0; t--)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(transform.GetChild(t).gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(transform.GetChild(t).gameObject);
                }
            }
        }
        public static Transform MMFindDeepChildBreadthFirst(this Transform parent, string transformName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                Transform child = queue.Dequeue();
                if (child.name == transformName)
                {
                    return child;
                }
                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }
            return null;
        }
        public static Transform MMFindDeepChildDepthFirst(this Transform parent, string transformName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == transformName)
                {
                    return child;
                }

                Transform result = child.MMFindDeepChildDepthFirst(transformName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        public static void ChangeLayersRecursively(this Transform transform, string layerName)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerName);
            }
        }
        public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
        {
            transform.gameObject.layer = layerIndex;
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerIndex);
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class RendererExtensions
    {
        public static bool MMIsVisibleFrom(this Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
    }
}

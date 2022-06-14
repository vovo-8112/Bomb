using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class MMCameraExtensions
    {
        public static float MMCameraWorldSpaceWidth(this Camera camera, float depth = 0f)
        {
            if (camera.orthographic)
            {
                return camera.aspect * camera.orthographicSize * 2f;
            }
            else
            {
                float fieldOfView = camera.fieldOfView * Mathf.Deg2Rad;
                return camera.aspect * depth * Mathf.Tan(fieldOfView);
            }
        }
        public static float MMCameraWorldSpaceHeight(this Camera camera, float depth = 0f)
        {
            if (camera.orthographic)
            {
                return camera.orthographicSize * 2f;
            }
            else
            {
                float fieldOfView = camera.fieldOfView * Mathf.Deg2Rad;
                return depth * Mathf.Tan(fieldOfView);
            }
        }
    }
}

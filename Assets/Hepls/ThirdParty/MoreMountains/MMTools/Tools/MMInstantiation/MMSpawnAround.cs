using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    [System.Serializable]
    public class MMSpawnAroundProperties
    {
        public enum MMSpawnAroundShapes { Sphere, Cube }
        [Header("Shape")] 
        [Tooltip("the shape within which objects should spawn")]
        public MMSpawnAroundShapes Shape = MMSpawnAroundShapes.Sphere;

        [Header("Position")]
        [Tooltip("a Vector3 that specifies the normal to the plane you want to spawn objects on (if you want to spawn objects on the x/z plane, the normal to that plane would be the y axis (0,1,0)")]
        public Vector3 NormalToSpawnPlane = Vector3.up;
        [Tooltip("the minimum distance to the origin of the spawn at which objects can be spawned")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
        public float MinimumSphereRadius = 1f;
        [Tooltip("the maximum distance to the origin of the spawn at which objects can be spawned")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
        public float MaximumSphereRadius = 2f;
        [Tooltip("the minimum size of the cube's base")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
        public Vector3 MinimumCubeBaseSize = Vector3.one;
        [Tooltip("the maximum size of the cube's base")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
        public Vector3 MaximumCubeBaseSize = new Vector3(2f, 2f, 2f);

        [Header("NormalAxisOffset")]
        [Tooltip("the minimum offset to apply on the normal axis")]
        public float MinimumNormalAxisOffset = 0f;
        [Tooltip("the maximum offset to apply on the normal axis")]
        public float MaximumNormalAxisOffset = 0f;

        [Header("NormalAxisOffsetCurve")]
        [Tooltip("whether or not to use a curve to offset the object's spawn position along the spawn plane")]
        public bool UseNormalAxisOffsetCurve = false;
        [Tooltip("a curve used to define how distance to the origin should be altered (potentially above min/max distance)")]
        [MMCondition("UseNormalAxisOffsetCurve",true)]
        public AnimationCurve NormalOffsetCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));
        [Tooltip("the value to which the curve's zero should be remapped to")]
        [MMCondition("UseNormalAxisOffsetCurve",true)]
        public float NormalOffsetCurveRemapZero = 0f;
        [Tooltip("the value to which the curve's one should be remapped to")]
        [MMCondition("UseNormalAxisOffsetCurve",true)]
        public float NormalOffsetCurveRemapOne = 1f;
        [Tooltip("whether or not to invert the curve (horizontally)")]
        [MMCondition("UseNormalAxisOffsetCurve",true)]
        public bool InvertNormalOffsetCurve = false;

        [Header("Rotation")]
        [Tooltip("the minimum random rotation to apply (in degrees)")]
        public Vector3 MinimumRotation = Vector3.zero;
        [Tooltip("the maximum random rotation to apply (in degrees)")]
        public Vector3 MaximumRotation = Vector3.zero;

        [Header("Scale")]
        [Tooltip("the minimum random scale to apply")]
        public Vector3 MinimumScale = Vector3.one;
        [Tooltip("the maximum random scale to apply")]
        public Vector3 MaximumScale = Vector3.one;
    }
    public static class MMSpawnAround 
    {
        public static void ApplySpawnAroundProperties(GameObject instantiatedObj, MMSpawnAroundProperties props, Vector3 origin)
        {
            instantiatedObj.transform.position = SpawnAroundPosition(props, origin);
            instantiatedObj.transform.rotation = SpawnAroundRotation(props);
            instantiatedObj.transform.localScale = SpawnAroundScale(props);
        }
        public static Vector3 SpawnAroundPosition(MMSpawnAroundProperties props, Vector3 origin)
        {
            Vector3 newPosition;
            if (props.Shape == MMSpawnAroundProperties.MMSpawnAroundShapes.Sphere)
            {
                float distance = Random.Range(props.MinimumSphereRadius, props.MaximumSphereRadius);
                newPosition = Vector3.Cross(Random.insideUnitSphere, props.NormalToSpawnPlane);
                newPosition.Normalize();
                newPosition *= distance;
            }
            else
            {
                float randomX = Random.Range(props.MinimumCubeBaseSize.x, props.MaximumCubeBaseSize.x);
                newPosition.x = Random.Range(-randomX, randomX) / 2f;
                float randomY = Random.Range(props.MinimumCubeBaseSize.y, props.MaximumCubeBaseSize.y);
                newPosition.y = Random.Range(-randomY, randomY) / 2f;
                float randomZ = Random.Range(props.MinimumCubeBaseSize.z, props.MaximumCubeBaseSize.z);
                newPosition.z = Random.Range(-randomZ, randomZ) / 2f;
                newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane); 
            }

            float randomOffset = Random.Range(props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset);
            if (props.UseNormalAxisOffsetCurve)
            {
                float normalizedOffset = 0f;
                if (randomOffset != 0)
                {
                    if (props.InvertNormalOffsetCurve)
                    {
                        normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 1f, 0f);
                    }
                    else
                    {
                        normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 0f, 1f);
                    }
                }

                float offset = props.NormalOffsetCurve.Evaluate(normalizedOffset);
                offset = MMMaths.Remap(offset, 0f, 1f, props.NormalOffsetCurveRemapZero, props.NormalOffsetCurveRemapOne);

                newPosition *= offset;
            }
            newPosition += props.NormalToSpawnPlane.normalized * randomOffset;
            newPosition += origin;

            return newPosition;
        }
        public static Vector3 SpawnAroundScale(MMSpawnAroundProperties props)
        {
            return MMMaths.RandomVector3(props.MinimumScale, props.MaximumScale);
        }
        public static Quaternion SpawnAroundRotation(MMSpawnAroundProperties props)
        {
            return Quaternion.Euler(MMMaths.RandomVector3(props.MinimumRotation, props.MaximumRotation));
        }
        public static void DrawGizmos(MMSpawnAroundProperties props, Vector3 origin, int quantity, float size, Color gizmosColor)
        {
            Gizmos.color = gizmosColor;
            for (int i = 0; i < quantity; i++)
            {
                Gizmos.DrawCube(SpawnAroundPosition(props, origin), SpawnAroundScale(props) * size);
            }
        }
    }
}

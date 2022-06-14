using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    #if UNITY_EDITOR
    [ExecuteAlways]
    #endif
    public class MMTransformRandomizer : MonoBehaviour
    {
        public enum AutoExecutionModes { Never, OnAwake, OnStart, OnEnable }

        [Header("Position")]
        public bool RandomizePosition = true;
        [MMCondition("RandomizePosition", true)]
        public Vector3 MinRandomPosition;
        [MMCondition("RandomizePosition", true)]
        public Vector3 MaxRandomPosition;

        [Header("Rotation")]
        public bool RandomizeRotation = true;
        [MMCondition("RandomizeRotation", true)]
        public Vector3 MinRandomRotation;
        [MMCondition("RandomizeRotation", true)]
        public Vector3 MaxRandomRotation;

        [Header("Scale")]
        public bool RandomizeScale = true;
        [MMCondition("RandomizeScale", true)]
        public Vector3 MinRandomScale;
        [MMCondition("RandomizeScale", true)]
        public Vector3 MaxRandomScale;

        [Header("Settings")]
        public bool AutoRemoveAfterRandomize = false;
        public bool RemoveAllColliders = false;
        public AutoExecutionModes AutoExecutionMode = AutoExecutionModes.Never;
        protected virtual void Awake()
        {
            if (Application.isPlaying && (AutoExecutionMode == AutoExecutionModes.OnAwake))
            {
                Randomize();
            }
        }
        protected virtual void Start()
        {
            if (Application.isPlaying && (AutoExecutionMode == AutoExecutionModes.OnStart))
            {
                Randomize();
            }
        }
        protected virtual void OnEnable()
        {
            if (Application.isPlaying && (AutoExecutionMode == AutoExecutionModes.OnEnable))
            {
                Randomize();
            }
        }
        public virtual void Randomize()
        {
            ProcessRandomizePosition();
            ProcessRandomizeRotation();
            ProcessRandomizeScale();
            RemoveColliders();
            Cleanup();
        }
        protected virtual void ProcessRandomizePosition()
        {
            if (!RandomizePosition)
            {
                return;
            }
            Vector3 randomPosition = MMMaths.RandomVector3(MinRandomPosition, MaxRandomPosition);
            this.transform.localPosition += randomPosition;
        }
        protected virtual void ProcessRandomizeRotation()
        {
            if (!RandomizeRotation)
            {
                return;
            }
            Vector3 randomRotation = MMMaths.RandomVector3(MinRandomRotation, MaxRandomRotation);
            this.transform.localRotation = Quaternion.Euler(randomRotation);
        }
        protected virtual void ProcessRandomizeScale()
        {
            if (!RandomizeScale)
            {
                return;
            }
            Vector3 randomScale = MMMaths.RandomVector3(MinRandomScale, MaxRandomScale);
            this.transform.localScale = randomScale;
        }
        protected virtual void RemoveColliders()
        {
            if (RemoveAllColliders)
            {
                #if UNITY_EDITOR
                Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    DestroyImmediate(collider);
                }
                Collider2D[] colliders2D = this.gameObject.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D collider2D in colliders2D)
                {
                    DestroyImmediate(collider2D);
                }
                #endif
            }
        }
        protected virtual void Cleanup()
        {
            if (AutoRemoveAfterRandomize)
            {
                #if UNITY_EDITOR
                    DestroyImmediate(this);
                #endif
            }
        }
    }
}

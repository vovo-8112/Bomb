using UnityEngine;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    [AddComponentMenu("More Mountains/Tools/Sprites/MMBezierLineRenderer")]
    public class MMBezierLineRenderer : MonoBehaviour
    {
        public Transform[] AdjustmentHandles;
        public int NumberOfSegments = 50;
        public string SortingLayerName = "Default";
        [MMReadOnly]
        public int NumberOfCurves = 0;

        protected int _sortingLayerID;
        protected LineRenderer _lineRenderer;
        protected Vector3 _point;
        protected Vector3 _p;
        protected bool _initialized = false;
        protected virtual void Awake()
        {
            Initialization();
        }
        protected virtual void Initialization()
        {
            if (_initialized)
            {
                return;
            }

            _sortingLayerID = SortingLayer.NameToID(SortingLayerName);

            NumberOfCurves = (int)AdjustmentHandles.Length / 3;

            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer != null)
            {
                _lineRenderer.sortingLayerID = _sortingLayerID;
            }
            _initialized = true;
        }
        protected virtual void LateUpdate()
        {
            DrawCurve();
        }
        protected virtual void DrawCurve()
        {
            for (int i = 0; i < NumberOfCurves; i++)
            {
                for (int j = 1; j <= NumberOfSegments; j++)
                {
                    float t = (j - 1) / (float)(NumberOfSegments - 1);
                    int pointIndex = i * 3;
                    _point = BezierPoint(t, AdjustmentHandles[pointIndex].position, AdjustmentHandles[pointIndex + 1].position, AdjustmentHandles[pointIndex + 2].position, AdjustmentHandles[pointIndex + 3].position);
                    _lineRenderer.positionCount = (i * NumberOfSegments) + j;                    
                    _lineRenderer.SetPosition((i * NumberOfSegments) + (j - 1), _point);
                }
            }
        }
        protected virtual Vector3 BezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            _p = uuu * p0;
            _p += 3 * uu * t * p1;
            _p += 3 * u * tt * p2;
            _p += ttt * p3;

            return _p;
        }
    }
}
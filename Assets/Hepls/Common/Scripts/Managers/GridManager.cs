using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Managers/GridManager")]
    public class GridManager : MMSingleton<GridManager>
    {
        public enum DebugDrawModes { TwoD, ThreeD }

        [Header("Grid")]
        [Tooltip("the origin of the grid in world space")]
        public Transform GridOrigin;
        [Tooltip("the size of each square grid cell")]
        public float GridUnitSize = 1f;

        [Header("Debug")]
        [Tooltip("whether or not to draw the debug grid")]
        public bool DrawDebugGrid = true;
        [MMCondition("DrawDebugGrid", true)]
        [Tooltip("the mode in which to draw the debug grid")]
        public DebugDrawModes DebugDrawMode = DebugDrawModes.TwoD;
        [MMCondition("DrawDebugGrid", true)]
        [Tooltip("the size (in squares of the debug grid)")]
        public int DebugGridSize = 30;
        [MMCondition("DrawDebugGrid", true)]
        [Tooltip("the color to use to draw the debug grid lines")]
        public Color CellBorderColor = new Color(60f, 221f, 255f, 1f);
        [MMCondition("DrawDebugGrid", true)]
        [Tooltip("the color to use to draw the debug grid cells backgrounds")]
        public Color InnerColor = new Color(60f, 221f, 255f, 0.3f);
        [HideInInspector]
        public List<Vector3> OccupiedGridCells;
        [HideInInspector]
        public Dictionary<GameObject, Vector3Int> LastPositions;
        [HideInInspector]
        public Dictionary<GameObject, Vector3Int> NextPositions;

        protected Vector3 _newGridPosition;
        protected Vector3 _debugOrigin = Vector3.zero;
        protected Vector3 _debugDestination = Vector3.zero;
        protected Vector3Int _workCoordinate = Vector3Int.zero;
        protected virtual void Start()
        {
            OccupiedGridCells = new List<Vector3>();
            LastPositions = new Dictionary<GameObject, Vector3Int>();
            NextPositions = new Dictionary<GameObject, Vector3Int>();
        }
        public virtual bool CellIsOccupied(Vector3 cellCoordinates)
        {
            return OccupiedGridCells.Contains(cellCoordinates);
        }
        public virtual void OccupyCell(Vector3 cellCoordinates)
        {
            if (!OccupiedGridCells.Contains(cellCoordinates))
            {
                OccupiedGridCells.Add(cellCoordinates);
            }
        }
        public virtual void FreeCell(Vector3 cellCoordinates)
        {
            if (OccupiedGridCells.Contains(cellCoordinates))
            {
                OccupiedGridCells.Remove(cellCoordinates);
            }
        }
        public virtual void SetNextPosition(GameObject trackedObject, Vector3Int cellCoordinates)
        {
            if (NextPositions.ContainsKey(trackedObject))
            {
                NextPositions[trackedObject] = cellCoordinates;
            }
            else
            {
                NextPositions.Add(trackedObject, cellCoordinates);
            }
        }
        public virtual void SetLastPosition(GameObject trackedObject, Vector3Int cellCoordinates)
        {
            if (LastPositions.ContainsKey(trackedObject))
            {
                LastPositions[trackedObject] = cellCoordinates;

            }
            else
            {
                LastPositions.Add(trackedObject, cellCoordinates);
            }
        }
        public virtual Vector3Int WorldToCellCoordinates(Vector3 worldPosition)
        {
            _newGridPosition = (worldPosition - GridOrigin.position) / GridUnitSize;

            _workCoordinate.x = Mathf.FloorToInt(_newGridPosition.x);
            _workCoordinate.y = Mathf.FloorToInt(_newGridPosition.y);
            _workCoordinate.z = Mathf.FloorToInt(_newGridPosition.z);

            return _workCoordinate;
        }
        public virtual Vector3 CellToWorldCoordinates(Vector3Int coordinates)
        {
            _newGridPosition = (Vector3)coordinates * GridUnitSize + GridOrigin.position;
            _newGridPosition += Vector3.one * (GridUnitSize / 2f);
            return _newGridPosition;
        }
        [System.Obsolete("As of v1.8 of the TopDown Engine, that method is obsolete, you should use WorldToCellCoordinates instead")]
        public virtual Vector3 ComputeGridPosition(Vector3 targetPosition)
        {
            _newGridPosition = (targetPosition - GridOrigin.position) / GridUnitSize;
            _newGridPosition.x = MMMaths.RoundToNearestHalf(_newGridPosition.x);
            _newGridPosition.y = MMMaths.RoundToNearestHalf(_newGridPosition.y);
            _newGridPosition.z = MMMaths.RoundToNearestHalf(_newGridPosition.z);
            
            return _newGridPosition;
        }
        [System.Obsolete("As of v1.8 of the TopDown Engine, that method is obsolete, you should use WorldToCellCoordinates instead")]
        public virtual Vector3 ComputeWorldPosition(Vector3 targetPosition)
        {
            return GridOrigin.position + (targetPosition * GridUnitSize);
        }
        protected virtual void OnDrawGizmos()
        {
            if (!DrawDebugGrid)
            {
                return;
            }

            Gizmos.color = CellBorderColor;

            if (DebugDrawMode == DebugDrawModes.ThreeD)
            {
                int i = -DebugGridSize;
                while (i <= DebugGridSize)
                {
                    _debugOrigin.x = GridOrigin.position.x - DebugGridSize * GridUnitSize;
                    _debugOrigin.y = GridOrigin.position.y;
                    _debugOrigin.z = GridOrigin.position.z + i * GridUnitSize;

                    _debugDestination.x = GridOrigin.position.x + DebugGridSize * GridUnitSize;
                    _debugDestination.y = GridOrigin.position.y;
                    _debugDestination.z = GridOrigin.position.z + i * GridUnitSize;

                    Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

                    _debugOrigin.x = GridOrigin.position.x + i * GridUnitSize;
                    _debugOrigin.y = GridOrigin.position.y;
                    _debugOrigin.z = GridOrigin.position.z - DebugGridSize * GridUnitSize; ;

                    _debugDestination.x = GridOrigin.position.x + i * GridUnitSize;
                    _debugDestination.y = GridOrigin.position.y;
                    _debugDestination.z = GridOrigin.position.z + DebugGridSize * GridUnitSize;

                    Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

                    i++;
                }
                Gizmos.color = InnerColor;
                for (int a = -DebugGridSize; a < DebugGridSize; a++)
                {
                    for (int b = -DebugGridSize; b < DebugGridSize; b++)
                    {
                        if ((a%2 == 0) && (b%2 != 0))
                        {
                            DrawCell3D(a, b);
                        }
                        if ((a%2 != 0) && (b%2 == 0))
                        {
                            DrawCell3D(a, b);
                        }
                    }
                }
            }
            else
            {
                int i = -DebugGridSize;
                while (i <= DebugGridSize)
                {
                    _debugOrigin.x = GridOrigin.position.x - DebugGridSize * GridUnitSize;
                    _debugOrigin.y = GridOrigin.position.y + i * GridUnitSize;
                    _debugOrigin.z = GridOrigin.position.z;

                    _debugDestination.x = GridOrigin.position.x + DebugGridSize * GridUnitSize;
                    _debugDestination.y = GridOrigin.position.y + i * GridUnitSize;
                    _debugDestination.z = GridOrigin.position.z;

                    Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

                    _debugOrigin.x = GridOrigin.position.x + i * GridUnitSize;
                    _debugOrigin.y = GridOrigin.position.y - DebugGridSize * GridUnitSize; ;
                    _debugOrigin.z = GridOrigin.position.z;

                    _debugDestination.x = GridOrigin.position.x + i * GridUnitSize;
                    _debugDestination.y = GridOrigin.position.y + DebugGridSize * GridUnitSize;
                    _debugDestination.z = GridOrigin.position.z;

                    Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

                    i++;
                }
                Gizmos.color = InnerColor;
                for (int a = -DebugGridSize; a < DebugGridSize; a++)
                {
                    for (int b = -DebugGridSize; b < DebugGridSize; b++)
                    {
                        if ((a % 2 == 0) && (b % 2 != 0))
                        {
                            DrawCell2D(a, b);
                        }
                        if ((a % 2 != 0) && (b % 2 == 0))
                        {
                            DrawCell2D(a, b);
                        }
                    }
                }
            }
        }
        protected virtual void DrawCell2D(int a, int b)
        {
            _debugOrigin.x = GridOrigin.position.x + a * GridUnitSize + GridUnitSize / 2f;            
            _debugOrigin.y = GridOrigin.position.y + b * GridUnitSize + GridUnitSize / 2f;
            _debugOrigin.z = GridOrigin.position.z;
            Gizmos.DrawCube(_debugOrigin, GridUnitSize * new Vector3(1f, 1f, 0f));
        }
        protected virtual void DrawCell3D(int a, int b)
        {
            _debugOrigin.x = GridOrigin.position.x + a * GridUnitSize + GridUnitSize / 2f;
            _debugOrigin.y = GridOrigin.position.y;
            _debugOrigin.z = GridOrigin.position.z + b * GridUnitSize + GridUnitSize / 2f;
            Gizmos.DrawCube(_debugOrigin, GridUnitSize * new Vector3(1f, 0f, 1f));
        }
    }
}
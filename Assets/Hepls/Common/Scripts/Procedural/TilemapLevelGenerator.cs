using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  MoreMountains.Tools;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace MoreMountains.TopDownEngine
{
    public class TilemapLevelGenerator : MMTilemapGenerator
    {
        [FormerlySerializedAs("GenerateOnStart")]
        [Header("TopDown Engine Settings")]
        [Tooltip("Whether or not this level should be generated automatically on Awake")]
        public bool GenerateOnAwake = false;

        [Header("Bindings")]
        [Tooltip("the Grid on which to work")]
        public Grid TargetGrid;
        [Tooltip("the tilemap containing the walls")]
        public Tilemap ObstaclesTilemap;
        [Tooltip("the tilemap containing the walls' shadows")]
        public MMTilemapShadow WallsShadowTilemap;
        [Tooltip("the level manager")]
        public LevelManager TargetLevelManager;

        [Header("Spawn")]
        [Tooltip("the object at which the player will spawn")]
        public Transform InitialSpawn;
        [Tooltip("the exit of the level")]
        public Transform Exit;
        [Tooltip("the minimum distance that should separate spawn and exit.")]
        public float MinDistanceFromSpawnToExit = 2f;

        protected const int _maxIterationsCount = 100;
        protected virtual void Awake()
        {
            if (GenerateOnAwake)
            {
                Generate();
            }
        }
        public override void Generate()
        {
            base.Generate();
            HandleWallsShadow();
            PlaceEntryAndExit();
            ResizeLevelManager();
        }
        protected virtual void ResizeLevelManager()
        {
            BoxCollider boxCollider = TargetLevelManager.GetComponent<BoxCollider>();
            
            Bounds bounds = ObstaclesTilemap.localBounds;
            boxCollider.center = bounds.center;
            boxCollider.size = new Vector3(bounds.size.x, bounds.size.y, boxCollider.size.z);
        }
        protected virtual void PlaceEntryAndExit()
        {
            UnityEngine.Random.InitState(GlobalSeed);
            int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
            int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
            Vector3 spawnPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
            InitialSpawn.transform.position = spawnPosition;

            Vector3 exitPosition = spawnPosition;
            int iterationsCount = 0;
            
            while ((Vector3.Distance(exitPosition, spawnPosition) < MinDistanceFromSpawnToExit) && (iterationsCount < _maxIterationsCount))
            {
                exitPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
                Exit.transform.position = exitPosition;
                iterationsCount++;
            }
        }
        protected virtual void HandleWallsShadow()
        {
            if (WallsShadowTilemap != null)
            {
                WallsShadowTilemap.UpdateShadows();
            }
        }
    }    
}


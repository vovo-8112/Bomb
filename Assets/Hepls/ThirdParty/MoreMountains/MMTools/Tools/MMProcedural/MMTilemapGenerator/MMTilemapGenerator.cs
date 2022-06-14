using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools 
{
    [ExecuteAlways]  
    public class MMTilemapGenerator : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("The width of the grid, in cells")]
        [MMVector("Min","Max")]
        public Vector2Int GridWidth = new Vector2Int(50,50);
        [Tooltip("the height of the grid, in cells")]
        [MMVector("Min","Max")]
        public Vector2Int GridHeight = new Vector2Int(50,50);

        [Header("Data")]
        [Tooltip("the list of layers that will be used to generate the tilemap")]
        public MMTilemapGeneratorLayerList Layers;
        [Tooltip("a value between 0 and 1 that will be used by all layers as their random seed. If you generate another map using the same seed, it'll look the same")]
        public int GlobalSeed = 0;
        [Tooltip("whether or not to randomize the global seed every time a new map is generated")]
        public bool RandomizeGlobalSeed = true;
        
        [Header("Slow Render")]
        [Tooltip("turning this to true will (at runtime only) draw the map progressively. This is really just for fun.")]
        public bool SlowRender = false;
        [Tooltip("the duration of the slow render, in seconds")]
        public float SlowRenderDuration = 1f;
        [Tooltip("the tween to use for the slow render")]
        public MMTweenType SlowRenderTweenType = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        
        protected int[,] _grid;

        protected int _width;
        protected int _height;
        public enum GenerateMethods
        {
            Full,
            Perlin,
            PerlinGround,
            Random,
            RandomWalk,
            RandomWalkAvoider,
            RandomWalkGround,
            Path,
            Copy
        }
        public virtual void Generate()
        {
            Random.InitState((int)System.DateTime.Now.Ticks);
            if (RandomizeGlobalSeed) { GlobalSeed = Mathf.Abs(Random.Range(int.MinValue, int.MaxValue)); }
            
            foreach (MMTilemapGeneratorLayer layer in Layers)
            {
                GenerateLayer(layer);   
            }
        }
        void Reset()
        {
            Layers = new MMTilemapGeneratorLayerList(){
                new MMTilemapGeneratorLayer()
            };
        }
        protected virtual void GenerateLayer(MMTilemapGeneratorLayer layer)
        {
            if (!layer.Active)
            {
                return;
            }
            
            if (layer.TargetTilemap == null) { Debug.LogError("Tilemap Generator : you need to specify a Target Tilemap to paint on."); }
            if (layer.Tile == null) { Debug.LogError("Tilemap Generator : you need to specify a Tile to paint with."); }
            if (layer.GridWidth == 0) { Debug.LogError("Tilemap Generator : grid width can't be 0."); }
            if (layer.GridHeight == 0) { Debug.LogError("Tilemap Generator : grid height can't be 0."); }

            float seedFloat = 0f;
            float layerSeedFloat = 0f;
            float globalSeedFloat = 0f;
            
            
            UnityEngine.Random.InitState(GlobalSeed);
            int width = layer.OverrideGridSize ? layer.GridWidth : UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
            int height = layer.OverrideGridSize ? layer.GridHeight : UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
            globalSeedFloat = UnityEngine.Random.value;
            if (layer.DoNotUseGlobalSeed)
            {
                Random.InitState((int)System.DateTime.Now.Ticks);
                if (layer.RandomizeSeed)
                {
                    layer.Seed = Mathf.Abs(Random.Range(int.MinValue, int.MaxValue));
                }
                UnityEngine.Random.InitState(layer.Seed);
                layerSeedFloat = UnityEngine.Random.value;
            }
            
            int seed = layer.DoNotUseGlobalSeed ? layer.Seed : GlobalSeed;
            seedFloat = layer.DoNotUseGlobalSeed ? layerSeedFloat : globalSeedFloat;

            switch (layer.GenerateMethod)
            {
                case GenerateMethods.Full:
                    _grid = MMGridGeneratorFull.Generate(width, height, layer.FullGenerationFilled);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.Perlin:
                    _grid = MMGridGeneratorPerlinNoise.Generate(width, height, seedFloat);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.PerlinGround:
                    _grid = MMGridGeneratorPerlinNoiseGround.Generate(width, height, seedFloat);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.Random:
                    _grid = MMGridGeneratorRandom.Generate(width, height, seed, layer.RandomFillPercentage);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.RandomWalk:
                    _grid = MMGridGeneratorRandomWalk.Generate(width, height, seed, layer.RandomWalkPercent, layer.RandomWalkStartingPoint, layer.RandomWalkMaxIterations);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.RandomWalkAvoider:

                    int[,] obstacleGrid = MMGridGenerator.TilemapToGrid(layer.RandomWalkAvoiderObstaclesTilemap, width, height);
                    _grid = MMGridGeneratorRandomWalkAvoider.Generate(width, height, seed, layer.RandomWalkAvoiderPercent, layer.RandomWalkAvoiderStartingPoint, obstacleGrid, layer.RandomWalkAvoiderObstaclesDistance, layer.RandomWalkAvoiderMaxIterations);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.RandomWalkGround:
                    _grid = MMGridGeneratorRandomWalkGround.Generate(width, height, seed, 
                        layer.RandomWalkGroundMinHeightDifference, layer.RandomWalkGroundMaxHeightDifference, 
                        layer.RandomWalkGroundMinFlatDistance, layer.RandomWalkGroundMaxFlatDistance, layer.RandomWalkGroundMaxHeight);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.Path:
                    _grid = MMGridGeneratorPath.Generate(width, height, seed, layer.PathDirection, layer.PathStartPosition, layer.PathMinWidth,
                        layer.PathMaxWidth, layer.PathDirectionChangeDistance, layer.PathWidthChangePercentage,
                        layer.PathDirectionChangePercentage);
                    layer.Grid = _grid;
                    break;
                case GenerateMethods.Copy:
                    layer.TargetTilemap.ClearAllTiles();
                    DelayedCopy(layer);
                    break;
            }

            if (layer.Smooth) { _grid = MMGridGenerator.SmoothenGrid(_grid); }
            if (layer.InvertGrid) { _grid = MMGridGenerator.InvertGrid(_grid); }

            _grid = MMGridGenerator.BindGrid(_grid, layer.BoundsTop, layer.BoundsBottom, layer.BoundsLeft, layer.BoundsRight);
            _grid = MMGridGenerator.ApplySafeSpots(_grid, layer.SafeSpots);

            RenderGrid(layer);
        }
        async static void DelayedCopy(MMTilemapGeneratorLayer layer)
        {
            await Task.Delay(500);
            MMTilemapShadow.Copy(layer.CopyTilemap, layer.TargetTilemap);
        }
        protected virtual void RenderGrid(MMTilemapGeneratorLayer layer)
        {
            MMTilemapGridRenderer.RenderGrid(_grid, layer, SlowRender, SlowRenderDuration, SlowRenderTweenType,this);
        }
        protected virtual void OnValidate()
        {
            if ((Layers == null) || (Layers.Count <= 0))
            {
                return;
            }
            foreach (MMTilemapGeneratorLayer layer in Layers)
            {
                layer.SetDefaults();
            }
        }
    }    
}


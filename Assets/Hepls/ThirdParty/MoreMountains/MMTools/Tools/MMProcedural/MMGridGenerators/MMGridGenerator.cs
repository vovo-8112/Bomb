using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    public class MMGridGenerator
    {
        public static int[,] PrepareGrid(ref int width, ref int height)
        {
            int[,] grid = new int[width, height];
            return grid;
        }
        public static bool SetGridCoordinate(int[,] grid, int x, int y, int value)
        {
            if (
                (x >= 0) 
                && (x <= grid.GetUpperBound(0)) 
                && (y >= 0) 
                && (y <= grid.GetUpperBound(1)) 
            )
            {
                grid[x, y] = value;
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int[,] TilemapToGrid(Tilemap tilemap, int width, int height)
        {
            if(tilemap == null)
            {
                Debug.LogError("[MMGridGenerator] You're trying to convert a tilemap into a grid but didn't specify what tilemap to convert.");
                return null;
            }
        
            int[,] grid = new int[width, height];
            Vector3Int currentPosition = Vector3Int.zero;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    currentPosition.x = i;
                    currentPosition.y = j;
                    currentPosition += MMTilemapGridRenderer.ComputeOffset(width-1, height-1);

                    grid[i, j] = (tilemap.GetTile(currentPosition) == null) ? 0 : 1;
                }
            }
            return grid;
        }
        public static void DebugGrid(int[,] grid, int width, int height)
        {
            string output = "";
            for (int j = height-1; j >= 0; j--)
            {
                output += "line "+j+" [";
                for (int i = 0; i < width; i++)
                {
                    output += grid[i, j];
                    if (i < width - 1)
                    {
                        output += ", ";
                    }
                }
                output += "]\n";
            }
            Debug.Log(output);
        }
        public static int GetValueAtGridCoordinate(int[,] grid, int x, int y, int errorValue)
        {
            if (
                (x >= 0) 
                && (x <= grid.GetUpperBound(0)) 
                && (y >= 0) 
                && (y <= grid.GetUpperBound(1)) 
            )
            {
                return grid[x, y];
            }
            else
            {
                return errorValue;
            }
        }
        public static int[,] InvertGrid(int[,] grid)
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = grid[i, j] == 0 ? 1 : 0;
                }
            }

            return grid;
        }
        public static int[,] SmoothenGrid(int[,] grid) 
        {
            int width = grid.GetUpperBound(0);
            int height = grid.GetUpperBound(1);
            
            for (int i = 0; i <= width; i ++) 
            {
                for (int j = 0; j <= height; j ++) 
                {
                    int adjacentWallsCount = GetAdjacentWallsCount(grid, i , j);

                    if (adjacentWallsCount > 4)
                    {
                        grid[i,j] = 1;
                    }
                    else if (adjacentWallsCount < 4)
                    {
                        grid[i,j] = 0;
                    }
                }
            }
            return grid;
        }
        public static int[,] ApplySafeSpots(int[,] grid, List<MMTilemapGeneratorLayer.MMTilemapGeneratorLayerSafeSpot> safeSpots)
        {
            foreach (MMTilemapGeneratorLayer.MMTilemapGeneratorLayerSafeSpot safeSpot in safeSpots)
            {
                int minX = Mathf.Min(safeSpot.Start.x, safeSpot.End.x);
                int maxX = Mathf.Max(safeSpot.Start.x, safeSpot.End.x);
                int minY = Mathf.Min(safeSpot.Start.y, safeSpot.End.y);
                int maxY = Mathf.Max(safeSpot.Start.y, safeSpot.End.y);

                for (int i = minX; i < maxX; i++)
                {
                    for (int j = minY; j < maxY; j++)
                    {
                        SetGridCoordinate(grid, i, j, 0);
                    }
                }
            }
            return grid;
        }
        public static int[,] BindGrid(int[,] grid, bool top, bool bottom, bool left, bool right)
        {
            int width = grid.GetUpperBound(0);
            int height = grid.GetUpperBound(1);
                
            if (top)
            {
                for (int i = 0; i <= width; i++)
                {
                    
                    grid[i, height] = 1;
                }
            }
                
            if (bottom)
            {
                for (int i = 0; i <= width; i++)
                {
                    grid[i, 0] = 1;
                }
            }
                
            if (left)
            {
                for (int j = 0; j <= height; j++)
                {
                    grid[0, j] = 1;
                }
            }
                
            if (right)
            {
                for (int j = 0; j <= height; j++)
                {
                    grid[width, j] = 1;
                }
            }

            return grid;
        }
        public static int GetAdjacentWallsCount(int[,] grid, int x, int y) 
        {
            int width = grid.GetUpperBound(0);
            int height = grid.GetUpperBound(1);
            int wallCount = 0;
            for (int i = x - 1; i <= x + 1; i ++) 
            {
                for (int j = y - 1; j <= y + 1; j ++) 
                {
                    if ((i >= 0) && (i <= width) && (j >= 0) && (j <= height)) 
                    {
                        if ((i != x) || (j != y)) 
                        {
                            wallCount += grid[i,j];
                        }
                    }
                    else 
                    {
                        wallCount ++;
                    }
                }
            }
            return wallCount;
        }
    }
}

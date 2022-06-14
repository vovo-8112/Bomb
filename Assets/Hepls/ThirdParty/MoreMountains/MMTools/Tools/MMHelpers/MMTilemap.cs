using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace MoreMountains.Tools
{
    public class MMTilemap : MonoBehaviour
    {
        public static Vector2 GetRandomPosition(Tilemap targetTilemap, Grid grid, int width, int height, bool shouldBeFilled = true, int maxIterations = 1000)
        {
            int iterationsCount = 0;
            Vector3Int randomCoordinate = Vector3Int.zero;
            
            while (iterationsCount < maxIterations)
            {
                randomCoordinate.x = UnityEngine.Random.Range(0, width);
                randomCoordinate.y = UnityEngine.Random.Range(0, height);
                randomCoordinate += MMTilemapGridRenderer.ComputeOffset(width-1, height-1);

                bool hasTile = targetTilemap.HasTile(randomCoordinate);
                if (hasTile == shouldBeFilled)
                {
                    return targetTilemap.CellToWorld(randomCoordinate) + (grid.cellSize / 2);
                }
                
                iterationsCount++;
            }

            return Vector2.zero;
        }
        public static Vector2 GetRandomPositionOnGround(Tilemap targetTilemap, Grid grid, int width, int height, int startingHeight, int xMin, int xMax, bool shouldBeFilled = true, int maxIterations = 1000)
        {
            int iterationsCount = 0;
            Vector3Int randomCoordinate = Vector3Int.zero;
            
            while (iterationsCount < maxIterations)
            {
                randomCoordinate.x = UnityEngine.Random.Range(xMin, xMax);
                randomCoordinate.y = startingHeight;
                randomCoordinate += MMTilemapGridRenderer.ComputeOffset(width-1, height-1);

                int counter = height;
                
                while (counter > 0)
                {
                    bool hasTile = targetTilemap.HasTile(randomCoordinate);
                    if (hasTile == shouldBeFilled)
                    {
                        randomCoordinate.y++;
                        return targetTilemap.CellToWorld(randomCoordinate) + (grid.cellSize / 2);
                    }

                    randomCoordinate.y--;
                    counter--;
                }

                iterationsCount++;
            }

            return Vector2.zero;
        }
    }
}

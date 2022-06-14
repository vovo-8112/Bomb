using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MoreMountains.Tools
{
    public class MMGridGeneratorPerlinNoiseGround : MMGridGenerator
    {
        public static int[,] Generate(int width, int height, float seed)
        {
            int[,] grid = PrepareGrid(ref width, ref height);
            
            for (int i = 0; i < width; i++)
            {
                int groundHeight = Mathf.FloorToInt((Mathf.PerlinNoise(i, seed) - 0.5f) * height) + (height/2);
                for (int j = groundHeight; j >= 0; j--)
                {
                    SetGridCoordinate(grid, i, j, 1);
                }
            }
            return grid;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public class MMGridGeneratorFull : MMGridGenerator 
    {
        public static int[,] Generate(int width, int height, bool full)
        {
            int[,] grid = PrepareGrid(ref width, ref height);
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetGridCoordinate(grid, i, j, full ? 1 : 0);
                }
            }
            return grid;
        } 
    }
}

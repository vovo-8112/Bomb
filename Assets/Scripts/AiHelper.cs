using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public static class AiHelper
    {
        public static List<Vector2> AddRangeSpawnBomb(Vector2 posBomb)
        {
            List<Vector2> ExploduesBombRange = new List<Vector2>();

            ExploduesBombRange.Add(posBomb);

            for (int i = 1; i < 4; i++)
            {
                ExploduesBombRange.Add(new Vector2(posBomb.x + i, posBomb.y));
            }

            for (int i = 1; i < 4; i++)
            {
                ExploduesBombRange.Add(new Vector2(posBomb.x, posBomb.y + i));
            }

            for (int i = 1; i < 4; i++)
            {
                ExploduesBombRange.Add(new Vector2(posBomb.x - i, posBomb.y));
            }

            for (int i = 1; i < 4; i++)
            {
                ExploduesBombRange.Add(new Vector2(posBomb.x, posBomb.y - i));
            }

            return ExploduesBombRange;
        }

      
    }
}
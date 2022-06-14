using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class ListExtensions
    {
        public static void MMSwap<T>(this IList<T> list, int i, int j)
        {
            T temporary = list[i];
            list[i] = list[j];
            list[j] = temporary;
        }
        public static void MMShuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.MMSwap(i, Random.Range(i, list.Count));
            }                
        }
    }
}


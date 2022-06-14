using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    public static class MMArrayExtensions
    {
        public static T MMRandomValue<T>(this T[] array)
        {
            int newIndex = Random.Range(0, array.Length);
            return array[newIndex];
        }
        public static T[] MMShuffle<T>(this T[] array)
        {
            for (int t = 0; t < array.Length; t++)
            {
                T tmp = array[t];
                int randomIndex = Random.Range(t, array.Length);
                array[t] = array[randomIndex];
                array[randomIndex] = tmp;
            }
            return array;
        }
    }
}

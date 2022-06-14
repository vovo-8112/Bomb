using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    public static class MMDictionaryExtensions
    {
        public static T KeyByValue<T, W>(this Dictionary<T, W> dictionary, T value)
        {
            T key = default;
            foreach (KeyValuePair<T, W> pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    break;
                }
            }
            return key;
        }
    }
}


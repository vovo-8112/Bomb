using System;
using System.Collections;
using UnityEngine;

namespace GameManager
{
    public class MapDate
    {
        public BitArray CratesDate;

        public static byte[] Serialize(object obj)
        {
            MapDate date = (MapDate)obj;
            byte[] dateInByte = new byte[Mathf.CeilToInt(129 / 8f)];
            date.CratesDate.CopyTo(dateInByte, 0);
            return dateInByte;
        }

        public static object DeSerialize(byte[] bytes)
        {
            MapDate date = new MapDate();

            byte[] mapBytes = new byte[Mathf.CeilToInt(129 / 8f)];
            Array.Copy(bytes, 0, mapBytes, 0, mapBytes.Length);
            date.CratesDate = new BitArray(mapBytes);
            return date;
        }
    }
}
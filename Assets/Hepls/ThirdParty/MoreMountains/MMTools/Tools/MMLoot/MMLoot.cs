using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace  MoreMountains.Tools
{
    public class MMLoot<T>
    {
        public T Loot;
        public float Weight = 1f;
        [MMReadOnly] 
        public float ChancePercentage;
        public float RangeFrom { get; set; }
        public float RangeTo { get; set; }
    }
    [System.Serializable]
    public class MMLootGameObject : MMLoot<GameObject> { }
    [System.Serializable]
    public class MMLootString : MMLoot<string> { }
    [System.Serializable]
    public class MMLootFloat : MMLoot<float> { }
    
}

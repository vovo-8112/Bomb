using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public static class LayermaskExtensions
    {
        public static bool MMContains(this LayerMask mask, int layer)
        {
            return ((mask.value & (1 << layer)) > 0);
        }
        public static bool MMContains(this LayerMask mask, GameObject gameobject)
        {
            return ((mask.value & (1 << gameobject.layer)) > 0);
        }
    }
}
